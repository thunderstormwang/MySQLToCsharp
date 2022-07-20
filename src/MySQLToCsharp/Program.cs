using Cocona;
using MySQLToCsharp.Parsers;
using MySQLToCsharp.TypeConverters;
using System;
using System.Linq;
using System.Text;

namespace MySQLToCsharp
{
    class Program
    {
        public static void Main(string[] args)
        {
            // 執行 batch 檔, 可建立 exe 檔
            // args = new string[] { "dir-to-markdown", "-i", "D:\\GeneratedSqlScripts\\payment" };
            // 原作者的寫法
            CoconaLiteApp.Run<QueryToCSharp>(args);
        }
    }

    public class QueryToCSharp
    {
        public static QueryToCsharpContext Context = QueryToCsharpContext.Current;
        const string defaultConverter = nameof(StandardConverter);

        [Command(Description = "Convert DDL sql query and generate C# class.")]
        public void Query(
            [Option('i', Description = "input mysql ddl query to parse")]string input,
            [Option('o', Description = "output directory path of generated C# class file")]string output,
            [Option('n', Description = "namespace to write")]string @namespace,
            [Option('c', Description = "converter name to use")]string converter = defaultConverter,
            [Option(Description = "true to ignore eol")]bool ignoreeol = true,
            [Option(Description = "true to add bom")]bool addbom = false,
            [Option(Description = "true to dry-run")]bool dry = false,
            [Option(Description = "executionid to detect execution")]string executionid = nameof(Query))
        {
            PrintDryMessage(dry);
            Console.WriteLine($"quey executed. Output Directory: {output}");

            var table = Parser.FromQuery(input);
            var resolvedConverter = TypeConverterResolver.Resolve(converter);
            var generator = new Generator(resolvedConverter, addbom, ignoreeol);

            var className = Generator.GetClassName(table.Name);
            var generated =generator.Generate(@namespace, className, table, resolvedConverter);
            generator.Save(className, generated, output, dry);

            QueryToCsharpContext.Current.AddLog(executionid, generated);
        }

        [Command(Description = "Convert DDL sql file and generate C# class.")]
        public void File(
            [Option('i', Description = "input file path to parse mysql ddl query")]string input,
            [Option('o', Description = "output directory path of generated C# class file")]string output,
            [Option('n', Description = "namespace to write")]string @namespace,
            [Option('c', Description = "converter name to use")]string converter = defaultConverter,
            [Option(Description = "true to ignore eol")]bool ignoreeol = true,
            [Option(Description = "true to add bom")]bool addbom = false,
            [Option(Description = "true to dry-run")]bool dry = false,
            [Option(Description = "executionid to detect execution")]string executionid = nameof(File))
        {
            PrintDryMessage(dry);
            Console.WriteLine($"file executed. Output Directory: {output}");

            var encoding = new UTF8Encoding(false);
            var table = Parser.FromFile(input, encoding);
            var resolvedConverter = TypeConverterResolver.Resolve(converter);
            var generator = new Generator(resolvedConverter, addbom, ignoreeol);

            var className = Generator.GetClassName(table.Name);
            var generated = generator.Generate(@namespace, className, table, resolvedConverter);
            generator.Save(className, generated, output, dry);

            QueryToCsharpContext.Current.AddLog(executionid, generated);
        }

        [Command(Description = "Convert DDL sql files in the folder and generate C# class.")]
        public void Dir(
            [Option('i', Description = "input folder path to parse mysql ddl query")]string input,
            [Option('o', Description = "output directory path of generated C# class files")]string output,
            [Option('n', Description = "namespace to write")]string @namespace,
            [Option('c', Description = "converter name to use")]string converter = defaultConverter,
            [Option(Description = "true to ignore eol")]bool ignoreeol = true,
            [Option(Description = "true to add bom")]bool addbom = false,
            [Option(Description = "true to dry-run")]bool dry = false,
            [Option(Description = "executionid to detect execution")]string executionid = nameof(Dir))
        {
            PrintDryMessage(dry);
            Console.WriteLine($"dir executed. Output Directory: {output}");
            var encoding = new UTF8Encoding(false);
            var tables = Parser.FromFolder(input, encoding).ToArray();
            var resolvedConverter = TypeConverterResolver.Resolve(converter);
            var generator = new Generator(resolvedConverter, addbom, ignoreeol);

            foreach (var table in tables)
            {
                var className = Generator.GetClassName(table.Name);
                var generated = generator.Generate(@namespace, className, table, resolvedConverter);
                generator.Save(className, generated, output, dry);

                QueryToCsharpContext.Current.AddLog(executionid, generated);
            }
        }

        [Command(Description = "Convert DDL sql files in the folder and generate Markdown.")]
        public void DirToMarkdown(
            [Option('i', Description = "input folder path to parse mysql ddl query, and each file in that folder must contain only one table")]string input,
            [Option(Description = "executionid to detect execution")]string executionid = nameof(DirToMarkdown))
        {
            var encoding = new UTF8Encoding(false);
            var tables = Parser.FromFolder(input,
                encoding).ToArray();
            
            Console.WriteLine($"### 目錄");
            Console.WriteLine($"| 編號 | Table名稱 | 用途 |");
            Console.WriteLine($"| -- | -- | -- |");
            var count = 1;
            foreach (var table in tables)
            {
                Console.WriteLine($"| {count} | {table.Name} | {table.Comment} |");
                count++;
            }
            Console.WriteLine($"");
            Console.WriteLine($"=================");
            Console.WriteLine($"");
            
            foreach (var table in tables)
            {
                PrintConsole(table);
            }
        }
        
        private void PrintConsole(MySqlTableDefinition mySqlTableDefinition)
        {
            Console.WriteLine($"### {mySqlTableDefinition.Name}");
            Console.WriteLine($"| 名稱 | 型態 | 用途 | 備註 |");
            Console.WriteLine($"| ---- | ---- | ---- | ---- |");
            foreach (var column in mySqlTableDefinition.Columns)
            {
                var columnType = column.Data.Length.HasValue && column.Data.Length > 0
                    ? $"{column.Data.DataType.ToLower()}({column.Data.Length})"
                    : $"{column.Data.DataType.ToLower()}";
                var description = string.Empty;
                var remark = string.Empty;

                if (!string.IsNullOrEmpty(column.Comment))
                {
                    var splitToken = column.Comment.Contains(Environment.NewLine) ? Environment.NewLine : "\\n";
                    var comments = column.Comment.Split(splitToken);
                    description = comments.FirstOrDefault();
                    if (comments.Length > 1)
                    {
                        remark = string.Join("<br/>",
                            comments.Skip(1));
                    }
                }

                Console.WriteLine($"| {column.Name} | {columnType} | {description} | {remark} |");
            }
            Console.WriteLine($"");
            Console.WriteLine($"=================");
            Console.WriteLine($"");
        }
        
        private void PrintDryMessage(bool dry)
        {
            if (dry)
            {
                Console.WriteLine($"[NOTE] dry run mode, {nameof(QueryToCSharp)} will not save to file.");
            }
        }
    }
}

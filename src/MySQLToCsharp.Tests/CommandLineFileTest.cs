﻿using Cocona;
using FluentAssertions;
using MySQLToCsharp.Internal;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace MySQLToCsharp.Tests
{
    public class CommandLineFileTest
    {
        [Fact]
        public void FileExecutionTest()
        {
            var dir = "file_hoge";
            var id = Guid.NewGuid().ToString();
            var args = new[] { "file", "-i", "test_data/simple/create_tables_simple.sql", "-o", dir, "-n", "Fuga", "--executionid", id};
            CoconaLiteApp.Create().Run<QueryToCSharp>(args);
            var expected = @"// ------------------------------------------------------------------------------
// <auto-generated>
// Code Generated by MySQLToCsharp
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fuga
{
    public partial class ships_gun
    {
        public int guns_id { get; set; }
        public int ship_id { get; set; }
    }
}
";
            var msg = QueryToCSharp.Context.GetLogs(id).First();
            msg.Should().Be(InternalUtils.NormalizeNewLines(expected));
        }

        [Fact]
        public void FileExecutionAnnotationTest()
        {
            var dir = "file_annotation";
            var id = Guid.NewGuid().ToString();
            var args = new[] { "file", "-i", "test_data/simple_annotation/create_tables_simple_annotation.sql", "-o", dir, "-n", "Fuga", "--executionid", id };
            CoconaLiteApp.Create().Run<QueryToCSharp>(args);
            var expected = @"// ------------------------------------------------------------------------------
// <auto-generated>
// Code Generated by MySQLToCsharp
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fuga
{
    public partial class quengine
    {
        public int id { get; set; }
        [Required]
        [StringLength(10)]
        public string class { get; set; }
        [Required]
        public byte[] data { get; set; }
    }
}
";
            var msg = QueryToCSharp.Context.GetLogs(id).First();
            msg.Should().Be(InternalUtils.NormalizeNewLines(expected));
        }

        [Fact]
        public void FileExecutionIgnoreEolWindowsTest()
        {
            var dir = "file_eol_windows";
            var id = Guid.NewGuid().ToString();
            var args = new[] { "file", "-i", "test_data/simple_eol/create_tables_simple_eol.sql", "-o", dir, "-n", "Fuga", "--executionid", id };
            CoconaLiteApp.Run<QueryToCSharp>(args);
            var expected = @"// ------------------------------------------------------------------------------
// <auto-generated>
// Code Generated by MySQLToCsharp
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fuga
{
    public partial class eol
    {
        public int guns_id { get; set; }
        public int ship_id { get; set; }
    }
}
";
            var msg = QueryToCSharp.Context.GetLogs(id).First();
            msg.Should().Be(InternalUtils.NormalizeNewLines(expected));
            QueryToCSharp.Context.Clear();

            // replace existing for windows test
            var replaced = File.ReadAllText($"{dir}/eol.cs").Replace("\r\n", "\n");
            File.WriteAllText($"{dir}/eol.cs", replaced);

            CoconaLiteApp.Run<QueryToCSharp>(args);
            File.ReadAllText($"{dir}/eol.cs").Should().Be(replaced);

            var ignoreeolArgs = new[] { "dir", "-i", "test_data/simple_eol/", "-o", dir, "-n", "Fuga", "--ignoreeol=false", "--executionid", id };
            CoconaLiteApp.Run<QueryToCSharp>(ignoreeolArgs);
            File.ReadAllText($"{dir}/eol.cs").Replace("\r\n", "\n").Should().Be(replaced);
        }

        [Fact]
        public void FileExecutionIgnoreEolLinuxTest()
        {
            var dir = "file_eol_linux";
            var id = Guid.NewGuid().ToString();
            var args = new[] { "file", "-i", "test_data/simple_eol/create_tables_simple_eol.sql", "-o", dir, "-n", "Fuga", "--executionid", id };
            CoconaLiteApp.Run<QueryToCSharp>(args);
            var expected = @"// ------------------------------------------------------------------------------
// <auto-generated>
// Code Generated by MySQLToCsharp
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fuga
{
    public partial class eol
    {
        public int guns_id { get; set; }
        public int ship_id { get; set; }
    }
}
";
            var msg = QueryToCSharp.Context.GetLogs(id).First();
            msg.Should().Be(InternalUtils.NormalizeNewLines(expected));
            QueryToCSharp.Context.Clear();

            // replace existing for windows test
            var replaced = File.ReadAllText($"{dir}/eol.cs").Replace("\r\n", "\n").Replace("\n", "\r\n");
            File.WriteAllText($"{dir}/eol.cs", replaced);

            CoconaLiteApp.Run<QueryToCSharp>(args);
            File.ReadAllText($"{dir}/eol.cs").Should().Be(replaced);

            var ignoreeolArgs = new[] { "dir", "-i", "test_data/simple_eol/", "-o", dir, "-n", "Fuga", "--ignoreeol=false", "--executionid", id };
            CoconaLiteApp.Run<QueryToCSharp>(ignoreeolArgs);
            File.ReadAllText($"{dir}/eol.cs").Replace("\r\n", "\n").Replace("\n", "\r\n").Should().Be(replaced);
        }
    }
}

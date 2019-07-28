CREATE TABLE BinaryData (
  `Id` INT NOT NULL AUTO_INCREMENT, 
  `Data` BINARY(32) NOT NULL, 
  `NullableData` BINARY(8) NULL, 
  `VarData` VARBINARY(128) NOT NULL, 
  `NullableVarData` VARBINARY(512) NULL, 
  PRIMARY KEY(`Id`)
);
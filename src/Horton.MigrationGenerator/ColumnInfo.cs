using System;
using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator
{
    public class ColumnInfo
    {
        public ColumnInfo(string name, string dataType)
        {
            Name = name;
            DataType = dataType;
        }

        public string Name { get; }
        public string DataType { get; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }

        public bool? IsUnicode { get; set; }
        public bool? IsFixedLength { get; set; }
        public int? MaxLength { get; set; }
        public bool IsMaxLength { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }

        public void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.Write("[");
            textWriter.Write(Name);
            textWriter.Write("] [");
            textWriter.Write(DataType);
            textWriter.Write("] ");
            textWriter.Write(PrintDefaultValue());
            textWriter.Write(PrintIdentity());
            textWriter.Write(PrintNull());
        }

        private string PrintDefaultValue()
        {
            // todo
            return "";
        }

        private string PrintIdentity()
        {
            return IsIdentity ? "IDENTITY " : "";
        }

        private string PrintNull()
        {
            return IsNullable ? "NULL " : "NOT NULL ";
        }
    }
}
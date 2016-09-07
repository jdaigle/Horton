using System.CodeDom.Compiler;
using System.Data.Entity.Core.Metadata.Edm;

namespace Horton.MigrationGenerator.DDL
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
        public string DefaultConstraintExpression { get; set; }

        public bool? IsUnicode { get; set; }
        public bool? IsFixedLength { get; set; }
        public int? MaxLength { get; set; }
        public bool IsMaxLength { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }

        public void AppendDDL(IndentedTextWriter textWriter, bool includeDefaultConstraints)
        {
            textWriter.Write(" [");
            textWriter.Write(Name);
            textWriter.Write("] [");
            textWriter.Write(DataType);
            textWriter.Write("]");
            textWriter.Write(PrintSize());
            textWriter.Write(PrintDefaultValue());
            textWriter.Write(PrintNull());
            if (includeDefaultConstraints)
            {
                textWriter.Write(PrintDefaultConstraints());
            }
            textWriter.Write(PrintIdentity());
        }

        private string PrintSize()
        {
            if (IsMaxLength)
            {
                return "(max)";
            }
            else if (MaxLength.HasValue)
            {
                return "(" + MaxLength.Value + ")";
            }
            else if (Precision.HasValue && Scale.HasValue)
            {
                return "(" + Precision.Value + "," + Scale.Value + ")";
            }
            else if (Precision.HasValue)
            {
                return "(" + Precision.Value + ")";
            }
            else if (Scale.HasValue)
            {
                return "(" + Scale.Value + ")";
            }
            return "";
        }

        private string PrintDefaultValue()
        {
            // todo
            return "";
        }

        private string PrintIdentity()
        {
            return IsIdentity ? " IDENTITY" : "";
        }

        private string PrintNull()
        {
            return IsNullable ? " NULL" : " NOT NULL";
        }

        private string PrintDefaultConstraints()
        {
            if (DefaultConstraintExpression == null)
            {
                return "";
            }

            return " " + DefaultConstraintExpression;
        }

        internal static ColumnInfo FromEF6(EdmProperty property, string tableName)
        {
            var typeName = property.TypeName;

            var isMaxLen = false;
            // Special case: the EDM treats 'nvarchar(max)' as a type name, but SQL Server treats
            // it as a type 'nvarchar' and a type qualifier.
            const string maxSuffix = "(max)";
            if (typeName.EndsWith(maxSuffix))
            {
                typeName = typeName.Substring(0, typeName.Length - maxSuffix.Length);
                isMaxLen = true;
            }

            var column = new ColumnInfo(property.Name, typeName)
            {
                IsNullable = property.Nullable,
                IsMaxLength = isMaxLen,
                IsUnicode = property.IsUnicode == true,
                IsIdentity = property.IsStoreGeneratedIdentity && typeName != "uniqueidentifier",
                IsFixedLength = property.IsFixedLength == true,
                MaxLength = property.IsMaxLengthConstant ? null : property.MaxLength,
                Scale = property.IsScaleConstant ? null : property.Scale,
                Precision = property.IsMaxLengthConstant ? null : property.Precision,
            };

            // Special case: EDM can say a uniqueidentifier is "identity", but it
            // really means that there is a default constraint on the table.
            if (property.IsStoreGeneratedIdentity && typeName == "uniqueidentifier")
            {
                column.IsIdentity = false;
                column.DefaultConstraintExpression = "CONSTRAINT DF_" + tableName + "_" + column.Name + " DEFAULT NEWID()";
            }

            // Special case: EDM gives "time" a Precision value, but in SQL it's actually Scale
            if (typeName == "time")
            {
                column.Scale = column.Precision;
                column.Precision = null;
            }

            // TODO: detect "rowversion" data types

            return column;
        }
    }
}
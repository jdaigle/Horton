using System.CodeDom.Compiler;
using System.Data.Entity.Core.Metadata.Edm;
using Horton.MigrationGenerator.Sys;

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
        public bool IsRowGuid { get; set; }
        public string DefaultConstraintExpression { get; set; }
        public string CheckConstraintExpression { get; set; }

        public int? MaxLength { get; set; }
        public bool IsMaxLength { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }

        public bool IsComputed { get; set; }
        public string ComputedColumnDefinitionExpression { get; set; }
        public bool IsPersisted { get; set; }

        public void AppendDDL(IndentedTextWriter textWriter, bool includeConstraints)
        {
            if (IsComputed)
            {
                textWriter.Write("[");
                textWriter.Write(Name);
                textWriter.Write("] AS ");
                textWriter.Write(ComputedColumnDefinitionExpression);
                if (IsPersisted)
                {
                    textWriter.Write(" PERSISTED");
                    textWriter.Write(PrintNull());
                }
                return;
            }

            textWriter.Write("[");
            textWriter.Write(Name);
            textWriter.Write("] [");
            textWriter.Write(DataType);
            textWriter.Write("]");
            textWriter.Write(PrintSize());
            textWriter.Write(PrintDefaultValue());
            textWriter.Write(PrintNull());
            textWriter.Write(PrintRowGuid());
            if (includeConstraints)
            {
                textWriter.Write(PrintDefaultConstraints());
            }
            textWriter.Write(PrintIdentity());
            if (includeConstraints)
            {
                textWriter.Write(PrintCheckConstraints());
            }
        }

        private string PrintRowGuid()
        {
            if (IsRowGuid)
            {
                return " ROWGUIDCOL";
            }
            return "";
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

        private string PrintCheckConstraints()
        {
            if (CheckConstraintExpression == null)
            {
                return "";
            }

            return " " + CheckConstraintExpression;
        }

        private void FixColumnType()
        {
            switch (DataType)
            {
                case "bit":
                case "tinyint":
                case "smallint":
                case "int":
                case "bigint":
                case "smalldatetime":
                case "datetime":
                case "date":
                case "sysname":
                case "smallmoney":
                case "money":
                case "uniqueidentifier":
                case "timestamp":
                case "rowversion":
                case "hierarchyid":
                case "geometry":
                case "geography":
                case "xml":
                    IsMaxLength = false;
                    MaxLength = null;
                    Scale = null;
                    Precision = null;
                    break;
                case "datetime2":
                case "datetimeoffset":
                case "time":
                    IsMaxLength = false;
                    MaxLength = null;
                    Precision = null;
                    break;
                case "nvarchar":
                case "nchar":
                case "ntext":
                    Precision = null;
                    Scale = null;
                    break;
                case "varchar":
                case "char":
                case "binary":
                case "varbinary":
                case "image":
                case "text":
                    Precision = null;
                    Scale = null;
                    break;
                case "decimal":
                case "numeric":
                    IsMaxLength = false;
                    MaxLength = null;
                    break;
                case "float":
                case "real":
                    IsMaxLength = false;
                    MaxLength = null;
                    Scale = null;
                    break;
            }
        }

        internal static ColumnInfo FromSQL(Column column)
        {
            var columnInfo = new ColumnInfo(column.Name, column.TypeName)
            {
                IsNullable = column.is_nullable,
                IsMaxLength = column.max_length == -1,
                IsIdentity = column.is_identity,
                MaxLength = column.max_length,
                Scale = column.scale,
                Precision = column.precision,
                IsRowGuid = column.is_rowguidcol,
                IsComputed = column.is_computed,
                ComputedColumnDefinitionExpression = column.computed_column_definition,
                IsPersisted = column.is_persisted,
            };

            columnInfo.FixColumnType();

            // Special case: SQL storage size for unicode is 2x size
            switch (columnInfo.DataType)
            {
                case "nvarchar":
                case "nchar":
                case "ntext":
                    columnInfo.MaxLength = columnInfo.MaxLength / 2;
                    break;
            }

            if (column.is_user_defined)
            {
                columnInfo.MaxLength = null;
                columnInfo.Scale = null;
                columnInfo.Precision = null;
            }

            if (column.default_object_id > 0)
            {
                columnInfo.DefaultConstraintExpression = "DEFAULT " + column.default_definition;
                if (!column.default_is_system_named)
                {
                    columnInfo.DefaultConstraintExpression = "CONSTRAINT " + column.default_name + " " + columnInfo.DefaultConstraintExpression;
                }
            }

            if (!string.IsNullOrEmpty(column.check_constraint_definition))
            {
                columnInfo.CheckConstraintExpression = "CHECK " + column.check_constraint_definition;
                if (!column.check_constraint_is_system_named)
                {
                    columnInfo.CheckConstraintExpression = "CONSTRAINT " + column.check_constraint_name + " " + columnInfo.CheckConstraintExpression;
                }
            }

            return columnInfo;
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

            var columnInfo = new ColumnInfo(property.Name, typeName)
            {
                IsNullable = property.Nullable,
                IsMaxLength = isMaxLen,
                IsIdentity = property.IsStoreGeneratedIdentity && typeName != "uniqueidentifier",
                MaxLength = property.IsMaxLengthConstant ? null : property.MaxLength,
                Scale = property.IsScaleConstant ? null : property.Scale,
                Precision = property.IsMaxLengthConstant ? null : property.Precision,
            };

            // Special case: EDM can say a uniqueidentifier is "identity", but it
            // really means that there is a default constraint on the table.
            if (property.IsStoreGeneratedIdentity && typeName == "uniqueidentifier")
            {
                columnInfo.IsIdentity = false;
                columnInfo.DefaultConstraintExpression = "CONSTRAINT DF_" + tableName + "_" + columnInfo.Name + " DEFAULT NEWID()";
            }

            // Special case: EDM gives "time" a Precision value, but in SQL it's actually Scale
            if (typeName == "time")
            {
                columnInfo.Scale = columnInfo.Precision;
                columnInfo.Precision = null;
            }

            columnInfo.FixColumnType();

            // TODO: detect "rowversion" data types

            return columnInfo;
        }
    }
}
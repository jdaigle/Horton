using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace AdventureWorks2008R2.EntityFramework.Person
{
    public sealed class BusinessEntity
    {
        public int BusinessEntityId { get; set; }
        public Guid RowGuid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Person Person { get; set; }

        internal sealed class Mapping : EntityTypeConfiguration<BusinessEntity>
        {
            public Mapping()
            {
                ToTable("BusinessEntity", "Person");
                HasKey(c => c.BusinessEntityId);
                Property(c => c.BusinessEntityId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                HasOptional(c => c.Person).WithRequired(c => c.BusinessEntity);
            }
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace AdventureWorks2008R2.EntityFramework.Person
{
    public class Person
    {
        public int BusinessEntityId { get; set; }
        public virtual BusinessEntity BusinessEntity { get; set; }

        public string PersonType { get; set; }
        public bool NameStyle { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public int EmailPromotion { get; set; }
        public string AdditionalContactInfo { get; set; }
        public string Demographics { get; set; }
        public Guid rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        internal sealed class Mapping : EntityTypeConfiguration<Person>
        {
            public Mapping()
            {
                ToTable("Person", "Person");

                HasKey(c => c.BusinessEntityId);

                //HasRequired(c => c.BusinessEntity).WithMany().HasForeignKey(c => c.BusinessEntityId);
                Property(c => c.PersonType).HasMaxLength(2).IsFixedLength().IsRequired();
                Property(c => c.Title).HasMaxLength(8).IsOptional();
                Property(c => c.FirstName).HasMaxLength(50).IsRequired();
                Property(c => c.MiddleName).HasMaxLength(50).IsOptional();
                Property(c => c.LastName).HasMaxLength(50).IsRequired();
                Property(c => c.Suffix).HasMaxLength(10).IsOptional();
                Property(c => c.AdditionalContactInfo).HasColumnType("xml");
                Property(c => c.Demographics).HasColumnType("xml");
            }
        }
    }
}

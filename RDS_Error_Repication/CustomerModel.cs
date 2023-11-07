using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RDS_Error_Repication
{
    public class CustomerModel
    {
        public string UUID { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string? Surname { get; set; }
        public DateTime Created { get; set; }

        public virtual ContactInformationModel ContactInformation { get; set; }
        public virtual POPIInfoModel PopiInfo { get; set; }
        public virtual List<CustomerInterestBridge> Interests { get; set; }
        public virtual List<FavouriteStoreBridge> Stores { get; set; }
        public virtual List<FavouriteProductBridge> Products { get; set; }

        public CustomerModel()
        {

        }
    }

    public class CustomerModelConfiguration : IEntityTypeConfiguration<CustomerModel>
    {


        public void Configure(EntityTypeBuilder<CustomerModel> builder)
        {
            builder.ToTable("Customer");

            builder.HasKey(c => c.UUID);

            builder.Property(c => c.UUID).HasMaxLength(50);
            builder.Property(c => c.ExternalId).HasMaxLength(100).IsRequired();

            builder.Property(c => c.Name)
            .HasMaxLength(50)
            .IsRequired();

            builder.Property(c => c.Surname)
            .HasMaxLength(50);

            builder.HasOne(c => c.PopiInfo).WithOne(ci => ci.Customer).HasForeignKey<POPIInfoModel>(x => x.CreatedBy);

            builder.HasOne(c => c.ContactInformation).WithOne(ci => ci.Customer).HasForeignKey<ContactInformationModel>(c => c.CustomerId);

            builder.HasMany(c => c.Stores).WithOne(i => i.Customer).HasForeignKey(b => b.CustomerId);
            builder.HasMany(c => c.Products).WithOne(i => i.Customer).HasForeignKey(b => b.CustomerId);



            builder
            .HasMany(c => c.Interests)
            .WithOne(i => i.Customer)
            .HasForeignKey(b => b.CustomerUUID);

            builder.Navigation(customer => customer.PopiInfo).AutoInclude();
            builder.Navigation(customer => customer.ContactInformation).AutoInclude();
            builder.Navigation(customer => customer.Stores).AutoInclude();
            builder.Navigation(customer => customer.Products).AutoInclude();
            builder.Navigation(customer => customer.Interests).AutoInclude();

        }


    }
}

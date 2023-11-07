using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RDS_Error_Repication
{
    public class ContactInformationModelConfiguration : IEntityTypeConfiguration<ContactInformationModel>
    {
        public void Configure(EntityTypeBuilder<ContactInformationModel> builder)
        {
            builder.ToTable("Contact_Information");
            builder.HasKey(p => p.Id);

            var EmailAddressConverter = new ValueConverter<EmailAddress, string>
            (e => e.ToString(), s => EmailAddress.From(s));
            var CellConverter = new ValueConverter<CellNumber, string>
            (c => c.ToString(), s => CellNumber.From(s));

            builder.Property(p => p.Id).HasMaxLength(50);
            builder.Property(ci => ci.CellNumber).HasColumnName("Cell");
            builder.Property(ci => ci.Email).HasColumnName("Email");
            builder.HasOne(c => c.Customer);
        }

        public class PopiInfoModelConfiguration : IEntityTypeConfiguration<POPIInfoModel>
        {


            public void Configure(EntityTypeBuilder<POPIInfoModel> builder)
            {
                builder.ToTable("Popi_Info");
                builder.HasKey(p => p.Id);
                builder.Property(p => p.Id).HasMaxLength(50);
                builder.Property(p => p.CreatedBy).HasMaxLength(50);
                builder.Property(p => p.LastModifiedBy).HasMaxLength(50);
                builder.HasOne(c => c.Customer);
            }


        }
    }

    public class CustomerInterestBridgeConfiguration : IEntityTypeConfiguration<CustomerInterestBridge>
    {


        public void Configure(EntityTypeBuilder<CustomerInterestBridge> builder)
        {
            builder.ToTable("CustomerInterests");

            builder.HasKey(interest => new { interest.CategoryId, interest.CustomerUUID });

            builder.Property(s => s.CategoryId).HasMaxLength(100);

            builder
            .HasOne(pt => pt.Customer)
            .WithMany(c => c.Interests)
            .HasForeignKey(b => b.CustomerUUID);

            builder.Ignore(s => s.Category);

        }
    }


    public class FavouriteProductBridgeConfiguration : IEntityTypeConfiguration<FavouriteProductBridge>
    {


        public void Configure(EntityTypeBuilder<FavouriteProductBridge> builder)
        {
            builder.ToTable("Customer_Favourite_Products");
            builder.HasKey(p => new
            {
                p.CustomerId,
                p.StoreId,
                p.ProductId,
                p.MallId
            });
            builder.Property(p => p.CustomerId).IsRequired().HasMaxLength(70);
            builder.Property(p => p.StoreId).IsRequired().HasMaxLength(70);
            builder.Property(p => p.ProductId).IsRequired().HasMaxLength(70);
            builder.Property(p => p.MallId).IsRequired().HasMaxLength(70);
            builder.Property(p => p.Weight).HasDefaultValue(1);

            builder.HasOne(c => c.Customer);
        }
    }

    public class FavouriteStoreBridgeConfiguration : IEntityTypeConfiguration<FavouriteStoreBridge>
    {


        public void Configure(EntityTypeBuilder<FavouriteStoreBridge> builder)
        {
            builder.ToTable("Customer_Favourite_Stores");
            builder.HasKey(p => new
            {
                p.CustomerId,
                p.StoreId,
                p.MallId
            });
            builder.Property(p => p.CustomerId).IsRequired().HasMaxLength(70);
            builder.Property(p => p.StoreId).IsRequired().HasMaxLength(70);
            builder.Property(p => p.MallId).IsRequired().HasMaxLength(70);
            builder.Property(p => p.Weight).HasDefaultValue(1);

            builder.HasOne(c => c.Customer);
        }
    }
}

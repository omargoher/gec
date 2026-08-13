using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public class AddressEntityTypeConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label)
            .HasConversion<string>() // Converts Enum <-> string (e.g., "Home", "Work")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.ReceiverName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Governorate)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Building)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Apartment)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Landmark)
            .HasMaxLength(200);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(20);

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.Property(a => a.Latitude)
            .HasPrecision(9, 6);

        builder.Property(a => a.Longitude)
            .HasPrecision(9, 6);

        builder.Property(a => a.IsDefault)
            .IsRequired();

        builder.HasIndex(a => a.CustomerId)
            .IsUnique()
            .HasDatabaseName("IX_Addresses_OneDefaultPerCustomer")
            .HasFilter("\"is_default\" = TRUE");

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
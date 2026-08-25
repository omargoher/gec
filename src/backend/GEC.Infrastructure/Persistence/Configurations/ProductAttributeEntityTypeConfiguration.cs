using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GEC.Domain.Entities;

namespace GEC.Infrastructure.Persistence.Configurations;

public class ProductAttributeEntityTypeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("product_attributes");

        builder.HasKey(pa => new { pa.ProductId, pa.AttributeId });

        builder.Property(pa => pa.IsRequired)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(pa => pa.Attribute)
            .WithMany(a => a.ProductAttributes)
            .HasForeignKey(pa => pa.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

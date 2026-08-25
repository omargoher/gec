using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GEC.Domain.Entities;

namespace GEC.Infrastructure.Persistence.Configurations;

public class VariantAttributeValueEntityTypeConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder.ToTable("variant_attribute_values");

        builder.HasKey(v => new { v.VariantId, v.AttributeId });

        builder.HasOne(v => v.Variant)
            .WithMany(v => v.AttributeValues)
            .HasForeignKey(v => v.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.AttributeValue)
            .WithMany(v => v.VariantAttributeValues)
            .HasForeignKey(v => new { v.AttributeId, v.AttributeValueId })
            .HasPrincipalKey(v => new { v.AttributeId, v.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductAttribute>()
            .WithMany()
            .HasForeignKey(v => new { v.ProductId, v.AttributeId })
            .HasPrincipalKey(pa => new { pa.ProductId, pa.AttributeId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.VariantId, v.AttributeValueId })
            .IsUnique()
            .HasDatabaseName("ix_variant_attribute_values_variant_value");

        builder.HasIndex(v => new { v.ProductId, v.AttributeId })
            .HasDatabaseName("ix_variant_attribute_values_product_id_attribute_id");
    }
}

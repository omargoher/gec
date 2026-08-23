using GEC.Domain.Entities;
using GEC.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public class ProductVariantEntityTypeConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants", t => t.HasCheckConstraint("ck_product_variants_price_non_negative", "price_amount >= 0"));

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Sku)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(v => v.PriceAmount)
            .HasColumnName("price_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(v => v.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.VariantSignature)
            .HasColumnName("variant_signature")
            .HasMaxLength(2000)
            .IsRequired();

        // Replaces obsolete UseXminAsConcurrencyToken()
        builder.Property<uint>("xmin")
            .IsRowVersion();

        builder.HasIndex(v => v.Sku)
            .IsUnique()
            .HasDatabaseName("ix_product_variants_sku");

        builder.HasIndex(v => v.ProductId)
            .HasDatabaseName("ix_product_variants_product_id");

        builder.HasIndex(v => new { v.ProductId, v.VariantSignature })
            .IsUnique()
            .HasDatabaseName("ix_product_variants_product_id_signature");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GEC.Domain.Entities;

namespace GEC.Infrastructure.Persistence.Configurations;

public class ProductSpecificationEntityTypeConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("product_specifications");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(s => s.ProductId)
            .HasDatabaseName("ix_product_specifications_product_id");
    }
}

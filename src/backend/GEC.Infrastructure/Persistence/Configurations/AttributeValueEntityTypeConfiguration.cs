using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GEC.Domain.Entities;

namespace GEC.Infrastructure.Persistence.Configurations;

public class AttributeValueEntityTypeConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("attribute_values");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Value)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(v => new { v.AttributeId, v.Value })
            .IsUnique()
            .HasDatabaseName("ix_attribute_values_attribute_id_value");

        builder.HasAlternateKey(v => new { v.AttributeId, v.Id });
    }
}

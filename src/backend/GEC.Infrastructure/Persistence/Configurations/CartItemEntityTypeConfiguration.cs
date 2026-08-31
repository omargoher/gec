using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public class CartItemEntityTypeConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CartId).IsRequired();
        builder.Property(x => x.VariantId).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();

        builder.HasIndex(x => new { x.CartId, x.VariantId }).IsUnique();

        builder.HasOne(x => x.Cart)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

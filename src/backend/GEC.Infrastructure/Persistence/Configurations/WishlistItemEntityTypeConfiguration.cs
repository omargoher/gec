using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public sealed class WishlistItemEntityTypeConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.AddedAt)
            .IsRequired();

        // Local FK — WishlistItem → Wishlist (same database, cascade delete).
        builder.HasOne(x => x.Wishlist)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ProductId)
            .IsRequired();

        // No HasOne(x => x.Product) — Product is owned by the Catalog service.
        // ProductId is a plain scalar reference, not a local FK.

        // Prevents duplicate favourites; also used by the composite-unique-check
        // in FavouriteService.IsDuplicateFavourite to identify the constraint name.
        builder.HasIndex(x => new { x.WishlistId, x.ProductId })
            .IsUnique()
            .HasDatabaseName("ix_wishlist_items_wishlist_id_product_id");
    }
}

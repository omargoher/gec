using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public sealed class WishlistEntityTypeConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("wishlists");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        // One wishlist per customer — enforced at DB level.
        builder.HasIndex(x => x.CustomerId)
            .IsUnique()
            .HasDatabaseName("ix_wishlists_customer_id");

        // No HasOne(x => x.Customer) — Customer is owned by the
        // Identity/Customer service. CustomerId is a plain scalar reference,
        // validated only by the fact that it comes from a verified JWT.
    }
}

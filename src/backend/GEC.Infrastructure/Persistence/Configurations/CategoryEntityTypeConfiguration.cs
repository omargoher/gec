using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(255);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Symbol).IsRequired();
        builder.Property(e => e.Exchange).IsRequired();
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Currency).IsRequired();

        builder.HasIndex(e => new { e.Symbol, e.Exchange }).IsUnique();
    }
}

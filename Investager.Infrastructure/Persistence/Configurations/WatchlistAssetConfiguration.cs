using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations;

public class WatchlistAssetConfiguration : IEntityTypeConfiguration<WatchlistAsset>
{
    public void Configure(EntityTypeBuilder<WatchlistAsset> builder)
    {
        builder.HasKey(e => new { e.WatchlistId, e.AssetId });

        builder.Property(e => e.DisplayOrder).IsRequired();
    }
}

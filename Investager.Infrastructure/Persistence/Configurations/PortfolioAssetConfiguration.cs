using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations
{
    public class PortfolioAssetConfiguration : IEntityTypeConfiguration<PortfolioAsset>
    {
        public void Configure(EntityTypeBuilder<PortfolioAsset> builder)
        {
            builder.HasKey(e => new { e.PortfolioId, e.AssetId });
        }
    }
}

using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence
{
    public class AssetPriceConfiguration : IEntityTypeConfiguration<AssetPrice>
    {
        public void Configure(EntityTypeBuilder<AssetPrice> builder)
        {
            builder.HasNoKey();
            builder.Property(e => e.Time).IsRequired();
            builder.Property(e => e.Key).IsRequired();
            builder.Property(e => e.Price).IsRequired();
        }
    }
}

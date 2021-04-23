using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence
{
    public class AssetPriceConfiguration : IEntityTypeConfiguration<TimeSeriesPoint>
    {
        public void Configure(EntityTypeBuilder<TimeSeriesPoint> builder)
        {
            builder.HasNoKey();
            builder.Property(e => e.Time).IsRequired();
            builder.Property(e => e.Key).IsRequired();
            builder.Property(e => e.Value).IsRequired();
        }
    }
}

using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations;

public class WatchlistCurrencyPairConfiguration : IEntityTypeConfiguration<WatchlistCurrencyPair>
{
    public void Configure(EntityTypeBuilder<WatchlistCurrencyPair> builder)
    {
        builder.HasKey(e => new { e.WatchlistId, e.FirstCurrencyId, e.SecondCurrencyId });

        builder.Property(e => e.DisplayOrder).IsRequired();
    }
}

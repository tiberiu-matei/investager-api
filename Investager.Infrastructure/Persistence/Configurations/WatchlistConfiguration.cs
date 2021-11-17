using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations
{
    public class WatchlistConfiguration : IEntityTypeConfiguration<Watchlist>
    {
        public void Configure(EntityTypeBuilder<Watchlist> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasMany(e => e.Assets).WithOne(e => e.Watchlist).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.CurrencyPairs).WithOne(e => e.Watchlist).OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Name).IsRequired();
        }
    }
}

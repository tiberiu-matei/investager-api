using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations
{
    public class CurrencyPairConfiguration : IEntityTypeConfiguration<CurrencyPair>
    {
        public void Configure(EntityTypeBuilder<CurrencyPair> builder)
        {
            builder.HasKey(e => new { e.FirstCurrencyId, e.SecondCurrencyId });

            builder.Property(e => e.HasTimeData).IsRequired();
        }
    }
}

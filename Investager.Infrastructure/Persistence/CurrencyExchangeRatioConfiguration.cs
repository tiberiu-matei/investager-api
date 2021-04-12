using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence
{
    public class CurrencyExchangeRatioConfiguration : IEntityTypeConfiguration<CurrencyExchangeRatio>
    {
        public void Configure(EntityTypeBuilder<CurrencyExchangeRatio> builder)
        {
            builder.HasNoKey();
            builder.Property(e => e.Time).IsRequired();
            builder.Property(e => e.Key).IsRequired();
            builder.Property(e => e.Ratio).IsRequired();
        }
    }
}

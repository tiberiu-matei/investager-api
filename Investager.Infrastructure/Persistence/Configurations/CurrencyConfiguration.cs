using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Code).IsRequired();
            builder.Property(e => e.Name).IsRequired();
            builder.Property(e => e.Type).IsRequired();

            builder.HasIndex(e => e.Code).IsUnique();
        }
    }
}

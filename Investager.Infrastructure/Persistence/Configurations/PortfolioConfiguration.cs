using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations
{
    public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
    {
        public void Configure(EntityTypeBuilder<Portfolio> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired();
            builder.Property(e => e.Name).IsRequired();
            builder.HasMany(e => e.PortfolioAssets).WithOne(e => e.Portfolio).OnDelete(DeleteBehavior.Cascade);
        }
    }
}

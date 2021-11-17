using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Investager.Infrastructure.Persistence.Configurations
{
    public class TimeSeriesPointConfiguration : IEntityTypeConfiguration<TimeSeriesPoint>
    {
        public void Configure(EntityTypeBuilder<TimeSeriesPoint> builder)
        {
            builder.HasNoKey();

            builder.ToTable("TimeSeriesPoint");

            builder.Property(e => e.Time).IsRequired().HasConversion(e => e, e => DateTime.SpecifyKind(e, DateTimeKind.Utc)); ;
            builder.Property(e => e.Key).IsRequired();
            builder.Property(e => e.Value).IsRequired();
        }
    }
}

using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasMany(e => e.Watchlists).WithOne(e => e.User).OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.Email).IsRequired();
        builder.Property(e => e.DisplayEmail).IsRequired();
        builder.Property(e => e.DisplayName).IsRequired();
        builder.Property(e => e.Theme).IsRequired().HasDefaultValue(Theme.None);
        builder.Property(e => e.PasswordSalt).IsRequired();
        builder.Property(e => e.PasswordHash).IsRequired();

        builder.HasIndex(e => e.Email).IsUnique();
    }
}

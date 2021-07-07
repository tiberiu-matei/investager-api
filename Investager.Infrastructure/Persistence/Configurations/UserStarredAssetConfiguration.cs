using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investager.Infrastructure.Persistence.Configurations
{
    public class UserStarredAssetConfiguration : IEntityTypeConfiguration<UserStarredAsset>
    {
        public void Configure(EntityTypeBuilder<UserStarredAsset> builder)
        {
            builder.HasKey(e => new { e.UserId, e.AssetId });
        }
    }
}

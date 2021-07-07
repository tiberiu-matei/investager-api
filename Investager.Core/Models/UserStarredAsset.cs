namespace Investager.Core.Models
{
    public class UserStarredAsset
    {
        public int UserId { get; set; }

        public User User { get; set; }

        public int AssetId { get; set; }

        public Asset Asset { get; set; }

        public int DisplayOrder { get; set; }
    }
}

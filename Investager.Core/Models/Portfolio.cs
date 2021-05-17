using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class Portfolio
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public User User { get; set; }

        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}

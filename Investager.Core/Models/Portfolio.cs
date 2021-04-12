using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class Portfolio
    {
        public int Id { get; set; } = default!;

        public string Name { get; set; } = default!;

        public User User { get; set; } = default!;

        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}

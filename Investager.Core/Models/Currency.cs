namespace Investager.Core.Models
{
    public class Currency
    {
        public int Id { get; set; }

        public string Code { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;
    }
}

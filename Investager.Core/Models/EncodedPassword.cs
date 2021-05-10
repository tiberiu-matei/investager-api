namespace Investager.Core.Models
{
    public class EncodedPassword
    {
        public byte[] Salt { get; set; } = default!;

        public byte[] Hash { get; set; } = default!;
    }
}

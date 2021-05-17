namespace Investager.Core.Models
{
    public class EncodedPassword
    {
        public byte[] Hash { get; set; }

        public byte[] Salt { get; set; }
    }
}

namespace Investager.Core.Models
{
    public class EncodedPassword
    {
        public byte[] Salt { get; set; }

        public byte[] Hash { get; set; }
    }
}

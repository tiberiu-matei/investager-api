using Investager.Core.Models;

namespace Investager.Core.Interfaces
{
    public interface IPasswordHelper
    {
        EncodedPassword EncodePassword(string password);

        bool IsPasswordValid(string password, EncodedPassword encodedPassword);
    }
}

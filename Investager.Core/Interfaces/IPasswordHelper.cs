using Investager.Core.Models;

namespace Investager.Core.Interfaces;

public interface IPasswordHelper
{
    EncodedPassword EncodePassword(string password);

    bool IsPasswordCorrect(string password, byte[] hash, byte[] salt);
}

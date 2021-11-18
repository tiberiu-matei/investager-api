using Investager.Core.Models;

namespace Investager.Core.Dtos;

public class UserDto
{
    public string Email { get; set; }

    public string DisplayName { get; set; }

    public Theme Theme { get; set; }
}

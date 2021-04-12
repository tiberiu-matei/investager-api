namespace Investager.Core.Dtos
{
    public class RegisterUserDto
    {
        public string Email { get; set; } = default!;

        public string Password { get; set; } = default!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }
}

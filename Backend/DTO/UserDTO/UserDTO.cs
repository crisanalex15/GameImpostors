namespace Backend.DTO.UserDTO
{
    public class RegisterRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }

    public class LoginRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RefreshTokenRequestDTO
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class VerifyEmailRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class ForgotPasswordRequestDTO
    {
        public string Email { get; set; } = null!;
    }

    public class VerifyPasswordResetRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class VerifyResetCodeRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class ChangePasswordRequestDTO
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class UpdateUsernameRequestDTO
    {
        public string Username { get; set; } = null!;
    }

    public class UserProfileRequestDTO
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool IsEmailVerified { get; set; }
        public DateTime LastModifiedAt { get; set; }

    }
}
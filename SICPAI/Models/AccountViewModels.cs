namespace SICPAI.Models
{
    public class LoginViewModel
    {
        public LoginViewModel() { }
        public LoginViewModel(string? returnUrl)
        {
            ReturnUrl = returnUrl;
        }

        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

namespace Core.ApplicationModels.KironTestAPI
{
    public class RegisterNewUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; }
    }
}

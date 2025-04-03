namespace GameAssetStorage.Models
{
    public class UserLoginModel
    {
        public string Username { get; set; } = string.Empty; // Matches User.Username
        public string Password { get; set; } = string.Empty;
    }
}
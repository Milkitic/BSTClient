namespace BSTClient.API.Models
{
    public class AuthenticateModel
    {
        public AuthenticateModel(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
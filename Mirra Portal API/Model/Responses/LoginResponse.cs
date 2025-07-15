namespace Mirra_Portal_API.Model.Responses
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Token Token { get; set; }

    }
}

namespace Mirra_Portal_API.Model.Responses
{
    public class ActivateEmailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Token Token { get; set; }
    }
}

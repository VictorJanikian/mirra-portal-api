namespace Mirra_Portal_API.Model
{
    public class Token
    {
        public string Value { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateExpiration { get; set; }

        public Token()
        {
            DateCreated = DateTime.Now;
        }

    }
}

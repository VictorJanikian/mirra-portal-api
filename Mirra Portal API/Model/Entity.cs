namespace Mirra_Portal_API.Model
{
    public class Entity
    {
        public int Id { get; set; }

        public Entity SetId(int id)
        {
            Id = id;
            return this;
        }
    }
}

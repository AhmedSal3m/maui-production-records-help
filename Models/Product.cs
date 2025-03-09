using SQLite;

namespace PlasticQC.Models
{
    [Table("Products")]
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50), Unique]
        public string ProductNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CreatedById { get; set; }
    }
}
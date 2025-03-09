using SQLite;

namespace PlasticQC.Models
{
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(50), Unique]
        public string Username { get; set; }

        [MaxLength(100)]
        public string Password { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
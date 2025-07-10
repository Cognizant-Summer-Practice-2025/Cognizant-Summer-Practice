using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }
}

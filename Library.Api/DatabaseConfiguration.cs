using System.ComponentModel.DataAnnotations;

namespace Library.Api
{
    public class DatabaseConfiguration
    {
        public const string SectionName = "Database";

        [Required]
        public string ConnectionString { get; set; }
    }
}

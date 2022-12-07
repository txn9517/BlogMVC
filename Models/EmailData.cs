using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Models
{
    // email attributes
    public class EmailData
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? EmailAddress { get; set; }

        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? Message { get; set; }
    }
}

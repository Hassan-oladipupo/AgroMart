using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BulkyBook.Models
{
    public class Category
    {
        
            [Key]
            public int Id { get; set; }
            [Required]
            public string Name { get; set; }

        [Display(Name = "Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be in the range of 1-100!!")]
        public int DisplayOrder { get; set; }

        

    }
}

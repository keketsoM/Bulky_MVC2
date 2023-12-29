using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Model
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;
        [DisplayName("Display Order")]
        [Range(0, 100)]
        public int DisplayOrder { get; set; }
    }
}

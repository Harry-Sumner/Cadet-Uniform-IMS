using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class Uniform
    {
        [Key]
        [Required]
        public int UniformID { get; set; }
        [StringLength(100)]
        [Required]
        public string Name { get; set; }
        [Required]
        public int TypeID { get; set; }
    }
}

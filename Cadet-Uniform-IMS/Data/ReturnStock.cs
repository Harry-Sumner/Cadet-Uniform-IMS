using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class ReturnStock
    {
        [Key]
        [Required]
        public int ReturnID { get; set; }
        [Required]
        public int UniformID { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}

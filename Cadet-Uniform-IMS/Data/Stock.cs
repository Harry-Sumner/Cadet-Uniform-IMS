using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class Stock
    {
        [Key]
        [Required]
        public int StockID { get; set; }
        [Required]
        public int UniformID { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}

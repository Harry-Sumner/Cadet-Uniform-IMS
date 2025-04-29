using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class OrderItem
    {
        [Key]
        public int OrderID { get; set; }

        [Key]
        public int StockID { get; set; }

        [Required]
        public int Quantity { get; set; }

    }
}

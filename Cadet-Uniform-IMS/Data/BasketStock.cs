using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class BasketStock
    {
        [Key]
        public int StockID { get; set; }

        [Key]
        [StringLength(450)]
        public string UID { get; set; }

        [Required]
        public string Quantity { get; set; }
    }
}

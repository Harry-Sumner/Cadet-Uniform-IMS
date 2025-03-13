using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class StockSize
    {
        [Key]
        [Required]
        public int StockID { get; set; }
        [Required]
        public int AttributeID { get; set; }
        [Required]
        public string Size { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class OrderHistory
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        public string UID { get; set; }

        public string? Cadet { get; set; }

    }
}

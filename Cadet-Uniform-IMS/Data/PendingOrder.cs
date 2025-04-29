using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class PendingOrder
    {
        [Key]
        public int PendingOrderID { get; set; }

        [Required]
        public string UID { get; set; }
    }
}

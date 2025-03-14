    using System.ComponentModel.DataAnnotations;

    namespace Cadet_Uniform_IMS.Data
    {
        public class SizeAttribute
        {
            [Key]
            [Required]
            public int AttributeID { get; set; }
            [Required]
            public int TypeID { get; set; }
            [StringLength(100)]
            [Required]
            public string AttributeName { get; set; }

        }
    }

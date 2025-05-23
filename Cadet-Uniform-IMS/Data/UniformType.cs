﻿using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class UniformType
    {
        [Key]
        [Required]
        public int TypeID { get; set; }
        [StringLength(50)]
        [Required]
        public string TypeName { get; set; }
    }
}

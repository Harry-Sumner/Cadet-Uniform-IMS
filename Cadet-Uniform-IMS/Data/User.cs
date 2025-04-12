using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class User : IdentityUser
    {
        [PersonalData, Required, StringLength(30)]
        public string FirstName { get; set; }
        [PersonalData, Required, StringLength(60)]
        public string Surname { get; set; }
        [Required, StringLength(5)]
        public string Rank { get; set; }
        public string Name { get { return $"{Rank} {Surname}"; } }

        public byte[]? ProfilePicture { get; set; }
    }

    public class Cadet : User
    {
        [Required, PersonalData, StringLength(16)]
        public string CadetNo { get; set; }

        [Required]
        public string Flight { get; set; }
    }

    public class Staff : User
    {
        [Required, PersonalData, StringLength(9)]
        public string StaffNo { get; set; }
    }
}

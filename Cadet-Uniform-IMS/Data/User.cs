using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.Data
{
    public class IMS_User : IdentityUser
    {
        [PersonalData, Required, StringLength(30)]
        public string FirstName { get; set; }
        [PersonalData, Required, StringLength(60)]
        public string Surname { get; set; }
        [Required, StringLength(5)]
        public string Rank { get; set; }
        public string Name { get { return $"{Rank} {Surname}"; } }
        public byte[]? ProfilePicture { get; set; }
        [PersonalData]
        public int? Height { get; set; }
        [PersonalData]
        public int? Head { get; set; }
        [PersonalData]
        public int? Neck { get; set; }
        [PersonalData]
        public int? Chest { get; set; }
        [PersonalData]
        public int? Leg { get; set; }
        [PersonalData]
        public int? WaistKnee { get; set; }
        [PersonalData]
        public int? Waist { get; set; }
        [PersonalData]
        public int? Hips { get; set; }
        [PersonalData]
        public int? Seat { get; set; }
        [PersonalData]
        public int? Shoe { get; set; }
    }

    public class IMS_Cadet : IMS_User
    {
        [Required, PersonalData, StringLength(16)]
        public string CadetNo { get; set; }

        [Required]
        public string Flight { get; set; }
    }

    public class IMS_Staff : IMS_User
    {
        [Required, PersonalData, StringLength(9)]
        public string StaffNo { get; set; }
    }
}

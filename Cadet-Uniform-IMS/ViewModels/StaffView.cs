﻿using System.ComponentModel.DataAnnotations;

namespace Cadet_Uniform_IMS.ViewModels
{
    public class StaffView
    {
        [Required, Display(Name = "Rank")]
        public required string Rank { get; set; }

        [Required, DataType(DataType.Text), Display(Name = "First Name")]
        public required string FirstName { get; set; }

        [Required, DataType(DataType.Text), Display(Name = "Surname")]
        public required string Surname { get; set; }

        [Required, Display(Name = "Staff Number")]
        public required string StaffNo { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}

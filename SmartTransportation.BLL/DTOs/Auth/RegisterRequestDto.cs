using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "UserType is required")]
        public int UserTypeId { get; set; }
    }
}

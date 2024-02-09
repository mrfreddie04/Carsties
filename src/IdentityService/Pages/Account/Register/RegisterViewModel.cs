using System.ComponentModel.DataAnnotations;

namespace IdentityService.Pages.Register;

public class RegisterViewModel
{
  [Required]
  [Display(Name = "Email Address")]
  [EmailAddress(ErrorMessage = "Invalid Email address")]
  public string Email { get; set; }

  [Required]
  [Display(Name = "User Name")]
  public string Username { get; set; }

  [Required]
  [Display(Name = "Full Name")]
  public string FullName { get; set; }  
      
  [Required]
  [DataType(DataType.Password)]  
  public string Password { get; set; }

  public string ReturnUrl { get; set; }  
  public string Button { get; set; }  
}

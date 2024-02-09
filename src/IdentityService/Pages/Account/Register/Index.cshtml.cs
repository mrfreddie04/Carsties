using System.Security.Claims;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityService.Pages.Register;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
  private readonly UserManager<ApplicationUser> _userManager;

  [BindProperty]
  public RegisterViewModel Input { get; set; } = new();

  [BindProperty]
  public bool RegisterSuccess { get; set; }

  public Index(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public IActionResult OnGet([FromQuery] string returnUrl)
  {
    if(!string.IsNullOrEmpty(returnUrl))
      Input.ReturnUrl = returnUrl;

    return Page();
  }

  public async Task<IActionResult> OnPostAsync()
  {
    //cancelled
    if(Input.Button != "register")
    {
      //return RedirectToPage("/Index");
      return Redirect("~/");
    }

    //Validate input
    if (!ModelState.IsValid) return Page();    

    //Create a User object
    var user = new ApplicationUser()
    {
      UserName = Input.Username,
      Email = Input.Email      
    };
    var result = await _userManager.CreateAsync(user, Input.Password);
    if(!result.Succeeded)
    {
      foreach(var error in result.Errors)
      {
        ModelState.AddModelError("Register", error.Description);
      }
      return Page();
    }    

    //Add Claims to the user
    await _userManager.AddClaimsAsync(user, new List<Claim>()
    {
      new(JwtClaimTypes.Name, Input.FullName)
    });    

    RegisterSuccess = true;

    return Page(); //RedirectToPage("/Account/Login/Index");
  }  
}

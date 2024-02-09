using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService. Services;

public class CustomProfileService : IProfileService
{
  private readonly UserManager<ApplicationUser> _userManager;

  public CustomProfileService(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public async Task GetProfileDataAsync(ProfileDataRequestContext context)
  {
    //get User & Claims - from IdentityDB
    var user = await _userManager.GetUserAsync(context.Subject);
    var existingClaims = await _userManager.GetClaimsAsync(user);

    //build claims list for the purpose of extending access token claims list
    //auction service will need this info
    var claims = new List<Claim>()
    {
      new Claim("username", user.UserName),
      //existingClaims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Name)
      //new Claim("fullname", existingClaims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Name).Value)
    };
    var nameClaim = existingClaims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Name);
    if(nameClaim is not null) claims.Add(nameClaim);    

    //add claims to the token
    context.IssuedClaims.AddRange(claims);
  }

  public Task IsActiveAsync(IsActiveContext context)
  {
    return Task.CompletedTask;
  }
}

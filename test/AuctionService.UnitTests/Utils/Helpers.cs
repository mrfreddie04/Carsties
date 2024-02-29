using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public class Helpers
{
  public static ClaimsPrincipal GetClaimsPrincipal()
  {
		//create Security Context
		var claims = new List<Claim>(){ 
      new Claim(ClaimTypes.Name, "test"),
      new Claim("username", "test") 
    };
		//create identity - provide claims and authentication type - we would use cookie auth - we just provide an arbitrary name
		var identity = new ClaimsIdentity(claims, "testing");
		//create claims principal
		return new ClaimsPrincipal(identity);
  }
}

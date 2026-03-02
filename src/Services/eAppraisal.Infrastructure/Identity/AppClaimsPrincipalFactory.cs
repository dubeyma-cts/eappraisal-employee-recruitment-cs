using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace eAppraisal.Infrastructure.Identity;

public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public AppClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrEmpty(user.AppRole))
            identity.AddClaim(new Claim("AppRole", user.AppRole));

        if (user.EmployeeId.HasValue)
            identity.AddClaim(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));

        if (!string.IsNullOrEmpty(user.FullName))
            identity.AddClaim(new Claim("FullName", user.FullName));

        return identity;
    }
}

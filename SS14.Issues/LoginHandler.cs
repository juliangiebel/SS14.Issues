using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SS14.Issues.Data;
using SS14.Issues.Helpers;

namespace SS14.Issues;

public sealed class LoginHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public LoginHandler(ApplicationDbContext dbContext, LinkGenerator linkGenerator, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task HandleTokenValidated(TokenValidatedContext ctx)
    {
        var identity = ctx.Principal?.Identities?.FirstOrDefault(i => i.IsAuthenticated);
        if (identity == null)
        {
            Debug.Fail("Unable to find identity.");
        }

        var guid = identity.Claims.GetUserId();

        //var adminData = await _dbContext.Admin.FirstOrDefaultAsync(a => a.UserId == guid);

        var adminList = _configuration.GetSection("Admins").Get<string[]>();
        
        if (adminList != null && adminList.Contains(guid.ToString()))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        }
    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SS14.Issues.Pages;

public class Login : PageModel
{
    public async Task OnGet(string redirectUri)
    {
        await HttpContext.ChallengeAsync("oidc", new 
            AuthenticationProperties { RedirectUri = redirectUri } );
    }  
}
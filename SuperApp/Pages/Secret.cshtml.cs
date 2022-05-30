using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SuperApp.Pages;

[Authorize]
public class Secret : PageModel
{
    public void OnGet()
    {

    }
}
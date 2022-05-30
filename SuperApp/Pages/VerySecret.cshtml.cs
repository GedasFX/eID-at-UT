using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SuperApp.Pages;

[Authorize("eID")]
public class VerySecret : PageModel
{
    public void OnGet()
    {
        
    }
}
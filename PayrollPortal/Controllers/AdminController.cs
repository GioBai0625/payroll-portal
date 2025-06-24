using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "HR")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _um;
    public AdminController(UserManager<IdentityUser> um) => _um = um;

    public IActionResult Users()
    {
        var users = _um.Users.Select(u => new { u.Id, u.Email }).ToList();
        return View(users);
    }
}

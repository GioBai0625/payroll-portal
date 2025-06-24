using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PayrollPortal.Data;
using PayrollPortal.Models;

namespace PayrollPortal.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Employee/Home
        public async Task<IActionResult> Home()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge(); // not logged in
            }

            var payslips = _context.Payslips
                .Where(p => p.EmployeeId == user.Id)
                .OrderByDescending(p => p.Month)
                .ToList();

            return View(payslips);
        }

        // GET: /Employee/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var payslip = _context.Payslips.FirstOrDefault(p => p.Id == id && p.EmployeeId == user.Id);

            if (payslip == null)
                return NotFound("Payslip not found or not authorized.");

            if (string.IsNullOrWhiteSpace(payslip.FilePath) || !payslip.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file path.");

            var fullPath = Path.Combine(_env.WebRootPath, payslip.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found on server.");

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/pdf", payslip.FileName);
        }
    }
}

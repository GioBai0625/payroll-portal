using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PayrollPortal.Data;
using PayrollPortal.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollPortal.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<IdentityUser> _userManager;

        public HRController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // GET: /HR/Dashboard
        public IActionResult Dashboard()
        {
            ViewBag.Employees = _userManager.Users.ToList();
            return View();
        }

        // POST: /HR/UploadPayslip
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPayslip(IFormFile file, string employeeEmail, string month)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a PDF file.");
                ViewBag.Employees = _userManager.Users.ToList();
                return View("Dashboard");
            }

            if (string.IsNullOrWhiteSpace(month) || !month.Contains('-'))
            {
                ModelState.AddModelError("", "Please enter a valid month (YYYY-MM).");
                ViewBag.Employees = _userManager.Users.ToList();
                return View("Dashboard");
            }

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(employeeEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "Employee not found.");
                ViewBag.Employees = _userManager.Users.ToList();
                return View("Dashboard");
            }

            // Save the PDF
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var physicalPath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Record metadata
            _context.Payslips.Add(new Payslip
            {
                EmployeeId = user.Id,
                EmployeeName = user.UserName,
                Month = month,
                FileName = file.FileName,
                FilePath = $"/uploads/{uniqueFileName}",
                UploadedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payslip uploaded!";
            return RedirectToAction("Dashboard");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FastMoney.Data;
using FastMoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastMoney.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAllAdmin()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userList = await _db.ApplicationUser.Where(u => u.Id != claim.Value && u.IsAdmin==true).ToListAsync();
            return Json(new { data = userList });
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsConfirmed(string? id)
        {
            var applicationUserFromDb = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (id == null)
            {
                return NotFound();
            }

            if (applicationUserFromDb == null)
            {
                return NotFound();
            }

            if (applicationUserFromDb.LockoutEnd == null || applicationUserFromDb.LockoutEnd < DateTime.Now)
            {
                applicationUserFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            else
            {
                applicationUserFromDb.LockoutEnd = DateTime.Now;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                var user = await _db.ApplicationUser.Where(m => m.Id == applicationUser.Id).FirstOrDefaultAsync();

                user.Name = applicationUser.Name;
                user.IdentificationNumber = applicationUser.IdentificationNumber;
                user.DateOfBirth = applicationUser.DateOfBirth;
                user.StreetAddress = applicationUser.StreetAddress;
                user.City = applicationUser.City;
                user.State = applicationUser.State;
                user.ZipCode = applicationUser.ZipCode;
                user.PhoneNumber = applicationUser.PhoneNumber;
                user.IsAdmin = applicationUser.IsAdmin;

                _db.Update(user);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(applicationUser);

        }
    }
}
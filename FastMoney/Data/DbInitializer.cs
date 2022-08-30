using FastMoney.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastMoney.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }

            catch(Exception ex)
            {

            }

            _userManager.CreateAsync(new ApplicationUser
            {
                UserName= "manager@valuationascensionbank.com",
                Email= "manager@valuationascensionbank.com",
                EmailConfirmed=true,
                Name="Application Manager",
                IdentificationNumber="123456789",
                DateOfBirth=DateTime.Today.Date,
                StreetAddress="Application Manager",
                City= "Application Manager",
                State= "Application Manager",
                ZipCode="xxxxxx",
                IsAdmin=true,
                AccountNumber=1,
                CurrentBalance=0.00,
                OpeningDate = DateTime.Now.Date,
                PhoneNumber="0123456789"
            }, "Manager123*").GetAwaiter().GetResult();
        }
    }
}

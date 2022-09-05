using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.Models;
using HeavenofBooks.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenofBooks.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;       
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(
            UserManager<IdentityUser> userManager,          
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
           
        }
        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Count()>0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            // create roles if they are not created yet

            if (!_roleManager.RoleExistsAsync(StaticDetails.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_User_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_User_Individual)).GetAwaiter().GetResult();

                //if roles are not created, then we will create admin user as well

                _userManager.CreateAsync(new AppUser
                {
                    UserName = "admin@heavenofbooks.com",
                    Email = "admin@heavenofbooks.com",
                    Name = "Furkan Yıldırım",
                    PhoneNumber = "5378420530",
                    StreetAddress = "testAddress",
                    State = "Marmara",
                    PostalCode = "34000",
                    City = "Istanbul"
                }, "Admin123*").GetAwaiter().GetResult();
                AppUser user = _context.appUser.FirstOrDefault(u => u.Email == "admin@heavenofbooks.com");
                _userManager.AddToRoleAsync(user, StaticDetails.Role_Admin).GetAwaiter().GetResult();
            }
        }
    }
}

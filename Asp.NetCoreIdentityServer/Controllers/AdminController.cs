using Asp.NetCoreIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.Controllers
{
    public class AdminController : BaseController
    {
              
        public AdminController(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager):base(userManager,null,roleManager)
        {
            
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult Users()
        {

            return View(_userManager.Users.ToList());
        }
    }
}

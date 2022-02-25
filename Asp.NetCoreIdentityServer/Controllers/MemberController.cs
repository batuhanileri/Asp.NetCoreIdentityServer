using Asp.NetCoreIdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Asp.NetCoreIdentityServer.ViewModels;

namespace Asp.NetCoreIdentityServer.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private UserManager<AppUser> _userManager { get; }
        private SignInManager<AppUser> _signInManager { get; }

        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

           //Mapster kütüphanesi = //Adapt ile USER Tablom ile UserViewModel classımı mapliyoruz. AutoMapper Mantığı..

            return View(userViewModel);
        }
    }
}

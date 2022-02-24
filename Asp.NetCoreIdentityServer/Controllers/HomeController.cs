﻿using Asp.NetCoreIdentityServer.Models;
using Asp.NetCoreIdentityServer.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<AppUser> _userManager { get; }

        public HomeController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {

            if (ModelState.IsValid)
            {
                AppUser user = new AppUser();
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                IdentityResult result = await _userManager.CreateAsync(user, userViewModel.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }

                else
                {
                    foreach (IdentityError item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }


            }

            return View(userViewModel);
        }
    }
}
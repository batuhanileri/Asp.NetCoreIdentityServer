﻿using Asp.NetCoreIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<AppUser> _userManager { get; }
        
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            
            return View(_userManager.Users.ToList());
        }
    }
}

﻿using Asp.NetCoreIdentityServer.Models;
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

        public IActionResult UserEdit()
        {
            AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            UserViewModel userViewModel = user.Adapt<UserViewModel>();
            return View(userViewModel);

        }
        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel userViewModel)
        {
            ModelState.Remove("Password");

            //Userviewmodelde password alanını burda güncellemediğimiz için invalid geliyor
            //o yüzden model state kısmından password alanını kontrol etmemesi için remove methodunu kullanıyoruz.

            if(ModelState.IsValid)
            {
                AppUser user =await _userManager.FindByNameAsync(User.Identity.Name);

                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                IdentityResult result = await _userManager.UpdateAsync(user);
               
                if(result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);

                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user,true);
                    
                    ViewBag.success = "true";
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }

            }
            return View(userViewModel);
        }
        public IActionResult PasswordChange()
        {

            return View();
        }
        [HttpPost]
        public IActionResult PasswordChange(PasswordChangeViewModel passwordChangeViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result;

                if (user != null)
                {
                    bool exist = _userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld).Result;
                    //eski şifremizi doğru yazdık mı kontrol ediyoruz.

                    if (exist)
                    {
                        IdentityResult result = _userManager.ChangePasswordAsync(user, passwordChangeViewModel.PasswordOld, passwordChangeViewModel.PasswordNew).Result;

                        if (result.Succeeded)
                        {
                            _userManager.UpdateSecurityStampAsync(user);
                            //Güncelleme sebebi 30 dakikada bir kontrol ediliyor client
                            // eğer security tokenleri eşleşmezse otomatik olarak siteden atacaktır
                            //o yüzden burada güncelliyoruz ki siteden atmasın.

                            _signInManager.SignOutAsync();
                            _signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true, false);
                                                                       
                            ViewBag.success = "true"; 
                        }
                        else
                        {
                            foreach (var item in result.Errors)
                            {
                                ModelState.AddModelError("", item.Description);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Eski Şifreniz Yanlış");
                    }

                }
            }

            return View(passwordChangeViewModel);
        }
        public void Logout()
        {
            _signInManager.SignOutAsync();
        }
    }
}

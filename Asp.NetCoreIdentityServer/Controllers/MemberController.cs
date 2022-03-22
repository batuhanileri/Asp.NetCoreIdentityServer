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
using Microsoft.AspNetCore.Mvc.Rendering;
using Asp.NetCoreIdentityServer.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;

namespace Asp.NetCoreIdentityServer.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager):base(userManager,signInManager)
        {
           
        }

        public IActionResult Index()
        {
            AppUser user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            //Mapster kütüphanesi = //Adapt ile USER Tablom ile UserViewModel classımı mapliyoruz. AutoMapper Mantığı..

            return View(userViewModel);
        }

        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            return View(userViewModel);

        }
        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel userViewModel,IFormFile userPicture)
        {
            ModelState.Remove("Password");
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            //Userviewmodelde password alanını burda güncellemediğimiz için invalid geliyor
            //o yüzden model state kısmından password alanını kontrol etmemesi için remove methodunu kullanıyoruz.

            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                if (userPicture!=null && userPicture.Length>0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);

                    using(var stream = new FileStream(path,FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);

                        user.Picture = "/UserPicture/" + fileName;
                    }
                }

                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;
                user.City = userViewModel.City;
                user.BirthDay = userViewModel.BirthDay;
                user.Gender = (int)userViewModel.Gender;

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
                    AddModelError(result);
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
                AppUser user = CurrentUser;

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
                            AddModelError(result);
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
        public IActionResult AccessDenied(string returnUrl)
        {
            if(returnUrl.Contains("BirthDayClaims"))
            {
                ViewBag.message = "Erişmeye çalıştığın sayfa 15 yaşından büyük olmadığınız için " +
                    "giriş yapamıyorsunuz.";
            }
            else if(returnUrl.Contains("CityClaimsBursa"))
            {
                ViewBag.message = "Bu sayfaya sadece şehri bursa olanlar erişebilir.";
            }
            else if (returnUrl.Contains("Exchange"))
            {
                ViewBag.message = "30 günlük ücretsiz deneme hakkınız sona ermiştir.";
            }
            else
            {
                ViewBag.message = "Bu Sayfaya Erişim Yetkiniz Yoktur..";
            }
            return View();
        }

        [Authorize(Policy ="CityPolicy")]
        public IActionResult CityClaimsBursa()
        {
            return View();
        }

        [Authorize(Policy = "BirthDayPolicy")]
        public IActionResult BirthDayClaims()
        {
            return View();
        }

        public async Task<IActionResult> ExchangeRedirect()
        {
            bool result = User.HasClaim(x => x.Type == "ExpireDateExchange");

            if(!result)
            {
                Claim ExpireDateExchange = new Claim("ExpireDateExchange", DateTime.Now.AddDays(30).ToShortDateString(),ClaimValueTypes.String,"Internal");

                await _userManager.AddClaimAsync(CurrentUser, ExpireDateExchange);
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(CurrentUser, true);
            }

            return RedirectToAction("Exchange");
        }
       
        [Authorize(Policy = "ExchangePolicy")]
        public IActionResult Exchange()
        {
            return View();
        }

        public IActionResult TwoFactorAuth()
        {
            AuthenticatorViewModel model = new AuthenticatorViewModel()
            {
                TwoFactorType = (TwoFactor)CurrentUser.TwoFactor
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> TwoFactorAuth(AuthenticatorViewModel authenticatorView)
        {
            switch(authenticatorView.TwoFactorType)
            {
                case TwoFactor.None:
                    CurrentUser.TwoFactorEnabled = false;
                    CurrentUser.TwoFactor = (sbyte)TwoFactor.None;

                    TempData["message"] = "İki adımlı kimlik doğrulama işlemi hiçbiri olarak güncellendi";
                    break;
            }
            await _userManager.UpdateAsync(CurrentUser);
            return View(authenticatorView);
        }
    }
}

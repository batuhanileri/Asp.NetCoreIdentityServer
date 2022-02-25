using Asp.NetCoreIdentityServer.Models;
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
        private SignInManager<AppUser> _signInManager { get; }

        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(loginViewModel.Email);

                if (user != null)
                {
                    if (await _userManager.IsLockedOutAsync(user))
                    //IsLockedOutAsync booldur=  true olursa birden fazla yanlış şifre girilmiş olup ,
                    //hesabı belirlenen süre kadar kilitlenir.
                    {
                        ModelState.AddModelError("", "Hesabınız bir süreliğine kilitlenmiştir. " +
                            " Lütfen daha sonra tekrar deneyiniz.");

                        return View(loginViewModel);
                    }
                    await _signInManager.SignOutAsync(); // önce bi çıkış olsun ki sistemde eski bir key silinsin

                    Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, loginViewModel.RememberMe, false);

                    // Paramaterenin içindeki loginViewModel.RememberMe beni hatırla özelliği gibi cookie expiration(60 gün) olanı etkin hale getirme işine yarıyor
                    // Son false başarsısız girişlerde kullanıcıyı kilitlicek misin anlamına geliyor.
                    if (signInResult.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);//Kullanıcı başarılı giriş yapmış fail sayısını 0'lıyoruz.

                        if (TempData["ReturnUrl"] != null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                    else
                    {
                        await _userManager.AccessFailedAsync(user); // Kullanıcın her yanlış girişini kaydedip sayıyı 1 artırır. 

                        int fail = await _userManager.GetAccessFailedCountAsync(user); // Kullanıcının kaç kez başarısız giriş yaptığını kaydediyoruz.
                        ModelState.AddModelError("", $"{fail} kez başarısız giriş yaptınız.");

                        if (fail == 3)
                        {
                            await _userManager.SetLockoutEndDateAsync(user,
                                 new DateTimeOffset(DateTime.Now.AddMinutes(20)));

                            // kullanıcı 3 kez başarısız girerse 20 dakika boyunca girişini engelliyoruz.

                            ModelState.AddModelError("", "Hesabınız 3 başarısız girişten dolayı 20 dakika süreyle kilitlenmiştir." +
                                " Lütfen daha sonra tekrar deneyiniz.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Email adresiniz veya Şifreniz yanlıştır.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bu email adresine kayıtlı kullanıcı bulunamamıştır.");
                }
            }

            return View(loginViewModel);
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

        public IActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ResetPassword(PasswordResetViewModel passwordResetViewModel)
        {

            AppUser user = _userManager.FindByEmailAsync(passwordResetViewModel.Email).Result;

            if (user != null)
            {
                string passwordResetToken = _userManager.GeneratePasswordResetTokenAsync(user).Result;
                //random bir token üretiyoruz.

                string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                {
                    userId = user.Id,
                    token = passwordResetToken
                    
                }, HttpContext.Request.Scheme);

                // id + token şeklinde link oluşuyor..


                Helper.PasswordReset.PasswordResetSendEmail(passwordResetLink, passwordResetViewModel.Email);

                ViewBag.status = "Success";
            }

            else
            {
                ModelState.AddModelError("", "Sistemde kayıtlı bir email adresi bulunamamıştır.");
            }

            return View(passwordResetViewModel);
        }
        public IActionResult ResetPasswordConfirm(string userId,string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("NewPassword")]PasswordResetViewModel passwordResetViewModel)
            
        //Bind ile masterda sadece gelmesini istediğimiz verinin propertysini(kullanmıcağımız property varsa Bind kullanıyoruz) yazıyoruz.
        {
            string token = TempData["token"].ToString(); //Sayfalar arası veri taşımak için TempData kullanıyoruz.
            string userId = TempData["userId"].ToString();

            AppUser user =await _userManager.FindByIdAsync(userId);

            if(user!=null)
            {
                IdentityResult result = await _userManager.ResetPasswordAsync(user, token, passwordResetViewModel.NewPassword);

                if(result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user); //kullanıcı şifre,username , mail gibi önemli alanları değiştirirse 
                                                                       //securitystamp'i de güncelliyoruz ki eski şifre,username ile tekrardan işlem yapamasın.

                    ViewBag.status = "Success";
                    
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
                ModelState.AddModelError("", "Hata Meydana Gelmiştir.");
            }

            return View(passwordResetViewModel);
        }
    }
}
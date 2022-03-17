using Asp.NetCoreIdentityServer.Enums;
using Asp.NetCoreIdentityServer.Models;
using Asp.NetCoreIdentityServer.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.Controllers
{
    public class HomeController : BaseController
    {

        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : base(userManager, signInManager)
        {

        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Member");
            }
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

                    if (_userManager.IsEmailConfirmedAsync(user).Result == false)
                    {
                        ModelState.AddModelError("", "Email Adresiniz Onaylanmamıştır. Lütfen E-postanızı kontrol ediniz");
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
                    string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken
                    }, protocol: HttpContext.Request.Scheme);

                    Helper.EmailConfirmation.SendEmail(link, user.Email);

                    return RedirectToAction("Login");
                }

                else
                {
                    AddModelError(result);
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
        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("NewPassword")] PasswordResetViewModel passwordResetViewModel)

        //Bind ile masterda sadece gelmesini istediğimiz verinin propertysini(kullanmıcağımız property varsa Bind kullanıyoruz) yazıyoruz.
        {
            string token = TempData["token"].ToString(); //Sayfalar arası veri taşımak için TempData kullanıyoruz.
            string userId = TempData["userId"].ToString();

            AppUser user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                IdentityResult result = await _userManager.ResetPasswordAsync(user, token, passwordResetViewModel.NewPassword);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user); //kullanıcı şifre,username , mail gibi önemli alanları değiştirirse 
                                                                       //securitystamp'i de güncelliyoruz ki eski şifre,username ile tekrardan işlem yapamasın.

                    ViewBag.status = "Success";

                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Hata Meydana Gelmiştir.");
            }

            return View(passwordResetViewModel);
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

            if(result.Succeeded)
            {
                ViewBag.status = "Email adresiniz onaylanmıştır. Login Ekranından giriş yapabilirsiniz.";
                    

            }
            else
            {
                ViewBag.status = "Hata Meydana geldi.";

            }
            return View();
        }

        public IActionResult GoogleLogin(string ReturnUrl)

        {
            string RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", RedirectUrl);

            return new ChallengeResult("Google", properties); 
        }

        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/")
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            
            if (info == null)
            {
                return RedirectToAction("LogIn");
            }
            else
            {
                

                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                

                if (result.Succeeded)
                {

                    return Redirect(ReturnUrl);
                }
                else
                {
                    AppUser user = new AppUser();
                   
                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    string ExternalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

                    if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Name))
                    {
                        string userName = info.Principal.FindFirst(ClaimTypes.Name).Value;

                        userName = userName.Replace(' ', '-').ToLower() + ExternalUserId.Substring(0, 5).ToString();

                        user.UserName = userName;
                    }
                    else
                    {
                        user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    AppUser user2 = await _userManager.FindByEmailAsync(user.Email);

                    if (user2 == null)
                    {
                        user.EmailConfirmed = true;
                        IdentityResult createResult = await _userManager.CreateAsync(user);

                        if (createResult.Succeeded)
                        {
                           
                        
                            IdentityResult loginResult = await _userManager.AddLoginAsync(user, info);

                            if (loginResult.Succeeded)
                            {
                                //     await signInManager.SignInAsync(user, true);

                                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                                return RedirectToAction("AuthenticationSignUp");
                            }
                            else
                            {
                                AddModelError(loginResult);
                            }
                        }
                        else
                        {
                            AddModelError(createResult);
                        }
                    }
                    else
                    {
                        IdentityResult loginResult = await _userManager.AddLoginAsync(user2, info);

                        await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                        return Redirect(ReturnUrl);
                    }
                }
            }

            List<string> errors = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();

            return View("Error", errors);
        }
        public IActionResult AuthenticationSignUp()
        {
            AppUser user = CurrentUser;

            AuthenticationViewModel authenticationViewModel = user.Adapt<AuthenticationViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            return View(authenticationViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AuthenticationSignUp(AuthenticationViewModel authenticationViewModel, IFormFile userPicture)
        {
            ModelState.Remove("Password");
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            //Userviewmodelde password alanını burda güncellemediğimiz için invalid geliyor
            //o yüzden model state kısmından password alanını kontrol etmemesi için remove methodunu kullanıyoruz.

            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                if (userPicture == null && userPicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);

                        user.Picture = "/UserPicture/" + fileName;
                    }
                }
                
                user.PhoneNumber = authenticationViewModel.PhoneNumber;
                user.City = authenticationViewModel.City;
                user.BirthDay = authenticationViewModel.BirthDay;
                user.Gender = (int)authenticationViewModel.Gender;
                

                IdentityResult result = await _userManager.UpdateAsync(user);
                await _userManager.AddPasswordAsync(user, authenticationViewModel.Password);

                if (result.Succeeded)
                {
                    

                    await _userManager.UpdateSecurityStampAsync(user);

                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, true);

                    return RedirectToAction("LogIn");

                }
                else
                {
                    AddModelError(result);
                }

            }
       
            return View(authenticationViewModel);
        }
    }
}
using Asp.NetCoreIdentityServer.Models;
using Asp.NetCoreIdentityServer.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;

namespace Asp.NetCoreIdentityServer.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : BaseController
    {

        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(userManager, null, roleManager)
        {

        }
        public IActionResult Index()
        {

            return View();
        }
        public IActionResult Roles()
        {

            return View(_roleManager.Roles.ToList()); // roles tablosunu gidip listeleme yapabiliyoruz.
        }
        public IActionResult RoleCreate()
        {

            return View();
        }
        [HttpPost]
        public IActionResult RoleCreate(RoleViewModel roleViewModel)
        {
            AppRole role = new AppRole();
            role.Name = roleViewModel.Name;  // yeni bir rol ismi oluşturuyoruz

            IdentityResult result = _roleManager.CreateAsync(role).Result; // create methodu ile veritabanına ekliyoruz.

            if (result.Succeeded)
            {
                return RedirectToAction("Roles");
            }
            else
            {
                AddModelError(result);
            }

            return View(roleViewModel);
        }
        public IActionResult Users()
        {

            return View(_userManager.Users.ToList());
        }
        public IActionResult RoleDelete(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;

            if (role != null)
            {
                IdentityResult result = _roleManager.DeleteAsync(role).Result;

            }
            return RedirectToAction("Roles");
        }
        public IActionResult RoleUpdate(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;

            return View(role.Adapt<RoleViewModel>()); //Burada id'leri eşleştirme yapabilmek için approle tablosu ile viewmodeli adapt
                                                      //yani mapster kütüphanesini kullanıyorum.
        }
        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel roleViewModel)
        {
            AppRole role = _roleManager.FindByIdAsync(roleViewModel.Id).Result;

            if (role != null)
            {
                role.Name = roleViewModel.Name;
                IdentityResult result = _roleManager.UpdateAsync(role).Result;
                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Güncelleme İşlemi Başarısız Oldu");
            }
            return View(roleViewModel);
        }

        public IActionResult RoleAssign(string id)
        {
            TempData["userId"] = id;
            AppUser user = _userManager.FindByIdAsync(id).Result; //kullanıcıyı id ile buluyoruz
            ViewBag.userName = user.UserName;  // kullanıcıyı viewbaga yazdırıyoruz çünkü viewden alıcaz 

            IQueryable<AppRole> roles = _roleManager.Roles; // rollerimizi çekiyoruz checkboxta göstericez

            List<string> userRoles = _userManager.GetRolesAsync(user).Result as List<string>; // user hangi rollere sahip onları görücez

            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>();

            foreach (var role in roles)
            {
                RoleAssignViewModel r = new RoleAssignViewModel(); //Checkboxları göstericez ve checkboxların işaretli olup olmadığını göstermek için viewmodel yazıyoruz.
                r.RoleId = role.Id;
                r.RoleName = role.Name;
                if (userRoles.Contains(role.Name))
                {
                    r.Exist = true; //Contains ile kullanıcıya bu rol atandı ise exist(checkbox) true olucak
                }
                else
                {

                    r.Exist = false; // atanmadıysa false kalıcak
                }

                roleAssignViewModels.Add(r);

            }


            return View(roleAssignViewModels);
        }
        [HttpPost]
        public async Task<IActionResult> RoleAssign(List<RoleAssignViewModel> roleAssignViewModels)
        {

            AppUser user = _userManager.FindByIdAsync(TempData["userId"].ToString()).Result;

            foreach (var item in roleAssignViewModels)
            {
                if (item.Exist) // checkbox check edildiyse AddToRoleAsync methodu ile o rolü o kullanıcıya atıyoruz
                {
                   await _userManager.AddToRoleAsync(user, item.RoleName);
                }
                else
                {       // checkbox eğer check edilmediyse RemoveFromRoleAsync methodu ile o rolü o kullanıcıdan çıkartıyoruz.
                   await _userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }
            return RedirectToAction("Users");
        }
    }
}

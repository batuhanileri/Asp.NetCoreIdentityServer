using Asp.NetCoreIdentityServer.Models;
using Asp.NetCoreIdentityServer.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
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

            if(result.Succeeded)
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

            if(role !=null)
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
                if(result.Succeeded)
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
    }
}

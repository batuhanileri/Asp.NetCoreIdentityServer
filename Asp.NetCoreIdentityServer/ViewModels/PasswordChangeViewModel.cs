using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.ViewModels
{
    public class PasswordChangeViewModel
    {
        [Required(ErrorMessage = "Eski Şifre gereklidir.")]
        [Display(Name = "Eski Şifreniz: ")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır.")]
        public string PasswordOld { get; set; }

        [Required(ErrorMessage = "Yeni Şifre gereklidir.")]
        [Display(Name = "Yeni Şifreniz: ")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır.")]   
        public string PasswordNew { get; set; }

        [Required(ErrorMessage = "Onay Yeni Şifre gereklidir.")]
        [Display(Name = "Onay Yeni Şifreniz: ")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır.")]
        [Compare("PasswordNew",ErrorMessage ="Yeni Şifreniz ve Onay Şifreniz birbirinden farklıdır.")]
        public string PasswordConfirm { get; set; }
    }
}

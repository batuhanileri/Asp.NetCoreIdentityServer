using Asp.NetCoreIdentityServer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.ViewModels
{
    public class AuthenticationViewModel
    {
        

        [Display(Name = "Tel No: ")]
        public string PhoneNumber { get; set; }     

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [Display(Name = "Şifre: ")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Şehir: ")]
        public string City { get; set; }
        public string Picture { get; set; }

        [Display(Name = "Doğum Tarihi: ")]
        [DataType(DataType.Date)]
        public DateTime? BirthDay { get; set; }

        [Display(Name = "Cinsiyet: ")]
        public Gender Gender { get; set; }
    }
}

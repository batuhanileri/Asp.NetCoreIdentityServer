using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.ViewModels
{
    public class RoleViewModel
    {
        [Display(Name="Rol İsmi")]
        [Required(ErrorMessage ="Role İsmi Gereklidir.")]
        public string Name { get; set; }

        public string Id { get; set; }
    }
}

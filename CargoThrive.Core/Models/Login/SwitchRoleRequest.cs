using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    public class SwitchRoleRequest
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        [Required]
        public long RoleId { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{

    /// <summary>
    /// 租户表
    /// </summary>
    public class Tenant : DefaultEntity
    {
        /// <summary>
        /// 租户名称
        /// </summary>
        [Required]
        [DisplayName("租户名称")]
        [Display(Name = "租户名称")]
        [Comment("租户名称")]
        [MaxLength(50)]
        [DefaultValue(null)]
        public string TenantName { get; set; }  // 租户名称
    }
}

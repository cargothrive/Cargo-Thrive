using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using CargoThrive.Core.Enums;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 默认请求实体
    /// </summary>
    public class DefaultRequest
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        [DisplayName("角色Id")]
        [Display(Name = "角色Id")]
        [Comment("角色Id")]
        public long? RoleId { get; set; }  // 角色Id


        /// <summary>
        /// 语言
        /// </summary>
        [DisplayName("语言")]
        [Display(Name = "语言")]
        [Comment("语言")]
        public LanguageEnum Language { get; set; }  // 语言


    }
}

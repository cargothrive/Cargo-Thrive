using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using CargoThrive.Core.Enums;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// Permission Table (权限表)
    /// </summary>
    public class Permission : DefaultEntity
    {
        /// <summary>
        /// 权限类型
        /// </summary>
        [Required(ErrorMessage = "权限类型不能为空")]
        [DisplayName("权限类型")]
        [Display(Name = "权限类型")]
        [Comment("权限类型（0=菜单，1=按钮，2=接口）")]
        public PermissionTypeEnum PermissionType { get; set; }

        /// <summary>
        /// 权限路径（菜单URL/接口地址）
        /// </summary>
        [DisplayName("权限路径")]
        [Display(Name = "权限路径")]
        [Comment("菜单URL或接口地址（按钮权限可为空）")]
        [StringLength(200, ErrorMessage = "权限路径不超过200字符")]
        public string? Path { get; set; }

        /// <summary>
        /// 父权限ID（用于权限层级，0=顶级权限）
        /// </summary>
        [DisplayName("父权限ID")]
        [Display(Name = "父权限ID")]
        [Comment("父权限ID（0表示顶级权限）")]
        [ForeignKey(nameof(ParentPermission))]
        public long ParentId { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        [Description("描述")]
        [Display(Name = "描述")]
        [Comment("描述")]
        [StringLength(200, ErrorMessage = "描述不超过200字符")]
        public string? Description { get; set; }


        /// <summary>
        /// 排序
        /// </summary>
        [DisplayName("排序")]
        [Display(Name = "排序")]
        [Comment("排序）")]
        public int Sort { get; set; } = 0;


        #region 导航属性
        /// <summary>
        /// 父权限（ParentId=0时为null）
        /// </summary>
        public Permission? ParentPermission { get; set; }
        #endregion
    }
}

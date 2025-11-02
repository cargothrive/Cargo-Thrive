using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Enums
{
    #region 权限类型
    /// <summary>
    /// 权限类型
    /// </summary>
    public enum PermissionTypeEnum
    {
        /// <summary>菜单权限</summary>
        [Description("菜单权限")]
        Menu = 0,

        /// <summary>按钮权限</summary>
        [Description("按钮权限")]
        Button = 1,

        /// <summary>接口权限</summary>
        [Description("接口权限")]
        Api = 2
    }
    #endregion
}
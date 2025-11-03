using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Enums
{
    #region 语言字典
    /// <summary>
    /// 语言字典
    /// </summary>
    public enum LanguageEnum
    {
        /// <summary>中文</summary>
        [Description("中文")]
        Chinese = 0,

        /// <summary>英语</summary>
        [Description("英语")]
        English = 1,

        /// <summary>西班牙语</summary>
        [Description("西班牙语")]
        Spanish = 2
    }
    #endregion
}
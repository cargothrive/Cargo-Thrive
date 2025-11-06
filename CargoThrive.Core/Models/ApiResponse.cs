using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 通用的 API 响应格式
    /// </summary>
    public class ApiResponse<T>
    {
        public T Result { get; set; }
        public string Message { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Exceptions
{
    public class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
        public AuthException(string message, Exception innerException) : base(message, innerException) { }
    }

    // 在 AuthService 中替换异常抛出
    // throw new Exception("用户名或密码错误");
    //throw new AuthException("用户名或密码错误");
}

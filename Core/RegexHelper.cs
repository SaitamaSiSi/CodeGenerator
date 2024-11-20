//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 13:41:50</date>
//------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace CodeGenerator.Core
{
    public class RegexHelper
    {
        public static bool IsValidIPv4(string ip)
        {
            string ipPattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(ip, ipPattern);
        }

        public static bool IsValidNumber(string number)
        {
            string ipPattern = @"^[0-9]*$";
            return Regex.IsMatch(number, ipPattern);
        }
    }
}

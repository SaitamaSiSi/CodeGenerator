//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:20:59</date>
//------------------------------------------------------------------------------

using Zyh.Common.Entity;

namespace CodeGenerator.Core
{
    public interface ICommand
    {
        public DatabaseType Command { get; set; }
    }
}

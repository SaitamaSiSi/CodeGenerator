//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:30:01</date>
//------------------------------------------------------------------------------

using CodeGenerator.Core;
using CodeGenerator.Model;
using System.Collections.Generic;
using Zyh.Common.Data;

namespace CodeGenerator.Command
{
    public abstract class CommandBase
    {
        public DatabaseType Command { get; set; }

        public CmdResCode Code { get; set; } = CmdResCode.成功;

        public List<ClassParam> Classes = new List<ClassParam>();
    }
}

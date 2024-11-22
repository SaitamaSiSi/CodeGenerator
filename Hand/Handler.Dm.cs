//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:34:17</date>
//------------------------------------------------------------------------------

using CodeGenerator.Command;
using CodeGenerator.Core;
using CodeGenerator.Generator;
using CodeGenerator.Model;

namespace CodeGenerator.Hand
{
    public partial class Handler : ICommandHandler<DmCmd, CmdResCode>
    {
        public ResultMessage<CmdResCode> Handle(DmCmd command)
        {
            foreach (var item in command.Classes)
            {
                ClassGenerator.CreateClasses(item, command.Config);
            }

            return ResultMessage<CmdResCode>.Successful("生成完毕", command.Code);
        }
    }
}

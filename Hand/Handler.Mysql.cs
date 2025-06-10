//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:45:12</date>
//------------------------------------------------------------------------------

using CodeGenerator.Command;
using CodeGenerator.Core;
using CodeGenerator.Generator;

namespace CodeGenerator.Hand
{
    public partial class Handler : ICommandHandler<MysqlCmd, CmdResCode>
    {
        public ResultMessage<CmdResCode> Handle(MysqlCmd command)
        {
            foreach (var item in command.Classes)
            {
                ClassGenerator.CreateClasses(item, command.Config);
            }

            return ResultMessage<CmdResCode>.Successful("生成完毕", command.Code);
        }

        public ResultMessage<CmdResCode> HandleSql(MysqlCmd command)
        {
            return ResultMessage<CmdResCode>.Successful("TODO", command.Code);
        }
    }
}

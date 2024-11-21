//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:45:12</date>
//------------------------------------------------------------------------------

using CodeGenerator.Command;
using CodeGenerator.Core;

namespace CodeGenerator.Hand
{
    public partial class Handler : ICommandHandler<MysqlCmd, CmdResCode>
    {
        public ResultMessage<CmdResCode> Handle(MysqlCmd command)
        {
            return ResultMessage<CmdResCode>.Successful("Mysql暂未实现", command.Code);
        }
    }
}

using CodeGenerator.Command;
using CodeGenerator.Core;
using CodeGenerator.Generator;
namespace CodeGenerator.Hand
{
    public partial class Handler : ICommandHandler<OpengaussCmd, CmdResCode>
    {
        public ResultMessage<CmdResCode> Handle(OpengaussCmd command)
        {
            foreach (var item in command.Classes)
            {
                ClassGenerator.CreateClasses(item, command.Config);
            }

            return ResultMessage<CmdResCode>.Successful("生成完毕", command.Code);
        }

        public ResultMessage<CmdResCode> HandleSql(OpengaussCmd command)
        {
            return ResultMessage<CmdResCode>.Successful("TODO", command.Code);
        }
    }
}

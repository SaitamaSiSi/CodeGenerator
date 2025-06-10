//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:20:13</date>
//------------------------------------------------------------------------------

namespace CodeGenerator.Core
{
    public interface ICommandHandler { }

    public interface ICommandHandler<TIn, TOut> : ICommandHandler
        where TIn : class, ICommand
        //where TOut : class, ICommand
    {
        ResultMessage<TOut> Handle(TIn command);
        ResultMessage<TOut> HandleSql(TIn command);
    }
}

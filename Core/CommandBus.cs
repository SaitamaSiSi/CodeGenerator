//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:24:08</date>
//------------------------------------------------------------------------------

using CodeGenerator.Hand;

namespace CodeGenerator.Core
{
    public static class CommandBus
    {
        static ICommandHandler<TIn, TOut> GetHandler<TIn, TOut>(TIn command)
            where TIn : class, ICommand
        {
            var handler = new Handler();
            if (handler is ICommandHandler<TIn, TOut>)
            {
                return handler as ICommandHandler<TIn, TOut>;
            }
            return null;
        }

        public static ResultMessage<TOut> Execute<TIn, TOut>(TIn command)
            where TIn : class, ICommand
        {
            if (command == null)
            {
                return ResultMessage<TOut>.Fail($"参数异常, command不能为空。", default(TOut));
            }

            var handler = GetHandler<TIn, TOut>(command);

            if (handler == null)
            {
                return ResultMessage<TOut>.Fail($"实现方法不能为空（不支持该方法）。", default(TOut), -1);
            }

            return handler.Handle(command);
        }

        public static ResultMessage<TOut> ExecuteSql<TIn, TOut>(TIn command)
            where TIn : class, ICommand
        {
            if (command == null)
            {
                return ResultMessage<TOut>.Fail($"参数异常, command不能为空。", default(TOut));
            }

            var handler = GetHandler<TIn, TOut>(command);

            if (handler == null)
            {
                return ResultMessage<TOut>.Fail($"实现方法不能为空（不支持该方法）。", default(TOut), -1);
            }

            return handler.HandleSql(command);
        }
    }
}

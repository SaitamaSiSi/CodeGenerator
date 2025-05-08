using CodeGenerator.Core;

namespace CodeGenerator.Command
{
    public class OpengaussCmd : CommandBase, ICommand
    {
        public OpengaussCmd()
        {
            Command = Zyh.Common.Entity.DatabaseType.OpenGauss;
        }
    }
}

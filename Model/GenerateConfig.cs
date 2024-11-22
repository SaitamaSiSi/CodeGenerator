//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/22 14:19:25</date>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CodeGenerator.Model
{
    public class GenerateConfig : BaseConfig
    {
        public string ClassNameSpace { get; set; } = "Zyh.Common.Entity";

        public string ProviderNameSpace { get; set; } = "Zyh.Common.Provider";

        public string ServiceNameSpace { get; set; } = "Zyh.Common.Service";

        public List<string> RemovePre { get; set; } = new List<string>() { "T_SYS_,T_LED_" };
    }
}

//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/14 17:20:20</date>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Zyh.Common.Entity;

namespace CodeGenerator.Model
{
    public class ClassParam
    {
        public string TableName { get; set; } = string.Empty;

        public DatabaseType Database { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public string ClassCom { get; set; } = string.Empty;

        public string ClassNameSpace { get; set; } = "Zyh.Common.Entity";

        public string ProviderNameSpace { get; set; } = "Zyh.Common.Provider";

        public string ServiceNameSpace { get; set; } = "Zyh.Common.Service";


        public List<string> AddNameSpace { get; set; } = new List<string>();

        public List<ColumnParam> Parameters = new List<ColumnParam>();

        public List<string> GetNames(bool withAutoCol = true, bool withPrimaryKey = true)
        {
            if (!withAutoCol && withPrimaryKey)
            {
                return Parameters.Where(m => !m.IsAutoIncrement).Select(m => m.Name).ToList();
            }
            else if (withAutoCol && !withPrimaryKey)
            {
                return Parameters.Where(m => !m.IsPrimaryKey).Select(m => m.Name).ToList();
            }
            else if (!withAutoCol && !withPrimaryKey)
            {
                return Parameters.Where(m => !m.IsAutoIncrement && !m.IsPrimaryKey).Select(m => m.Name).ToList();
            }
            return Parameters.Select(m => m.Name).ToList();
        }

        public List<string> GetPrimaryKeys()
        {
            return Parameters.Where(m => m.IsPrimaryKey).Select(m => m.Name).ToList();
        }
    }
}

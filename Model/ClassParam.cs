//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/14 17:20:20</date>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator.Model
{
    public class ClassParam
    {
        public string TableName { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string ClassCom { get; set; } = string.Empty;

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

        public string GetPrimaryAutoIncKey()
        {
            return Parameters.Where(m => m.IsPrimaryKey && m.IsAutoIncrement).Select(m => m.Name).FirstOrDefault() ?? string.Empty;
        }
    }
}

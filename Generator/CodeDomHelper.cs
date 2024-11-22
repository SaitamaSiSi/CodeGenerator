//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/14 14:19:43</date>
//------------------------------------------------------------------------------

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zyh.Common.Entity;

namespace CodeGenerator.Generator
{
    public class CodeDomHelper
    {
        #region 辅助方法

        protected static string GetSpace(int num)
        {
            string space = string.Empty;

            for (int i = 0; i < num; i++)
            {
                space += "    ";
            }

            return space;
        }

        protected static string GetNewLine(int num)
        {
            return Environment.NewLine + GetSpace(num);
        }

        public static string ConvertToCamelCase(string str, bool isFirstLower = false, List<string>? delPre = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (delPre != null)
            {
                for (int i = 0; i < delPre.Count; i++)
                {
                    string preName = delPre[i].ToUpper();
                    if (str.StartsWith(preName))
                    {
                        str = str.Replace(preName, "");
                        break;
                    }
                }
            }

            // 使用 StringBuilder 来构建新的字符串
            StringBuilder sb = new StringBuilder();

            // 将第一个字符之后的每个字符的首字母大写
            bool isFirst = true;
            foreach (char c in str)
            {
                if (isFirst && char.IsLetter(c))
                {
                    if (isFirstLower)
                    {
                        sb.Append(char.ToLower(c));
                        isFirstLower = false;
                    }
                    else
                    {
                        sb.Append(char.ToUpper(c));
                    }
                    isFirst = false;
                }
                else if (char.IsLetter(c) && char.IsUpper(c))
                {
                    sb.Append(char.ToLower(c));
                }
                else
                {
                    isFirst = true;
                }
            }

            return sb.ToString();
        }

        protected static string DmToCSharpByType(string dmDataType)
        {
            switch (dmDataType.ToUpper())
            {
                case "CHAR":
                case "VARCHAR":
                case "VARCHAR2":
                case "CLOB":
                case "TEXT":
                default:
                    return "String";
                case "SMALLINT":
                    return "Int16";
                case "DECIMAL":
                    return "Decimal";
                case "NUMBER":
                case "INT":
                case "INTEGER":
                    return "Int32";
                case "BIGINT":
                    return "Int64";
                case "FLOAT":
                    return "Float";
                case "DOUBLE":
                    return "Double";
                case "DATE":
                case "DATETIME":
                case "TIMESTAMP":
                    return "DateTime";
                case "BOOLEAN":
                case "TINYINT":
                case "BIT":
                    return "Boolean";
                case "BLOB":
                    return "Byte[]";
            }
        }

        protected static string MysqlToCSharpByType(string mysqlDataType)
        {
            switch (mysqlDataType.ToUpper())
            {
                case "CHAR":
                case "VARCHAR":
                case "VARCHAR2":
                case "CLOB":
                case "TEXT":
                case "JSON":
                default:
                    return "String";
                case "SMALLINT":
                    return "Int16";
                case "DECIMAL":
                    return "Decimal";
                case "NUMBER":
                case "INT":
                case "INTEGER":
                    return "Int32";
                case "BIGINT":
                    return "Int64";
                case "FLOAT":
                    return "Float";
                case "DOUBLE":
                    return "Double";
                case "DATE":
                case "DATETIME":
                case "TIMESTAMP":
                    return "DateTime";
                case "BOOLEAN":
                case "TINYINT":
                case "BIT":
                    return "Boolean";
                case "BLOB":
                    return "Byte[]";
            }
        }

        protected static string DmToDbType(string dmDataType)
        {
            switch (dmDataType.ToUpper())
            {
                case "CHAR":
                    return "DmDbType.Char";
                case "VARCHAR":
                case "VARCHAR2":
                default:
                    return "DmDbType.VarChar";
                case "TEXT":
                    return "DmDbType.Text";
                case "CLOB":
                    return "DmDbType.Clob";
                case "SMALLINT":
                    return "DmDbType.Int16";
                case "DECIMAL":
                    return "DmDbType.Decimal";
                case "NUMBER":
                case "INT":
                case "INTEGER":
                    return "DmDbType.Int32";
                case "BIGINT":
                    return "DmDbType.Int64";
                case "FLOAT":
                    return "DmDbType.Float";
                case "DOUBLE":
                    return "DmDbType.Double";
                case "DATE":
                case "TIMESTAMP":
                    return "DmDbType.DateTime";
                case "BOOLEAN":
                case "TINYINT":
                case "BIT":
                    return "DmDbType.Byte";
                case "BLOB":
                    return "DmDbType.Blob";
            }
        }

        protected static string MysqlToDbType(string mysqlDataType)
        {
            switch (mysqlDataType.ToUpper())
            {
                case "CHAR":
                    return "MySqlDbType.VarChar";
                case "VARCHAR":
                case "VARCHAR2":
                default:
                    return "MySqlDbType.String";
                case "TEXT":
                    return "MySqlDbType.Text";
                case "SMALLINT":
                    return "MySqlDbType.Int16";
                case "DECIMAL":
                    return "MySqlDbType.Decimal";
                case "NUMBER":
                case "INT":
                case "INTEGER":
                    return "MySqlDbType.Int32";
                case "BIGINT":
                    return "MySqlDbType.Int64";
                case "FLOAT":
                    return "MySqlDbType.Float";
                case "DOUBLE":
                    return "MySqlDbType.Double";
                case "DATE":
                case "TIMESTAMP":
                    return "MySqlDbType.DateTime";
                case "BOOLEAN":
                case "TINYINT":
                case "BIT":
                    return "MySqlDbType.Byte";
                case "BLOB":
                    return "MySqlDbType.Blob";
            }
        }

        #endregion

        #region 类相关操作

        public static string GetDirName(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.Dm:
                    return "Dm";
                case DatabaseType.Mysql:
                    return "Mysql";
                default:
                    return string.Empty;
            }
        }

        public static void SaveClass(CodeCompileUnit unit, string className, string addParam, string preDir, string lastDir, bool isGenerate = true)
        {
            //生成C#脚本("VisualBasic"：VB脚本)
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            //代码风格:大括号的样式{}
            options.BracingStyle = "C";
            //是否在字段、属性、方法之间添加空白行
            options.BlankLinesBetweenMembers = true;
            // options.ElseOnClosing = false;
            // options.IndentString = "    ";

            //输出文件路径 Generated/Inherit
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", preDir, lastDir);
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string outputFile = Path.Combine(outputPath, className + (isGenerate ? $"{addParam}.generated.cs" : $"{addParam}.cs"));
            //保存
            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                //为指定的代码文档对象模型(CodeDOM) 编译单元生成代码并将其发送到指定的文本编写器，使用指定的选项。(官方解释)
                //将自定义代码编译器(代码内容)、和代码格式写入到sw中
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
            }
        }

        #endregion

    }
}

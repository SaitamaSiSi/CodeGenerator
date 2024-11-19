//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/14 14:19:43</date>
//------------------------------------------------------------------------------

using CodeGenerator.Model;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGenerator.Core
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

        protected static string ConvertToCamelCase(string str, bool isFirstLower = false)
        {
            if (string.IsNullOrEmpty(str))
                return str;

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

        protected static string GetCSharpType(string dmDataType)
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
                case "NUMBER":
                case "DECIMAL":
                case "INT":
                case "INTEGER":
                    return "Int32";
                case "FLOAT":
                case "DOUBLE PRECISION":
                    return "Double";
                case "DATE":
                case "TIMESTAMP":
                    return "DateTime";
                case "BOOLEAN":
                case "TINYINT":
                    return "Boolean";
            }
        }

        protected static string GetDmDbType(string dmDataType)
        {
            switch (dmDataType.ToUpper())
            {
                case "CHAR":
                    return "Dm.DmDbType.Char";
                case "VARCHAR":
                case "VARCHAR2":
                default:
                    return "DmDbType.VarChar";
                case "TEXT":
                    return "DmDbType.VarChar";
                case "CLOB":
                    return "DmDbType.Clob";
                case "NUMBER":
                case "INT":
                case "INTEGER":
                    return "Dm.DmDbType.Int32";
                case "DECIMAL":
                    return "Dm.DmDbType.Decimal";
                case "FLOAT":
                    return "Dm.DmDbType.Float";
                case "DOUBLE PRECISION":
                    return "Dm.DmDbType.Double";
                case "DATE":
                case "TIMESTAMP":
                    return "Dm.DmDbType.DateTime";
                case "BOOLEAN":
                case "TINYINT":
                    return "Dm.DmDbType.Byte";
            }
        }

        #endregion

        #region 类相关操作

        public static void SaveClass(CodeCompileUnit unit, string className, string addParam, bool isGenerate = true)
        {
            //生成C#脚本("VisualBasic"：VB脚本)
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            //代码风格:大括号的样式{}
            options.BracingStyle = "C";
            //是否在字段、属性、方法之间添加空白行
            options.BlankLinesBetweenMembers = true;

            //输出文件路径 Generated/Inherit
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generated");
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

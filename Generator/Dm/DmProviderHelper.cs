//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/15 14:40:26</date>
//------------------------------------------------------------------------------

using CodeGenerator.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace CodeGenerator.Generator.Dm
{
    public class DmProviderHelper : CodeDomHelper
    {
        public static string PreChar = ":";
        public const string SqlProvierBase = "SqlProviderBase";
        public const string SqlProvier = "SqlProvider";

        #region 私有Provider基础方法

        private static string TableName(string tableName)
        {
            return $"protected const string TableName = \"{tableName}\";";
        }

        private static string Exists_DataDict_Sql(string className, string tableName, List<string> tableKeys)
        {
            string sql = @$"SELECT COUNT(*) FROM {tableName}
WHERE";
            for (int i = 0; i < tableKeys.Count; i++)
            {
                if (i != 0)
                {
                    sql += " AND";
                }
                sql += $" {tableKeys[i]}={PreChar}{ConvertToCamelCase(tableKeys[i])}";
            }
            return $"protected const string Exists_{className}_Sql = @\"{sql}\";";
        }

        private static string Get_DataDict_Sql(string className, string tableName, List<string> tableKeys)
        {
            string sql = @$"SELECT * FROM {tableName}
WHERE";
            for (int i = 0; i < tableKeys.Count; i++)
            {
                if (i != 0)
                {
                    sql += " AND";
                }
                sql += $" {tableKeys[i]}={PreChar}{ConvertToCamelCase(tableKeys[i])}";
            }
            return $"protected const string Get_{className}_Sql = @\"{sql}\";";
        }

        private static string Find_DataDict_Sql(string className, string tableName)
        {
            string sql = @$"SELECT * FROM {tableName} WHERE 1=1";
            return $"protected const string Find_{className}_Sql = @\"{sql}\";";
        }

        private static string GetPager_DataDict_Sql(string className, string tableName)
        {
            string sql = @$"SELECT *, ROW_NUMBER() OVER(ORDER BY NOW())AS RowIndex FROM {tableName} WHERE 1=1";
            return $"protected const string GetPager_{className}_Sql = @\"{sql}\";";
        }

        private static string Insert_DataDict_Sql(string className, string tableName, List<string> columns)
        {
            string sql = $"INSERT INTO {tableName}{Environment.NewLine}VALUES{Environment.NewLine}";

            int index = 0;
            sql += "(";
            foreach (var col in columns)
            {
                sql += $"{PreChar}{ConvertToCamelCase(col)}";
                if (index < columns.Count - 1)
                {
                    sql += ", ";

                }
                index++;
            }
            sql += ")";

            return $"protected const string Insert_{className}_Sql = @\"{sql}\";";
        }

        private static string Update_DataDict_Sql(string className, string tableName, List<string> tableKeys, List<string> columns)
        {
            string sql = $"UPDATE {tableName}" + Environment.NewLine;

            int index = 1;
            sql += "SET";
            foreach (var col in columns)
            {
                sql += $" {col}={PreChar}{ConvertToCamelCase(col)}";
                if (index != columns.Count)
                {
                    sql += ",";

                }
                index++;
            }

            sql = @$"{sql}
WHERE";
            for (int i = 0; i < tableKeys.Count; i++)
            {
                if (i != 0)
                {
                    sql += " AND";
                }
                sql += $" {tableKeys[i]}={PreChar}{ConvertToCamelCase(tableKeys[i])}";
            }
            return $"protected const string Update_{className}_Sql = @\"{sql}\";";
        }

        private static string Delete_DataDict_Sql(string className, string tableName, List<string> tableKeys)
        {
            string sql = @$"DELETE FROM {tableName}
WHERE";
            for (int i = 0; i < tableKeys.Count; i++)
            {
                if (i != 0)
                {
                    sql += " AND";
                }
                sql += $" {tableKeys[i]}={PreChar}{ConvertToCamelCase(tableKeys[i])}";
            }
            return $"protected const string Delete_{className}_Sql = @\"{sql}\";";
        }

        private static string GetReaderSentence(ColumnParam column, ref int index, bool flag = false)
        {
            string cSharpType = DmToCSharpByType(column.Type);
            string sentence = $"{(string.Equals(column.IsNullable, "Y") ? $"({cSharpType}?)" : "")}";

            switch (cSharpType)
            {
                case "String":
                case "Int32":
                case "Double":
                case "Datetime":
                default:
                    {
                        if (flag)
                        {
                            sentence += $"reader.Get{cSharpType}(reader.GetOrdinal(\"{column.Name}\"))";
                        }
                        else
                        {
                            sentence += $"reader.Get{cSharpType}(start + {index})";
                        }
                        break;
                    }
                case "Boolean":
                    {
                        if (flag)
                        {
                            sentence += $"Convert.ToBoolean(reader.GetByte(reader.GetOrdinal(\"{column.Name}\")))";
                        }
                        else
                        {
                            sentence += $"Convert.ToBoolean(reader.GetByte(start + {index}))";
                        }
                        break;
                    }
            }

            string colName = column.Name; // ConvertToCamelCase(column.Name);
            string retStr = $"ent.{colName} = reader[{(flag ? $"\"{colName}\"" : $"(start + {index})")}] == {typeof(DBNull).Name}.Value ? default({cSharpType}{(string.Equals(column.IsNullable, "Y") ? "?" : "")}) : {sentence};";
            index++;
            return retStr;
        }

        private static CodeMemberMethod GetProviderFill1(string entityName, List<ColumnParam> columns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Fill";
            //访问类型
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDataReader).Name, "reader"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int).Name, "start"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int).Name, "length")
            {
                Direction = FieldDirection.Out
            });
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(entityName);
            //设置返回值
            string retStr = GetSpace(3) + $"var ent = new {entityName}();" + Environment.NewLine + GetSpace(3) + $"length = {columns.Count};";

            int index = 0;
            foreach (var col in columns)
            {
                string sentence = GetReaderSentence(col, ref index);
                retStr += Environment.NewLine + GetSpace(3) + sentence;
            }
            retStr += Environment.NewLine + GetSpace(3) + "return ent;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderFill2(string entityName, List<ColumnParam> columns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Fill";
            //访问类型
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDataReader).Name, "reader"));
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(entityName);
            //设置返回值
            string retStr = GetSpace(3) + $"var ent = new {entityName}();";

            int index = 0;
            foreach (var col in columns)
            {
                string sentence = GetReaderSentence(col, ref index, true);
                retStr += Environment.NewLine + GetSpace(3) + sentence;
            }
            retStr += Environment.NewLine + GetSpace(3) + "return ent;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderBuildParameters1(string entityName, List<ColumnParam> columns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "BuildParameters";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(entityName, "ent"));
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(DbParameter[]).Name);
            //设置返回值
            string retStr = GetSpace(3) + $"if (ent == null)"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return null;"
                + Environment.NewLine + GetSpace(3) + "}";

            int index = 0;
            retStr += Environment.NewLine + GetSpace(3) + $"var paramList = new DmParameter[{columns.Count}];";
            foreach (var col in columns)
            {
                string colName = ConvertToCamelCase(col.Name);
                retStr += Environment.NewLine + GetSpace(3) + $"paramList[{index++}] = new DmParameter(\"{colName}\", {DmToDbType(col.Type)}) " + "{" + $" Value = ent.{col.Name}, Direction = ParameterDirection.Input " + "};";
            }

            retStr += Environment.NewLine + GetSpace(3) + "return paramList;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderBuildParameters2(string entityName, List<ColumnParam> columns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "BuildParameters";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression($"IEnumerable<{entityName}>", "list"));
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(DbParameter[]).Name);
            //设置返回值
            string retStr = GetSpace(3) + $"if (list == null || !list.Any())"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return null;"
                + Environment.NewLine + GetSpace(3) + "}";

            retStr += Environment.NewLine + GetSpace(3) + $"var paramList = new List<DmParameter>(list.Count() * {columns.Count});"
                + Environment.NewLine + GetSpace(3) + "var index = 0;"
                + Environment.NewLine + GetSpace(3) + "foreach (var ent in list)"
                 + Environment.NewLine + GetSpace(3) + "{";
            foreach (var col in columns)
            {
                string colName = col.Name; // ConvertToCamelCase(col.Name);
                retStr += Environment.NewLine + GetSpace(4) + $"paramList.Add(new DmParameter($\"{col.Name.ToLower()}_" + "{index}\", " + $"{DmToDbType(col.Type)}) " + "{" + $" Value = ent.{colName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "return paramList.ToArray();";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderBuildParametersForNonKey(string entityName, List<ColumnParam> columns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "BuildParametersForNonKey";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(entityName, "ent"));
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(DbParameter[]).Name);
            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + $"var paramList = new DmParameter[{columns.Count}];";
            int index = 0;
            foreach (var col in columns)
            {
                string colName = col.Name; // ConvertToCamelCase(col.Name);
                retStr += Environment.NewLine + GetSpace(3) + $"paramList[{index++}] = new DmParameter(\"{colName}\", {DmToDbType(col.Type)}) " + "{" + $" Value = ent.{colName}, Direction = ParameterDirection.Input " + "};";
            }

            retStr += Environment.NewLine + GetSpace(3) + "return paramList;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderExists(string className, List<ColumnParam> keyColumns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Exists";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(bool).Name);

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Exists_{className}_Sql);";
            //添加一个参数
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = DmToCSharpByType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                string dmDbType = DmToDbType(keyColumn.Type);
                retStr += Environment.NewLine + GetSpace(3) + $"cmd.Parameters.Add(new DmParameter(\"{colName}\", {dmDbType}) " + "{" + $" Value = {paramName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += Environment.NewLine + GetSpace(3) + "var result = DataContextObject.ExecuteScalar(cmd);"
                + Environment.NewLine + GetSpace(3) + "return Convert.ToInt32(result) > 0;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderGet(string className, List<ColumnParam> keyColumns)
        {
            string entityName = className + DmEntityHelper.Entity;
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Get";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(entityName);

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + $"{entityName} result = null;"
                + Environment.NewLine + GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Get_{className}_Sql);";
            //添加一个参数
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = DmToCSharpByType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                string dmDbType = DmToDbType(keyColumn.Type);
                retStr += Environment.NewLine + GetSpace(3) + $"cmd.Parameters.Add(new DmParameter(\"{colName}\", {dmDbType}) " + "{" + $" Value = {paramName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += Environment.NewLine + GetSpace(3) + "using (var reader = DataContextObject.ExecuteReader(cmd))"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "if (reader.Read())"
                + Environment.NewLine + GetSpace(4) + "{"
                + Environment.NewLine + GetSpace(5) + "result = Fill(reader);"
                + Environment.NewLine + GetSpace(4) + "}"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "return result;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderFindAll(string className)
        {
            string entityName = className + DmEntityHelper.Entity;
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "FindAll";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference($"List<{entityName}>");
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string).Name, "whereClause"));

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + $"var result = new List<{entityName}>();"
                + Environment.NewLine + GetSpace(3) + $"var sql = Find_{className}_Sql + whereClause;"
                + Environment.NewLine + GetSpace(3) + "using var cmd = DatabaseObject.GetSqlStringCommand(sql);";


            retStr += Environment.NewLine + GetSpace(3) + "using (var reader = DataContextObject.ExecuteReader(cmd))"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "while (reader.Read())"
                + Environment.NewLine + GetSpace(4) + "{"
                + Environment.NewLine + GetSpace(5) + "var ent = Fill(reader);"
                + Environment.NewLine + GetSpace(5) + "result.Add(ent);"
                + Environment.NewLine + GetSpace(4) + "}"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "return result;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderGetPager(string className)
        {
            string entityName = className + DmEntityHelper.Entity;
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "GetPager";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference($"List<{entityName}>");
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int).Name, "pageIndex"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int).Name, "pageSize"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string).Name, "whereClause"));

            //设置返回值
            string retStr =
                Environment.NewLine + GetSpace(3) + $"var result = new List<{entityName}>();"
                + Environment.NewLine + GetSpace(3) + "Int32 tempIndex = pageIndex <= 0 ? 1 : pageIndex;"
                + Environment.NewLine + GetSpace(3) + "Int32 tempSize = pageSize < 0 ? 0 : pageSize;"
                + Environment.NewLine + GetSpace(3) + "Int32 startIndex = (tempIndex - 1) * tempSize + 1;"
                + Environment.NewLine + GetSpace(3) + "Int32 endIndex = tempIndex * tempSize;"
                + Environment.NewLine + GetSpace(3) + "var pageSql = $\"SELECT * FROM ({" + $"GetPager_{className}_Sql" + " + whereClause}) AS subquery where RowIndex between {startIndex} and {endIndex}\";"
                + Environment.NewLine + GetSpace(3) + "using var cmd = DatabaseObject.GetSqlStringCommand(pageSql);"
                + Environment.NewLine + GetSpace(3) + "using (var reader = DataContextObject.ExecuteReader(cmd))"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "while (reader.Read())"
                + Environment.NewLine + GetSpace(4) + "{"
                + Environment.NewLine + GetSpace(5) + "var ent = Fill(reader);"
                + Environment.NewLine + GetSpace(5) + "result.Add(ent);"
                + Environment.NewLine + GetSpace(4) + "}"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "return result;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderAdd1(string className, List<ColumnParam> keyColumns)
        {
            string entityName = className + DmEntityHelper.Entity;
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Add";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(int).Name);
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(entityName, "ent"));

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Insert_{className}_Sql) as DmCommand;";

            foreach (var keyColumn in keyColumns)
            {
                string colName = ConvertToCamelCase(keyColumn.Name);
                string dmDbType = DmToDbType(keyColumn.Type);
                retStr += Environment.NewLine + GetSpace(3) + $"cmd.Parameters.Add(new DmParameter(\"{colName}\", {dmDbType}) " + "{" + $" Value = ent.{keyColumn.Name}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += Environment.NewLine + GetSpace(3) + "var nonKeyParams = BuildParametersForNonKey(ent);"
                + Environment.NewLine + GetSpace(3) + "cmd.Parameters.AddRange(nonKeyParams);"
                + Environment.NewLine + GetSpace(3) + "var execResult = DataContextObject.ExecuteNonQuery(cmd);";

            retStr += Environment.NewLine + GetSpace(3) + "return execResult;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderAdd2(string entityName, string tableName, List<ColumnParam> columns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Add";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression($"IEnumerable<{entityName}>", "list"));
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(int).Name);
            //设置返回值
            string retStr = GetSpace(3) + $"if (list == null || !list.Any())"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return 0;" + Environment.NewLine + GetSpace(3) + "}";

            retStr += Environment.NewLine + GetSpace(3) + $"const string InsertIntoClause = \"INSERT INTO {tableName} VALUES \";"
                + Environment.NewLine + GetSpace(3) + "var valueClauses = new List<String>(list.Count());"
                + Environment.NewLine + GetSpace(3) + "var index = 0;"
                 + Environment.NewLine + GetSpace(3) + "foreach (var ent in list)"
                 + Environment.NewLine + GetSpace(3) + "{";

            retStr += Environment.NewLine + GetSpace(4)
                + "var clause = $\"(";
            int index = 0;
            foreach (var col in columns)
            {
                retStr += $":{col.Name.ToLower()}_" + "{index}";
                if (index < columns.Count - 1)
                {
                    retStr += ", ";
                }
                index++;
            }
            retStr += ")\";";

            retStr += Environment.NewLine + GetSpace(4) + "valueClauses.Add(clause);"
                + Environment.NewLine + GetSpace(4) + "index++;"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "var sql = InsertIntoClause + string.Join(\",\", valueClauses) + \";\";"
                + Environment.NewLine + GetSpace(3) + "using var cmd = DatabaseObject.GetSqlStringCommand(sql) as DmCommand;"
                + Environment.NewLine + GetSpace(3) + "var parameters = BuildParameters(list);"
                + Environment.NewLine + GetSpace(3) + "cmd.Parameters.AddRange(parameters);"
                + Environment.NewLine + GetSpace(3) + "return DataContextObject.ExecuteNonQuery(cmd);";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderUpdate(string className)
        {
            string entityName = className + DmEntityHelper.Entity;
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Update";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(entityName, "ent"));
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(int).Name);
            //设置返回值
            string retStr = GetSpace(3) + "var parameters = BuildParameters(ent);"
                + Environment.NewLine + GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Update_{className}_Sql) as DmCommand;"
                + Environment.NewLine + GetSpace(3) + "cmd.Parameters.AddRange(parameters);" + Environment.NewLine + GetSpace(3) + "return DataContextObject.ExecuteNonQuery(cmd);";

            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderDelete(string className, List<ColumnParam> keyColumns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Delete";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(typeof(int).Name);

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Delete_{className}_Sql);";

            //添加一个参数
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = DmToCSharpByType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                string dmDbType = DmToDbType(keyColumn.Type);
                retStr += Environment.NewLine + GetSpace(3) + $"cmd.Parameters.Add(new DmParameter(\"{colName}\", {dmDbType}) " + "{" + $" Value = {paramName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += Environment.NewLine + GetSpace(3) + "return DataContextObject.ExecuteNonQuery(cmd);";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        #endregion

        #region 公有Provider方法

        public static CodeTypeDeclaration CreateGenerateProviderClass(CodeCompileUnit unit, ClassParam param)
        {
            CodeNamespace myNamespace = new CodeNamespace(param.ProviderNameSpace + ".Base");

            //导入必要的命名空间引用
            myNamespace.Imports.Add(new CodeNamespaceImport("System"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Data"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Data.Common"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            myNamespace.Imports.Add(new CodeNamespaceImport(param.ClassNameSpace));
            myNamespace.Imports.Add(new CodeNamespaceImport("Dm"));

            //Code:代码体
            CodeTypeDeclaration myClass = new CodeTypeDeclaration(param.ClassName + SqlProvierBase);
            //指定为类
            myClass.IsClass = true;
            myClass.IsPartial = true;
            //设置类的访问类型
            myClass.TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract;
            //把这个类放在这个命名空间下
            myNamespace.Types.Add(myClass);
            //把该命名空间加入到编译器单元的命名空间集合中
            unit.Namespaces.Add(myNamespace);

            CodeTypeReference iTypeRef = new CodeTypeReference($"{SqlProvierBase}<{param.ClassName}{DmEntityHelper.Entity}>");
            myClass.BaseTypes.Add(iTypeRef);

            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + TableName(param.TableName)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Exists_DataDict_Sql(param.ClassName, param.TableName, param.TableKey)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Get_DataDict_Sql(param.ClassName, param.TableName, param.TableKey)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Find_DataDict_Sql(param.ClassName, param.TableName)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + GetPager_DataDict_Sql(param.ClassName, param.TableName)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Insert_DataDict_Sql(param.ClassName, param.TableName, param.GetNames())));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Update_DataDict_Sql(param.ClassName, param.TableName, param.TableKey, param.GetNames())));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Delete_DataDict_Sql(param.ClassName, param.TableName, param.TableKey)));

            //添加构造方法
            CodeConstructor constructor1 = new CodeConstructor();
            constructor1.Attributes = MemberAttributes.Public;
            // 调用基类的无参数构造函数
            constructor1.BaseConstructorArgs.Add(new CodeSnippetExpression());
            // 创建第二个构造函数（有一个String类型的参数）
            CodeConstructor constructor2 = new CodeConstructor
            {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression(typeof(string), "connectionName") }
            };
            constructor2.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connectionName")); // 调用基类的一个参数的构造函数
            myClass.Members.Add(constructor1);
            myClass.Members.Add(constructor2);

            return myClass;
        }

        public static CodeTypeDeclaration CreateNormalProviderClass(CodeCompileUnit unit, ClassParam param)
        {
            unit.Namespaces.Add(new CodeNamespace());
            //导入必要的命名空间引用
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Data"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Linq"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Text"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport(param.ClassNameSpace));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport(param.ProviderNameSpace + ".Base"));

            CodeNamespace myNamespace = new CodeNamespace(param.ProviderNameSpace);

            //Code:代码体
            CodeTypeDeclaration myClass = new CodeTypeDeclaration(param.ClassName + SqlProvier);
            //指定为类
            myClass.IsClass = true;
            myClass.IsPartial = true;
            //设置类的访问类型
            myClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            //把这个类放在这个命名空间下
            myNamespace.Types.Add(myClass);
            //把该命名空间加入到编译器单元的命名空间集合中
            unit.Namespaces.Add(myNamespace);

            CodeTypeReference iTypeRef = new CodeTypeReference(param.ClassName + SqlProvierBase);
            myClass.BaseTypes.Add(iTypeRef);

            // myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + "#region Constructor"));

            //添加构造方法
            CodeConstructor constructor1 = new CodeConstructor();
            constructor1.Attributes = MemberAttributes.Public;
            // 调用基类的无参数构造函数
            constructor1.BaseConstructorArgs.Add(new CodeSnippetExpression());
            // 创建第二个构造函数（有一个String类型的参数）
            CodeConstructor constructor2 = new CodeConstructor
            {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression(typeof(string), "connectionName") }
            };
            constructor2.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connectionName")); // 调用基类的一个参数的构造函数
            myClass.Members.Add(constructor1);
            myClass.Members.Add(constructor2);

            // myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + "#endregion"));

            return myClass;
        }

        public static void CreateProviderMethod(CodeTypeDeclaration myClass, string className, string tableName, List<ColumnParam> columns, List<string> keyColumnNames)
        {
            string entityName = className + DmEntityHelper.Entity;

            // CRUD
            List<ColumnParam> keyColumns = columns.Where(m => keyColumnNames.Contains(m.Name.ToUpper())).ToList();
            CodeMemberMethod exists = GetProviderExists(className, keyColumns);
            CodeMemberMethod get = GetProviderGet(className, keyColumns);
            CodeMemberMethod findAll = GetProviderFindAll(className);
            CodeMemberMethod getPager = GetProviderGetPager(className);
            CodeMemberMethod add1 = GetProviderAdd1(className, keyColumns);
            CodeMemberMethod add2 = GetProviderAdd2(entityName, tableName, columns);
            CodeMemberMethod update = GetProviderUpdate(className);
            CodeMemberMethod delete = GetProviderDelete(className, keyColumns);

            // Build Parameters
            CodeMemberMethod buildParameters1 = GetProviderBuildParameters1(entityName, columns);
            CodeMemberMethod buildParameters2 = GetProviderBuildParameters2(entityName, columns);
            CodeMemberMethod buildParametersForNonKey = GetProviderBuildParametersForNonKey(entityName, columns);

            // Fill Data
            CodeMemberMethod fill1 = GetProviderFill1(entityName, columns);
            CodeMemberMethod fill2 = GetProviderFill2(entityName, columns);



            myClass.Members.Add(exists);
            myClass.Members.Add(get);
            myClass.Members.Add(findAll);
            myClass.Members.Add(getPager);
            myClass.Members.Add(add1);
            myClass.Members.Add(add2);
            myClass.Members.Add(update);
            myClass.Members.Add(delete);

            myClass.Members.Add(buildParameters1);
            myClass.Members.Add(buildParameters2);
            myClass.Members.Add(buildParametersForNonKey);

            myClass.Members.Add(fill1);
            myClass.Members.Add(fill2);

        }

        #endregion

    }
}

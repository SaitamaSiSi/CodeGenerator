//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/15 14:40:26</date>
//------------------------------------------------------------------------------

using CodeGenerator.Model;
using Dm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Zyh.Common.Entity;

namespace CodeGenerator.Generator
{
    public class ProviderHelper : CodeDomHelper
    {
        public const string SqlProvierBase = "SqlProviderBase";
        public const string SqlProvier = "SqlProvider";

        #region 私有Provider基础方法

        private static string PreChar(DatabaseType type)
        {
            if (type == DatabaseType.Dm)
            {
                return ":";
            }
            else if (type == DatabaseType.Mysql || type == DatabaseType.OpenGauss)
            {
                return "@";
            }
            return string.Empty;
        }

        private static string CommandStr(DatabaseType type)
        {
            if (type == DatabaseType.Dm)
            {
                return "DmCommand";
            }
            else if (type == DatabaseType.Mysql)
            {
                return "MySqlCommand";
            }
            else if (type == DatabaseType.OpenGauss)
            {
                return "NpgsqlCommand";
            }
            return string.Empty;
        }

        private static string DmParameterStr(DatabaseType type)
        {
            if (type == DatabaseType.Dm)
            {
                return "DmParameter";
            }
            else if (type == DatabaseType.Mysql)
            {
                return "MySqlParameter";
            }
            else if (type == DatabaseType.OpenGauss)
            {
                return "NpgsqlParameter";
            }
            return string.Empty;
        }

        private static string TableName(string tableName)
        {
            return $"protected const string TableName = \"{tableName}\";";
        }

        private static string Exists_DataDict_Sql(string className, string tableName, List<string> tableKeys, DatabaseType type)
        {
            string sql = @$"SELECT COUNT(*) FROM {tableName}
WHERE";
            for (int i = 0; i < tableKeys.Count; i++)
            {
                if (i != 0)
                {
                    sql += " AND";
                }
                sql += $" {tableKeys[i]}={PreChar(type)}{ConvertToCamelCase(tableKeys[i])}";
            }
            return $"protected const string Exists_{className}_Sql = @\"{sql}\";";
        }

        private static string Get_DataDict_Sql(string className, string tableName, List<string> tableKeys, DatabaseType type)
        {
            string sql = @$"SELECT * FROM {tableName}
WHERE";
            for (int i = 0; i < tableKeys.Count; i++)
            {
                if (i != 0)
                {
                    sql += " AND";
                }
                sql += $" {tableKeys[i]}={PreChar(type)}{ConvertToCamelCase(tableKeys[i])}";
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

        private static string Insert_DataDict_Sql(string className, string tableName, List<string> columns, DatabaseType type)
        {
            string sql = $"INSERT INTO {tableName}{Environment.NewLine}(";

            int index = 0;
            foreach (var col in columns)
            {
                sql += col;
                if (index < columns.Count - 1)
                {
                    sql += ", ";
                }
                index++;
            }

            sql += $") VALUES{Environment.NewLine}";

            index = 0;
            sql += "(";
            foreach (var col in columns)
            {
                sql += $"{PreChar(type)}{ConvertToCamelCase(col)}";
                if (index < columns.Count - 1)
                {
                    sql += ", ";

                }
                index++;
            }
            sql += ")";

            return $"protected const string Insert_{className}_Sql = @\"{sql}\";";
        }

        private static string Update_DataDict_Sql(string className, string tableName, List<string> tableKeys, List<string> columns, DatabaseType type)
        {
            string sql = $"UPDATE {tableName}" + Environment.NewLine;

            int index = 1;
            sql += "SET";
            foreach (var col in columns)
            {
                sql += $" {col}={PreChar(type)}{ConvertToCamelCase(col)}";
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
                sql += $" {tableKeys[i]}={PreChar(type)}{ConvertToCamelCase(tableKeys[i])}";
            }
            return $"protected const string Update_{className}_Sql = @\"{sql}\";";
        }

        private static string Delete_DataDict_Sql(string className, string tableName, List<string> tableKeys, DatabaseType type)
        {
            string sql = @$"DELETE FROM {tableName}
WHERE";
            for (int i = 0; i < tableKeys.Count; i++)
            {
                if (i != 0)
                {
                    sql += " AND";
                }
                sql += $" {tableKeys[i]}={PreChar(type)}{ConvertToCamelCase(tableKeys[i])}";
            }
            return $"protected const string Delete_{className}_Sql = @\"{sql}\";";
        }

        private static string GetReaderSentence(ColumnParam column, ref int index, DatabaseType type, bool flag = false)
        {
            string cSharpType = ToCSharpByType(column.Type);
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
                            sentence += (type == DatabaseType.OpenGauss ? $"reader.GetBoolean(reader.GetOrdinal(\"{column.Name}\"))" : $"Convert.ToBoolean(reader.GetByte(reader.GetOrdinal(\"{column.Name}\")))");
                        }
                        else
                        {
                            sentence += (type == DatabaseType.OpenGauss ? $"reader.GetBoolean(start + {index})" : $"Convert.ToBoolean(reader.GetByte(start + {index}))");
                        }
                        break;
                    }
            }

            string colName = column.Name; // ConvertToCamelCase(column.Name);
            string retStr = $"ent.{colName} = reader[{(flag ? $"\"{colName}\"" : $"(start + {index})")}] == {typeof(DBNull).Name}.Value ? default({cSharpType}{(string.Equals(column.IsNullable, "Y") ? "?" : "")}) : {sentence};";
            index++;
            return retStr;
        }

        private static CodeMemberMethod GetProviderFill1(string entityName, List<ColumnParam> columns, DatabaseType type)
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
            string retStr = GetSpace(3) + $"var ent = new {entityName}();"
                + GetNewLine(3) + $"length = {columns.Count};";

            int index = 0;
            foreach (var col in columns)
            {
                string sentence = GetReaderSentence(col, ref index, type);
                retStr += GetNewLine(3) + sentence;
            }
            retStr += GetNewLine(3) + "return ent;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderFill2(string entityName, List<ColumnParam> columns, DatabaseType type)
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
                string sentence = GetReaderSentence(col, ref index, type, true);
                retStr += GetNewLine(3) + sentence;
            }
            retStr += GetNewLine(3) + "return ent;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderBuildParameters1(string entityName, List<ColumnParam> columns, DatabaseType type)
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
                + GetNewLine(3) + "{"
                + GetNewLine(4) + "return null;"
                + GetNewLine(3) + "}";

            int index = 0;
            retStr += GetNewLine(3) + $"var paramList = new {DmParameterStr(type)}[{columns.Count}];";
            foreach (var col in columns)
            {
                string colName = ConvertToCamelCase(col.Name);
                retStr += GetNewLine(3)
                    + $"paramList[{index++}] = new {DmParameterStr(type)}(\"{colName}\", ";
                if (type == DatabaseType.Dm)
                {
                    retStr += DmToDbType(col.Type);
                }
                else if (type == DatabaseType.Mysql)
                {
                    retStr += MysqlToDbType(col.Type);
                }
                else if (type == DatabaseType.OpenGauss)
                {
                    retStr += OpengaussToDbType(col.Type);
                }
                retStr += ") {" + $" Value = ent.{col.Name}, Direction = ParameterDirection.Input " + "};";
            }

            retStr += GetNewLine(3) + "return paramList;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderBuildParameters2(string entityName, List<ColumnParam> columns, DatabaseType type)
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
                + GetNewLine(3) + "{"
                + GetNewLine(4) + "return null;"
                + GetNewLine(3) + "}"
                + GetNewLine(3) + $"var paramList = new List<{DmParameterStr(type)}>(list.Count() * {columns.Count});"
                + GetNewLine(3) + "var index = 0;"
                + GetNewLine(3) + "foreach (var ent in list)"
                 + GetNewLine(3) + "{";
            foreach (var col in columns)
            {
                string colName = col.Name; // ConvertToCamelCase(col.Name);
                retStr += GetNewLine(4) + $"paramList.Add(new {DmParameterStr(type)}($\"{col.Name.ToLower()}_" + "{index}\", ";
                if (type == DatabaseType.Dm)
                {
                    retStr += DmToDbType(col.Type);
                }
                else if (type == DatabaseType.Mysql)
                {
                    retStr += MysqlToDbType(col.Type);
                }
                else if (type == DatabaseType.OpenGauss)
                {
                    retStr += OpengaussToDbType(col.Type);
                }
                retStr += ") {" + $" Value = ent.{colName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += GetNewLine(3) + "}"
                + GetNewLine(3) + "return paramList.ToArray();";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderBuildParametersForNonKey(string entityName, List<ColumnParam> columns, DatabaseType type)
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
            string retStr = GetSpace(3) + $"var paramList = new {DmParameterStr(type)}[{columns.Count}];";
            int index = 0;
            foreach (var col in columns)
            {
                string colName = ConvertToCamelCase(col.Name);
                retStr += GetNewLine(3) + $"paramList[{index++}] = new {DmParameterStr(type)}(\"{colName}\", ";
                if (type == DatabaseType.Dm)
                {
                    retStr += DmToDbType(col.Type);
                }
                else if (type == DatabaseType.Mysql)
                {
                    retStr += MysqlToDbType(col.Type);
                }
                else if (type == DatabaseType.OpenGauss)
                {
                    retStr += OpengaussToDbType(col.Type);
                }
                retStr += ") {" + $" Value = ent.{col.Name}, Direction = ParameterDirection.Input " + "};";
            }

            retStr += GetNewLine(3) + "return paramList;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderExists(string className, List<ColumnParam> keyColumns, DatabaseType type)
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
            string retStr = GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Exists_{className}_Sql);";
            //添加一个参数
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = ToCSharpByType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                string providerDbType = string.Empty;
                if (type == DatabaseType.Dm)
                {
                    providerDbType = DmToDbType(keyColumn.Type);
                }
                else if (type == DatabaseType.Mysql)
                {
                    providerDbType = MysqlToDbType(keyColumn.Type);
                }
                else if (type == DatabaseType.OpenGauss)
                {
                    providerDbType += OpengaussToDbType(keyColumn.Type);
                }
                retStr += GetNewLine(3) + $"cmd.Parameters.Add(new {DmParameterStr(type)}(\"{colName}\", {providerDbType}) " + "{" + $" Value = {paramName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += GetNewLine(3) + "var result = DataContextObject.ExecuteScalar(cmd);"
                + GetNewLine(3) + "return Convert.ToInt32(result) > 0;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderGet(string className, List<ColumnParam> keyColumns, DatabaseType type)
        {
            string entityName = className + EntityHelper.Entity;
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Get";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(entityName);

            //设置返回值
            string retStr = GetSpace(3) + $"{entityName} result = null;"
                + GetNewLine(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Get_{className}_Sql);";
            //添加一个参数
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = ToCSharpByType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                string providerDbType = string.Empty;
                if (type == DatabaseType.Dm)
                {
                    providerDbType = DmToDbType(keyColumn.Type);
                }
                else if (type == DatabaseType.Mysql)
                {
                    providerDbType = MysqlToDbType(keyColumn.Type);
                }
                else if (type == DatabaseType.OpenGauss)
                {
                    providerDbType += OpengaussToDbType(keyColumn.Type);
                }
                retStr += GetNewLine(3) + $"cmd.Parameters.Add(new {DmParameterStr(type)}(\"{colName}\", {providerDbType}) " + "{" + $" Value = {paramName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += GetNewLine(3) + "using (var reader = DataContextObject.ExecuteReader(cmd))"
                + GetNewLine(3) + "{"
                + GetNewLine(4) + "if (reader.Read())"
                + GetNewLine(4) + "{"
                + GetNewLine(5) + "result = Fill(reader);"
                + GetNewLine(4) + "}"
                + GetNewLine(3) + "}"
                + GetNewLine(3) + "return result;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderFindAll(string className)
        {
            string entityName = className + EntityHelper.Entity;
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
            string retStr = GetSpace(3) + $"var result = new List<{entityName}>();"
                + GetNewLine(3) + $"var sql = Find_{className}_Sql + whereClause;"
                + GetNewLine(3) + "using var cmd = DatabaseObject.GetSqlStringCommand(sql);"
                + GetNewLine(3) + "using (var reader = DataContextObject.ExecuteReader(cmd))"
                + GetNewLine(3) + "{"
                + GetNewLine(4) + "while (reader.Read())"
                + GetNewLine(4) + "{"
                + GetNewLine(5) + "var ent = Fill(reader);"
                + GetNewLine(5) + "result.Add(ent);"
                + GetNewLine(4) + "}"
                + GetNewLine(3) + "}"
                + GetNewLine(3) + "return result;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderGetPager(string className)
        {
            string entityName = className + EntityHelper.Entity;
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
            string retStr = GetSpace(3) + $"var result = new List<{entityName}>();"
                + GetNewLine(3) + "Int32 tempIndex = pageIndex <= 0 ? 1 : pageIndex;"
                + GetNewLine(3) + "Int32 tempSize = pageSize < 0 ? 0 : pageSize;"
                + GetNewLine(3) + "Int32 startIndex = (tempIndex - 1) * tempSize + 1;"
                + GetNewLine(3) + "Int32 endIndex = tempIndex * tempSize;"
                + GetNewLine(3) + "var pageSql = $\"SELECT * FROM ({" + $"GetPager_{className}_Sql" + " + whereClause}) AS subquery where RowIndex between {startIndex} and {endIndex}\";"
                + GetNewLine(3) + "using var cmd = DatabaseObject.GetSqlStringCommand(pageSql);"
                + GetNewLine(3) + "using (var reader = DataContextObject.ExecuteReader(cmd))"
                + GetNewLine(3) + "{"
                + GetNewLine(4) + "while (reader.Read())"
                + GetNewLine(4) + "{"
                + GetNewLine(5) + "var ent = Fill(reader);"
                + GetNewLine(5) + "result.Add(ent);"
                + GetNewLine(4) + "}"
                + GetNewLine(3) + "}"
                + GetNewLine(3) + "return result;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderAdd1(string className, DatabaseType type, string primaryAutoIncKey)
        {
            string entityName = className + EntityHelper.Entity;
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
            string addStr = string.Empty;
            if (!string.IsNullOrEmpty(primaryAutoIncKey))
            {
                switch (type)
                {
                    case DatabaseType.Dm:
                        {
                            addStr = GetNewLine(3) + $"ent.{primaryAutoIncKey} = DataContextObject.ExecuteScalar<Int32>(\"Select @@IDENTITY;\");";
                            break;
                        }
                    case DatabaseType.Mysql:
                        {
                            addStr = GetNewLine(3) + $"ent.{primaryAutoIncKey} = (Int32)cmd.LastInsertedId;";
                            break;
                        }
                }

            }
            string retStr = GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Insert_{className}_Sql) as {CommandStr(type)};"
                + GetNewLine(3) + "var nonKeyParams = BuildParametersForNonKey(ent);"
                + GetNewLine(3) + "cmd.Parameters.AddRange(nonKeyParams);"
                + GetNewLine(3) + "var execResult = DataContextObject.ExecuteNonQuery(cmd);"
                + addStr
                + GetNewLine(3) + "return execResult;";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderAdd2(string entityName, string tableName, List<ColumnParam> columns, DatabaseType type)
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
                + GetNewLine(3) + "{"
                + GetNewLine(4) + "return 0;"
                + GetNewLine(3) + "}"
                + GetNewLine(3) + $"const string InsertIntoClause = \"INSERT INTO {tableName} VALUES \";"
                + GetNewLine(3) + "var valueClauses = new List<String>(list.Count());"
                + GetNewLine(3) + "var index = 0;"
                + GetNewLine(3) + "foreach (var ent in list)"
                + GetNewLine(3) + "{";

            retStr += GetNewLine(4) + "var clause = $\"(";
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

            retStr += GetNewLine(4) + "valueClauses.Add(clause);"
                + GetNewLine(4) + "index++;"
                + GetNewLine(3) + "}"
                + GetNewLine(3) + "var sql = InsertIntoClause + string.Join(\",\", valueClauses) + \";\";"
                + GetNewLine(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(sql) as {CommandStr(type)};"
                + GetNewLine(3) + "var parameters = BuildParameters(list);"
                + GetNewLine(3) + "cmd.Parameters.AddRange(parameters);"
                + GetNewLine(3) + "return DataContextObject.ExecuteNonQuery(cmd);";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderUpdate(string className, DatabaseType type)
        {
            string entityName = className + EntityHelper.Entity;
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
                + GetNewLine(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Update_{className}_Sql) as {CommandStr(type)};"
                + GetNewLine(3) + "cmd.Parameters.AddRange(parameters);"
                + GetNewLine(3) + "return DataContextObject.ExecuteNonQuery(cmd);";

            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetProviderDelete(string className, List<ColumnParam> keyColumns, DatabaseType type)
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
            string retStr = GetSpace(3) + $"using var cmd = DatabaseObject.GetSqlStringCommand(Delete_{className}_Sql);";

            //添加一个参数
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = ToCSharpByType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                string providerDbType = string.Empty;
                if (type == DatabaseType.Dm)
                {
                    providerDbType = DmToDbType(keyColumn.Type);
                }
                else if (type == DatabaseType.Mysql)
                {
                    providerDbType = MysqlToDbType(keyColumn.Type);
                }
                else if (type == DatabaseType.OpenGauss)
                {
                    providerDbType = OpengaussToDbType(keyColumn.Type);
                }
                retStr += GetNewLine(3) + $"cmd.Parameters.Add(new {DmParameterStr(type)}(\"{colName}\", {providerDbType}) " + "{" + $" Value = {paramName}, Direction = ParameterDirection.Input " + "});";
            }

            retStr += GetNewLine(3) + "return DataContextObject.ExecuteNonQuery(cmd);";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        #endregion

        #region 公有Provider方法

        public static CodeTypeDeclaration CreateGenerateProviderClass(CodeCompileUnit unit, ClassParam param, DatabaseType type, string classNameSpace, string providerNameSpace)
        {
            CodeNamespace myNamespace = new CodeNamespace(providerNameSpace + ".Base");

            //导入必要的命名空间引用
            myNamespace.Imports.Add(new CodeNamespaceImport("System"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Data"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Data.Common"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            myNamespace.Imports.Add(new CodeNamespaceImport(classNameSpace));
            if (type == DatabaseType.Dm)
            {
                myNamespace.Imports.Add(new CodeNamespaceImport("Dm"));
            }
            else if (type == DatabaseType.Mysql)
            {
                myNamespace.Imports.Add(new CodeNamespaceImport("MySqlConnector"));
            }
            else if (type == DatabaseType.OpenGauss)
            {
                myNamespace.Imports.Add(new CodeNamespaceImport("Npgsql"));
            }

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

            CodeTypeReference iTypeRef = new CodeTypeReference($"{SqlProvierBase}<{param.ClassName}{EntityHelper.Entity}>");
            myClass.BaseTypes.Add(iTypeRef);


            var tableKeys = param.GetPrimaryKeys();
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + "#region SQL" + GetNewLine(2)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + TableName(param.TableName)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Exists_DataDict_Sql(param.ClassName, param.TableName, tableKeys, type)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Get_DataDict_Sql(param.ClassName, param.TableName, tableKeys, type)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Find_DataDict_Sql(param.ClassName, param.TableName)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + GetPager_DataDict_Sql(param.ClassName, param.TableName)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Insert_DataDict_Sql(param.ClassName, param.TableName, param.GetNames(false), type)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Update_DataDict_Sql(param.ClassName, param.TableName, tableKeys, param.GetNames(false, false), type)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetSpace(2) + Delete_DataDict_Sql(param.ClassName, param.TableName, tableKeys, type)));
            myClass.Members.Add(new CodeSnippetTypeMember(GetNewLine(2) + "#endregion "));

            CodeRegionDirective start = new CodeRegionDirective(CodeRegionMode.Start, "Constructor");
            CodeRegionDirective end = new CodeRegionDirective(CodeRegionMode.End, "Constructor");

            //添加构造方法
            CodeConstructor constructor1 = new CodeConstructor();
            constructor1.StartDirectives.Add(start);
            constructor1.Attributes = MemberAttributes.Public;
            // 调用基类的无参数构造函数
            constructor1.BaseConstructorArgs.Add(new CodeSnippetExpression());
            // 创建第二个构造函数（有一个String类型的参数）
            CodeConstructor constructor2 = new CodeConstructor
            {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression(typeof(string), "connectionName") }
            };
            constructor2.EndDirectives.Add(end);
            constructor2.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connectionName")); // 调用基类的一个参数的构造函数
            myClass.Members.Add(constructor1);
            myClass.Members.Add(constructor2);

            return myClass;
        }

        public static CodeTypeDeclaration CreateNormalProviderClass(CodeCompileUnit unit, ClassParam param, string classNameSpace, string providerNameSpace)
        {
            unit.Namespaces.Add(new CodeNamespace());
            //导入必要的命名空间引用
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Data"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Linq"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Text"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport(classNameSpace));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport(providerNameSpace + ".Base"));

            CodeNamespace myNamespace = new CodeNamespace(providerNameSpace);

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

            CodeRegionDirective start = new CodeRegionDirective(CodeRegionMode.Start, "Constructor");
            CodeRegionDirective end = new CodeRegionDirective(CodeRegionMode.End, "Constructor");

            //添加构造方法
            CodeConstructor constructor1 = new CodeConstructor();
            constructor1.StartDirectives.Add(start);
            constructor1.Attributes = MemberAttributes.Public;
            // 调用基类的无参数构造函数
            constructor1.BaseConstructorArgs.Add(new CodeSnippetExpression());
            // 创建第二个构造函数（有一个String类型的参数）
            CodeConstructor constructor2 = new CodeConstructor
            {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression(typeof(string), "connectionName") }
            };
            constructor2.EndDirectives.Add(end);
            constructor2.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connectionName")); // 调用基类的一个参数的构造函数
            myClass.Members.Add(constructor1);
            myClass.Members.Add(constructor2);

            return myClass;
        }

        public static void CreateProviderMethod(CodeTypeDeclaration myClass, string className, string tableName, List<ColumnParam> columns, List<string> keyColumnNames, DatabaseType type, string primaryAutoIncKey)
        {
            string entityName = className + EntityHelper.Entity;

            // CRUD
            List<ColumnParam> keyColumns = columns.Where(m => keyColumnNames.Contains(m.Name.ToUpper())).ToList();
            CodeMemberMethod exists = GetProviderExists(className, keyColumns, type);
            CodeMemberMethod get = GetProviderGet(className, keyColumns, type);
            CodeMemberMethod findAll = GetProviderFindAll(className);
            CodeMemberMethod getPager = GetProviderGetPager(className);
            CodeMemberMethod add1 = GetProviderAdd1(className, type, primaryAutoIncKey);
            CodeMemberMethod add2 = GetProviderAdd2(entityName, tableName, columns, type);
            CodeMemberMethod update = GetProviderUpdate(className, type);
            CodeMemberMethod delete = GetProviderDelete(className, keyColumns, type);

            // Build Parameters
            CodeMemberMethod buildParameters1 = GetProviderBuildParameters1(entityName, columns, type);
            CodeMemberMethod buildParameters2 = GetProviderBuildParameters2(entityName, columns, type);
            CodeMemberMethod buildParametersForNonKey = GetProviderBuildParametersForNonKey(entityName, columns, type);

            // Fill Data
            CodeMemberMethod fill1 = GetProviderFill1(entityName, columns, type);
            CodeMemberMethod fill2 = GetProviderFill2(entityName, columns, type);

            CodeRegionDirective start = new CodeRegionDirective(CodeRegionMode.Start, "CRUD");
            CodeRegionDirective end = new CodeRegionDirective(CodeRegionMode.End, "CRUD");
            CodeRegionDirective start2 = new CodeRegionDirective(CodeRegionMode.Start, "Fill Data");
            CodeRegionDirective end2 = new CodeRegionDirective(CodeRegionMode.End, "Fill Data");

            exists.StartDirectives.Add(start);
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
            buildParametersForNonKey.EndDirectives.Add(end);

            fill1.StartDirectives.Add(start2);
            myClass.Members.Add(fill1);
            myClass.Members.Add(fill2);
            fill2.EndDirectives.Add(end2);
        }

        #endregion

    }
}

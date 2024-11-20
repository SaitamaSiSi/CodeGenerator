//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/18 16:29:11</date>
//------------------------------------------------------------------------------

using CodeGenerator.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator.Generator.Dm
{
    public class DmServiceHelper : CodeDomHelper
    {
        public const string SqlServiceBase = "SqlServiceBase";
        public const string SqlService = "SqlService";

        #region 私有Service基础方法

        private static CodeMemberMethod GetServiceExists(List<ColumnParam> keyColumns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Exists";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference("Boolean");

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin())"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return Provider.Exists(";

            //添加一个参数
            int index = 0;
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = GetCSharpType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                retStr += paramName;
                if (index < keyColumns.Count - 1)
                {
                    retStr += ", ";
                }
            }

            retStr += ");" + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetServiceGet(string entityName, List<ColumnParam> keyColumns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Get";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(entityName);

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin())"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return Provider.Get(";

            //添加一个参数
            int index = 0;
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = GetCSharpType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                retStr += paramName;
                if (index < keyColumns.Count - 1)
                {
                    retStr += ", ";
                }
            }

            retStr += ");" + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetServiceFindAll(string entityName)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "FindAll";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference($"List<{entityName}>");
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression("String", "whereClause"));

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin())"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return Provider.FindAll(whereClause);"
                + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetServiceGetPager(string entityName)
        {
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
            method.Parameters.Add(new CodeParameterDeclarationExpression("String", "whereClause"));

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin())"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return Provider.GetPager(pageIndex, pageSize, whereClause);"
                + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetServiceAdd1(string entityName)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Add";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference("Boolean");
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(entityName, "ent"));

            //设置返回值
            string retStr =
                Environment.NewLine + GetSpace(3) + "if (ent == null)"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "throw new ArgumentNullException(\"ent\");"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin(true))"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "var result = Provider.Add(ent) > 0;"
                + Environment.NewLine + GetSpace(4) + "scope.Commit();"
                + Environment.NewLine + GetSpace(4) + "return result;"
                + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetServiceAdd2(string entityName)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Add";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference("Boolean");
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression($"IEnumerable<{entityName}>", "list"));

            //设置返回值
            string retStr =
                Environment.NewLine + GetSpace(3) + "if (list == null)"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "throw new ArgumentNullException(\"list\");"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "var count = list.Count();"
                + Environment.NewLine + GetSpace(3) + "if (count == 0)"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "return false;"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin(true))"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "var result = Provider.Add(list) == count;"
                + Environment.NewLine + GetSpace(4) + "scope.Commit();"
                + Environment.NewLine + GetSpace(4) + "return result;"
                + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetServiceUpdate(string entityName)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Update";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference("Boolean");
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(entityName, "ent"));

            //设置返回值
            string retStr =
                Environment.NewLine + GetSpace(3) + "if (ent == null)"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "throw new ArgumentNullException(\"ent\");"
                + Environment.NewLine + GetSpace(3) + "}"
                + Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin(true))"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "var result = Provider.Update(ent) > 0;"
                + Environment.NewLine + GetSpace(4) + "scope.Commit();"
                + Environment.NewLine + GetSpace(4) + "return result;"
                + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetServiceDelete(List<ColumnParam> keyColumns)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Delete";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference("Boolean");

            //设置返回值
            string retStr = Environment.NewLine + GetSpace(3) + "using (var scope = DataContextScope.GetCurrent(ConnectionName).Begin(true))"
                + Environment.NewLine + GetSpace(3) + "{"
                + Environment.NewLine + GetSpace(4) + "var result = Provider.Delete(";

            //添加一个参数
            int index = 0;
            foreach (var keyColumn in keyColumns)
            {
                string paramName = ConvertToCamelCase(keyColumn.Name, true);
                string cSharpType = GetCSharpType(keyColumn.Type);
                method.Parameters.Add(new CodeParameterDeclarationExpression(cSharpType, paramName));

                string colName = ConvertToCamelCase(keyColumn.Name);
                retStr += paramName;
                if (index < keyColumns.Count - 1)
                {
                    retStr += ", ";
                }
            }

            retStr += ") > 0;"
                + Environment.NewLine + GetSpace(4) + "scope.Commit();"
                + Environment.NewLine + GetSpace(4) + "return result;"
                + Environment.NewLine + GetSpace(3) + "}";
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        #endregion

        #region 公有Service方法

        public static CodeTypeDeclaration CreateGenerateServiceClass(CodeCompileUnit unit, ClassParam param)
        {
            CodeNamespace myNamespace = new CodeNamespace(param.ServiceNameSpace + ".Base");

            //导入必要的命名空间引用
            myNamespace.Imports.Add(new CodeNamespaceImport("System"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Data"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Data.Common"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            myNamespace.Imports.Add(new CodeNamespaceImport(param.ClassNameSpace));
            myNamespace.Imports.Add(new CodeNamespaceImport(param.ProviderNameSpace));
            myNamespace.Imports.Add(new CodeNamespaceImport(param.ServiceNameSpace));
            foreach (var addNameSpace in param.AddNameSpace)
            {
                myNamespace.Imports.Add(new CodeNamespaceImport(addNameSpace));
            }

            //Code:代码体
            CodeTypeDeclaration myClass = new CodeTypeDeclaration(param.ClassName + SqlServiceBase);
            //指定为类
            myClass.IsClass = true;
            myClass.IsPartial = true;
            //设置类的访问类型
            myClass.TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract;
            //把这个类放在这个命名空间下
            myNamespace.Types.Add(myClass);
            //把该命名空间加入到编译器单元的命名空间集合中
            unit.Namespaces.Add(myNamespace);

            CodeTypeReference iTypeRef = new CodeTypeReference($"{SqlServiceBase}<{param.ClassName}{DmEntityHelper.Entity}>");
            myClass.BaseTypes.Add(iTypeRef);

            CodeMemberField field = new CodeMemberField("String", "_connectionName");
            //设置访问类型
            field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            myClass.Members.Add(field);

            CodeMemberProperty pField = new CodeMemberProperty();
            pField.Name = "ConnectionName";
            pField.Type = new CodeTypeReference("String");
            //设置访问类型
            pField.Attributes = MemberAttributes.Family;
            pField.HasGet = true;
            pField.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_connectionName")));
            myClass.Members.Add(pField);

            CodeMemberField providerField = new CodeMemberField($"{param.ClassName + DmProviderHelper.SqlProvier}", "Provider");
            //设置访问类型
            providerField.Attributes = MemberAttributes.Family | MemberAttributes.Final;
            myClass.Members.Add(providerField);

            //添加构造方法
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression("String", "connectionName"));
            constructor.Statements.Add(new CodeSnippetStatement(
                Environment.NewLine + GetSpace(3) + "_connectionName = connectionName;"
                 + Environment.NewLine + GetSpace(3) + $"Provider = new {param.ClassName + DmProviderHelper.SqlProvier}(connectionName);"
                ));
            myClass.Members.Add(constructor);

            return myClass;
        }

        public static CodeTypeDeclaration CreateNormalServiceClass(CodeCompileUnit unit, ClassParam param)
        {
            unit.Namespaces.Add(new CodeNamespace());
            //导入必要的命名空间引用
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Data"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Linq"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Text"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport(param.ServiceNameSpace + ".Base"));

            CodeNamespace myNamespace = new CodeNamespace(param.ServiceNameSpace);

            //Code:代码体
            CodeTypeDeclaration myClass = new CodeTypeDeclaration(param.ClassName + SqlService);
            //指定为类
            myClass.IsClass = true;
            myClass.IsPartial = true;
            //设置类的访问类型
            myClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            //把这个类放在这个命名空间下
            myNamespace.Types.Add(myClass);
            //把该命名空间加入到编译器单元的命名空间集合中
            unit.Namespaces.Add(myNamespace);

            CodeTypeReference iTypeRef = new CodeTypeReference(param.ClassName + SqlServiceBase);
            myClass.BaseTypes.Add(iTypeRef);

            CodeMemberField field = new CodeMemberField("String", "DefaultConnectionString");
            //设置访问类型
            field.Attributes = MemberAttributes.Const;
            field.InitExpression = new CodePrimitiveExpression("DefaultConnectionString");
            myClass.Members.Add(field);

            //添加构造方法
            CodeConstructor constructor1 = new CodeConstructor();
            constructor1.Attributes = MemberAttributes.Public;
            // 调用基类的无参数构造函数
            constructor1.ChainedConstructorArgs.Add(new CodeVariableReferenceExpression("DefaultConnectionString"));
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

        public static void CreateServiceMethod(CodeTypeDeclaration myClass, string className, List<ColumnParam> columns, List<string> keyColumnNames)
        {
            string entityName = className + DmEntityHelper.Entity;

            // CRUD
            List<ColumnParam> keyColumns = columns.Where(m => keyColumnNames.Contains(m.Name.ToUpper())).ToList();

            CodeMemberMethod exists = GetServiceExists(keyColumns);
            CodeMemberMethod get = GetServiceGet(entityName, keyColumns);
            CodeMemberMethod findAll = GetServiceFindAll(entityName);
            CodeMemberMethod getPager = GetServiceGetPager(entityName);
            CodeMemberMethod add1 = GetServiceAdd1(entityName);
            CodeMemberMethod add2 = GetServiceAdd2(entityName);
            CodeMemberMethod update = GetServiceUpdate(entityName);
            CodeMemberMethod delete = GetServiceDelete(keyColumns);

            myClass.Members.Add(exists);
            myClass.Members.Add(get);
            myClass.Members.Add(findAll);
            myClass.Members.Add(getPager);
            myClass.Members.Add(add1);
            myClass.Members.Add(add2);
            myClass.Members.Add(update);
            myClass.Members.Add(delete);

        }

        #endregion

    }
}

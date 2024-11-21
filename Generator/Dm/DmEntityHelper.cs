//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/15 14:39:58</date>
//------------------------------------------------------------------------------

using CodeGenerator.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;

namespace CodeGenerator.Generator.Dm
{
    public class DmEntityHelper : CodeDomHelper
    {
        public const string Entity = "Entity";

        #region 私有Entity基础方法

        private static CodeMemberMethod GetEntityCloneFromOrTo(List<string> columns, string className, bool isFrom)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = isFrom ? "CloneFrom" : "CloneTo";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //添加一个参数
            method.Parameters.Add(new CodeParameterDeclarationExpression(className, "thatObj"));
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(className);
            //设置返回值
            string retStr = GetSpace(3) + "if (thatObj == null)" + Environment.NewLine +
                GetSpace(3) + "{" + Environment.NewLine +
                GetSpace(4) + "throw new ArgumentNullException(\"thatObj\");" + Environment.NewLine +
                GetSpace(3) + "}";

            foreach (var col in columns)
            {
                string fieldName = ConvertToCamelCase(col);
                retStr = retStr + Environment.NewLine + GetSpace(3);
                retStr = retStr + (isFrom ? $"this.{fieldName} = thatObj.{fieldName};" : $"thatObj.{fieldName} = this.{fieldName};");
            }
            retStr = retStr + Environment.NewLine + GetSpace(3) + "return " + (isFrom ? "this;" : "thatObj;");
            method.Statements.Add(new CodeSnippetStatement(retStr));

            return method;
        }

        private static CodeMemberMethod GetEntityClone(string className)
        {
            //添加方法
            CodeMemberMethod method = new CodeMemberMethod();
            //方法名
            method.Name = "Clone";
            //访问类型
            method.Attributes = MemberAttributes.Public;
            //设置返回值类型：int/不设置则为void
            method.ReturnType = new CodeTypeReference(className);
            //设置返回值
            method.Statements.Add(new CodeSnippetStatement(
                GetSpace(3) + $"var thatObj = new {className}();" + Environment.NewLine +
                GetSpace(3) + "return this.CloneTo(thatObj);"
                ));
            return method;
        }

        private static CodeMemberMethod GetEntityIClone()
        {
            // 实现ICloneable接口的Clone方法
            CodeMemberMethod cloneMethod = new CodeMemberMethod
            {
                Name = "ICloneable.Clone",
                Attributes = MemberAttributes.Final, // Final表示覆盖接口中的抽象方法
                ReturnType = new CodeTypeReference(typeof(object))
            };

            cloneMethod.Statements.Add(
                new CodeSnippetStatement(
                GetSpace(3) + "return this.Clone();"
                ));

            return cloneMethod;
        }

        #endregion

        #region 公共Entity方法

        public static CodeTypeDeclaration CreateNormalEntityClass(CodeCompileUnit unit, ClassParam param)
        {
            unit.Namespaces.Add(new CodeNamespace());
            //导入必要的命名空间引用
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Data"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Linq"));
            unit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Text"));

            CodeNamespace myNamespace = new CodeNamespace(param.ClassNameSpace);

            //Code:代码体
            CodeTypeDeclaration myClass = new CodeTypeDeclaration(param.ClassName + Entity);
            //指定为类
            myClass.IsClass = true;
            myClass.IsPartial = true;
            //设置类的访问类型
            myClass.TypeAttributes = System.Reflection.TypeAttributes.Public;

            //把这个类放在这个命名空间下
            myNamespace.Types.Add(myClass);
            //把该命名空间加入到编译器单元的命名空间集合中
            unit.Namespaces.Add(myNamespace);

            return myClass;
        }

        public static CodeTypeDeclaration CreateGenerateEntityClass(CodeCompileUnit unit, ClassParam param)
        {
            CodeNamespace myNamespace = new CodeNamespace(param.ClassNameSpace);

            //导入必要的命名空间引用
            myNamespace.Imports.Add(new CodeNamespaceImport("System"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Text"));

            //Code:代码体
            CodeTypeDeclaration myClass = new CodeTypeDeclaration(param.ClassName + Entity);
            //指定为类
            myClass.IsClass = true;
            myClass.IsPartial = true;
            //设置类的访问类型
            myClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            // 添加XML文档注释
            myClass.Comments.Add(
                new CodeCommentStatement(
                    "<summary>" + Environment.NewLine +
                    " " + param.ClassCom + Environment.NewLine +
                    " </summary>",
                    docComment: true
                )
            );
            //把这个类放在这个命名空间下
            myNamespace.Types.Add(myClass);
            //把该命名空间加入到编译器单元的命名空间集合中
            unit.Namespaces.Add(myNamespace);

            // 添加IEntity接口
            CodeTypeReference iEntityTypeRef = new CodeTypeReference("IEntity");
            // 添加ICloneable接口
            CodeTypeReference iCloneableTypeRef = new CodeTypeReference(typeof(ICloneable).Name);
            myClass.BaseTypes.Add(iEntityTypeRef);
            myClass.BaseTypes.Add(iCloneableTypeRef);

            return myClass;
        }

        public static void CreateEntityBaseMethod(CodeTypeDeclaration myClass, List<string> columns)
        {
            string className = myClass.Name;
            CodeMemberMethod cloneFrom = GetEntityCloneFromOrTo(columns, className, true);
            CodeMemberMethod cloneTo = GetEntityCloneFromOrTo(columns, className, false);
            CodeMemberMethod clone = GetEntityClone(className);
            CodeMemberMethod iclone = GetEntityIClone();
            ///将方法添加到myClass类中
            myClass.Members.Add(cloneFrom);
            myClass.Members.Add(cloneTo);
            myClass.Members.Add(clone);
            myClass.Members.Add(iclone);

        }

        public static void CreateFields(CodeTypeDeclaration myClass, List<ColumnParam> columns)
        {
            foreach (var item in columns)
            {
                string fieldName = ConvertToCamelCase(item.Name);
                string cSharpType = DmToCSharpByType(item.Type) + (string.Equals(item.IsNullable, "Y") ? "?" : "");
                CodeMemberField field = new CodeMemberField(cSharpType, fieldName);
                //设置访问类型
                field.Attributes = MemberAttributes.Private;
                // 添加XML文档注释
                field.Comments.Add(
                    new CodeCommentStatement(
                        "<summary>" + Environment.NewLine +
                        " get or set " + item.Comments + Environment.NewLine +
                        " </summary>",
                        docComment: true
                    )
                );

                CodeMemberProperty pField = new CodeMemberProperty();
                pField.Name = item.Name;
                pField.Type = new CodeTypeReference(cSharpType);
                //设置访问类型
                pField.Attributes = MemberAttributes.Public;
                // 添加XML文档注释
                pField.Comments.Add(
                    new CodeCommentStatement(
                        "<summary>" + Environment.NewLine +
                        " get or set " + item.Comments + Environment.NewLine +
                        " </summary>",
                        docComment: true
                    )
                );
                pField.HasGet = true;
                pField.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
                pField.HasSet = true;
                pField.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodePropertySetValueReferenceExpression()));

                myClass.Members.Add(field);
                myClass.Members.Add(pField);
            }
        }

        #endregion
    }
}

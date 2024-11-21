//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/14 13:19:34</date>
//------------------------------------------------------------------------------

using CodeGenerator.Model;
using System.CodeDom;

namespace CodeGenerator.Generator.Dm
{
    public class DmGenerator
    {
        public static void CreateEntities(ClassParam param)
        {
            // 创建实体
            CreateEntityClass(param);

            // 创建数据
            CreateProvicerClass(param);

            // 创建服务
            CreateServiceClass(param);
        }

        private static void CreateEntityClass(ClassParam param)
        {
            //准备一个代码编译器单元
            CodeCompileUnit generateUnit = new CodeCompileUnit();
            //Code:代码体
            CodeTypeDeclaration generateClass = DmEntityHelper.CreateGenerateEntityClass(generateUnit, param);
            //添加字段
            DmEntityHelper.CreateFields(generateClass, param.Parameters);
            //添加方法
            DmEntityHelper.CreateEntityBaseMethod(generateClass, param.GetNames());
            //保存
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, DmEntityHelper.Entity);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            DmEntityHelper.CreateNormalEntityClass(normalUnit, param);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, DmEntityHelper.Entity, false);
        }

        private static void CreateProvicerClass(ClassParam param)
        {
            CodeCompileUnit generateUnit = new CodeCompileUnit();
            CodeTypeDeclaration generateClass = DmProviderHelper.CreateGenerateProviderClass(generateUnit, param);
            DmProviderHelper.CreateProviderMethod(generateClass, param.ClassName, param.TableName, param.Parameters, param.GetPrimaryKeys());
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, DmProviderHelper.SqlProvierBase);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            CodeTypeDeclaration normalClass = DmProviderHelper.CreateNormalProviderClass(normalUnit, param);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, DmProviderHelper.SqlProvier, false);
        }

        private static void CreateServiceClass(ClassParam param)
        {
            CodeCompileUnit generateUnit = new CodeCompileUnit();
            CodeTypeDeclaration generateClass = DmServiceHelper.CreateGenerateServiceClass(generateUnit, param);
            DmServiceHelper.CreateServiceMethod(generateClass, param.ClassName, param.Parameters, param.GetPrimaryKeys());
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, DmServiceHelper.SqlServiceBase);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            CodeTypeDeclaration normalClass = DmServiceHelper.CreateNormalServiceClass(normalUnit, param);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, DmServiceHelper.SqlService, false);
        }
    }
}

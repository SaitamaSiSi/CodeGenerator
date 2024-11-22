//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/14 13:19:34</date>
//------------------------------------------------------------------------------

using CodeGenerator.Model;
using System.CodeDom;
using Zyh.Common.Entity;

namespace CodeGenerator.Generator
{
    public class ClassGenerator
    {
        #region 内部私有方法

        /// <summary>
        /// 实体层生成方法
        /// </summary>
        /// <param name="param"></param>
        /// <param name="config"></param>
        private static void CreateEntityClass(ClassParam param, GenerateConfig config)
        {
            string prePath = CodeDomHelper.GetDirName(config.DbType);
            string lastPath = $"{config.ClassNameSpace}/Generated";

            //准备一个代码编译器单元
            CodeCompileUnit generateUnit = new CodeCompileUnit();
            //Code:代码体
            CodeTypeDeclaration generateClass = EntityHelper.CreateGenerateEntityClass(generateUnit, param, config.ClassNameSpace);
            //添加字段
            EntityHelper.CreateFields(generateClass, param.Parameters, config.DbType);
            //添加方法
            EntityHelper.CreateEntityBaseMethod(generateClass, param.GetNames());
            //保存
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, EntityHelper.Entity, prePath, lastPath);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            EntityHelper.CreateNormalEntityClass(normalUnit, param, config.ClassNameSpace);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, EntityHelper.Entity, prePath, string.Empty, false);
        }

        /// <summary>
        /// 数据链路层生成方法
        /// </summary>
        /// <param name="param"></param>
        /// <param name="config"></param>
        private static void CreateProvicerClass(ClassParam param, GenerateConfig config)
        {
            string prePath = CodeDomHelper.GetDirName(config.DbType);
            string lastPath = $"{config.ProviderNameSpace}/Generated";

            CodeCompileUnit generateUnit = new CodeCompileUnit();
            CodeTypeDeclaration generateClass = ProviderHelper.CreateGenerateProviderClass(generateUnit, param, config.DbType, config.ClassNameSpace, config.ProviderNameSpace);
            ProviderHelper.CreateProviderMethod(generateClass, param.ClassName, param.TableName, param.Parameters, param.GetPrimaryKeys(), config.DbType);
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, ProviderHelper.SqlProvierBase, prePath, lastPath);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            ProviderHelper.CreateNormalProviderClass(normalUnit, param, config.ClassNameSpace, config.ProviderNameSpace);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, ProviderHelper.SqlProvier, prePath, string.Empty, false);
        }

        /// <summary>
        /// 业务逻辑层生成方法
        /// </summary>
        /// <param name="param"></param>
        /// <param name="config"></param>
        private static void CreateServiceClass(ClassParam param, GenerateConfig config)
        {
            string prePath = CodeDomHelper.GetDirName(config.DbType);
            string lastPath = $"{config.ServiceNameSpace}/Generated";

            CodeCompileUnit generateUnit = new CodeCompileUnit();
            CodeTypeDeclaration generateClass = ServiceHelper.CreateGenerateServiceClass(generateUnit, param, config.ClassNameSpace, config.ProviderNameSpace, config.ServiceNameSpace);
            ServiceHelper.CreateServiceMethod(generateClass, param.ClassName, param.Parameters, param.GetPrimaryKeys(), config.DbType);
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, ServiceHelper.SqlServiceBase, prePath, lastPath);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            ServiceHelper.CreateNormalServiceClass(normalUnit, param, config.ServiceNameSpace);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, ServiceHelper.SqlService, prePath, string.Empty, false);
        }

        #endregion

        #region 公共创建方法

        /// <summary>
        /// 创建单表的对应数据库操作类文件
        /// </summary>
        /// <param name="param"></param>
        /// <param name="config"></param>
        public static void CreateClasses(ClassParam param, GenerateConfig config)
        {
            // 创建实体
            CreateEntityClass(param, config);

            // 创建数据
            CreateProvicerClass(param, config);

            // 创建服务
            CreateServiceClass(param, config);
        }

        #endregion

    }
}

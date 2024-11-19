//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/14 13:19:34</date>
//------------------------------------------------------------------------------

using CodeGenerator.Core;
using CodeGenerator.Model;
using Dm;
using System;
using System.CodeDom;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace CodeGenerator.Generator
{
    public class DmGenerator : IDisposable
    {
        private string ConnectString { get; set; }

        public DmGenerator(string connectStr)
        {
            ConnectString = connectStr;
        }

        public void CreateEntities()
        {
            ClassParam param = new ClassParam();
            param.TableName = "T_LED_EQUIP";
            param.ClassName = "LedEquip";

            using (DmConnection connection = new DmConnection(ConnectString))
            {
                connection.Open();

                string sql = $"SELECT COMMENTS FROM ALL_TAB_COMMENTS WHERE TABLE_NAME = '{param.TableName}'";
                using (DmCommand cmd = new DmCommand(sql, connection))
                {
                    var dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        param.ClassCom = dr.GetString(0);
                    }
                }

                sql = @$"
SELECT DISTINCT
    t.COLUMN_NAME 列名,
    t.DATA_TYPE   字段类型,
    c.COMMENTS    描述,
    t.DATA_LENGTH 类型长度,
    t.DATA_PRECISION 整数位,
    t.DATA_SCALE   小数位,
    t.NULLABLE    是否非空
FROM 
    ALL_TAB_COLUMNS t
INNER JOIN 
    ALL_COL_COMMENTS c ON t.TABLE_NAME = c.TABLE_NAME AND t.COLUMN_NAME = c.COLUMN_NAME
WHERE 
    t.TABLE_NAME = '{param.TableName}' 
GROUP BY 
    t.COLUMN_ID;";
                using (DmCommand cmd = new DmCommand(sql, connection))
                {
                    DmDataAdapter da = new DmDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    DataTable dt = ds.Tables[0];

                    foreach (DataRow row in dt.Rows)
                    {
                        ColumnParam column = new ColumnParam
                        {
                            Name = row["列名"].ToString().ToUpper(),
                            Type = row["字段类型"].ToString(),
                            Comments = row["描述"].ToString().ToUpper(),
                            IsNullable = row["是否非空"].ToString().ToUpper()
                        };
                        param.Parameters.Add(column);
                    }
                }

                sql = @$"
SELECT 
    WM_CONCAT(B.COLUMN_NAME) AS PK_COLUMNS
FROM 
    ALL_CONSTRAINTS A,
    ALL_CONS_COLUMNS B
WHERE 
    A.CONSTRAINT_TYPE = 'P'
    AND A.TABLE_NAME = '{param.TableName}'
    AND B.OWNER = A.OWNER
    AND A.TABLE_NAME = B.TABLE_NAME
GROUP BY 
    A.OWNER,
    A.TABLE_NAME;";
                using (DmCommand cmd = new DmCommand(sql, connection))
                {
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // 获取主键列名
                            string primaryKeys = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            param.TableKey = primaryKeys.Split(',').Distinct().ToList();
                        }
                    }
                }

                connection.Close();
            }

            // 创建实体
            CreateEntityClass(param);

            // 创建数据
            CreateProvicerClass(param);

            // 创建服务
            CreateServiceClass(param);
        }

        private void CreateEntityClass(ClassParam param)
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

        private void CreateProvicerClass(ClassParam param)
        {
            CodeCompileUnit generateUnit = new CodeCompileUnit();
            CodeTypeDeclaration generateClass = DmProviderHelper.CreateGenerateProviderClass(generateUnit, param);
            DmProviderHelper.CreateProviderMethod(generateClass, param.ClassName, param.TableName, param.Parameters, param.TableKey);
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, DmProviderHelper.SqlProvierBase);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            CodeTypeDeclaration normalClass = DmProviderHelper.CreateNormalProviderClass(normalUnit, param);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, DmProviderHelper.SqlProvier, false);
        }

        private void CreateServiceClass(ClassParam param)
        {
            CodeCompileUnit generateUnit = new CodeCompileUnit();
            CodeTypeDeclaration generateClass = DmServiceHelper.CreateGenerateServiceClass(generateUnit, param);
            DmServiceHelper.CreateServiceMethod(generateClass, param.ClassName, param.Parameters, param.TableKey);
            CodeDomHelper.SaveClass(generateUnit, param.ClassName, DmServiceHelper.SqlServiceBase);

            CodeCompileUnit normalUnit = new CodeCompileUnit();
            CodeTypeDeclaration normalClass = DmServiceHelper.CreateNormalServiceClass(normalUnit, param);
            CodeDomHelper.SaveClass(normalUnit, param.ClassName, DmServiceHelper.SqlService, false);
        }

        public void Dispose()
        {
        }
    }
}

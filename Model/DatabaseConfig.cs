//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/19 16:39:23</date>
//------------------------------------------------------------------------------

using Zyh.Common.Entity;

namespace CodeGenerator.Model
{
    public class DatabaseConfig
    {
        public DatabaseType DbType { get; set; }
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string GetConnectStr()
        {
            if (DbType == DatabaseType.Dm)
            {
                return $"Server={IP};PORT={Port};USER ID={UserName};PWD={Password}";
            }
            else if (DbType == DatabaseType.Mysql)
            {
                return $"Server={IP};port={Port};user={UserName};password={Password};";
            }
            else
            {
                return "";
            }
        }

        public string GetSelSchemeSql()
        {
            if (DbType == DatabaseType.Dm)
            {
                return "SELECT DISTINCT OBJECT_NAME FROM ALL_OBJECTS WHERE OBJECT_TYPE = 'SCH';";
            }
            else if (DbType == DatabaseType.Mysql)
            {
                return "SHOW DATABASES;";
                // return "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA;";
            }
            else
            {
                return "";
            }
        }

        public string GetSelTableSql(string schemaName)
        {
            if (DbType == DatabaseType.Dm)
            {
                return @$"
SELECT 
A.TABLE_NAME,
B.COMMENTS 
FROM DBA_TABLES A 
LEFT JOIN ALL_TAB_COMMENTS B 
ON A.TABLE_NAME = B.TABLE_NAME
WHERE A.OWNER = '{schemaName}'
GROUP BY A.TABLE_NAME;
";
            }
            else if (DbType == DatabaseType.Mysql)
            {
                return @$"
SELECT 
TABLE_NAME, 
TABLE_COMMENT AS COMMENTS
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = '{schemaName}';
";
            }
            else
            {
                return "";
            }
        }

        public string GetSelColumnSql(string schemaName, string tableName)
        {
            if (DbType == DatabaseType.Dm)
            {
                return @$"
SELECT DISTINCT
    t.COLUMN_NAME AS Name,
    t.DATA_TYPE AS Type,
    c.COMMENTS AS Comments,
    t.DATA_LENGTH AS Length,
    t.NULLABLE AS IsNullable,
    CASE WHEN d.NAME = t.COLUMN_NAME THEN TRUE ELSE FALSE END AS IsAutoIncrement
FROM 
    ALL_TAB_COLUMNS t
INNER JOIN 
    ALL_COL_COMMENTS c
     ON t.TABLE_NAME = c.TABLE_NAME AND t.COLUMN_NAME = c.COLUMN_NAME
LEFT JOIN
(
    select b.table_name,a.name from  SYS.SYSCOLUMNS a,all_tables b,sys.sysobjects c where a.INFO2 & 0x01 = 0x01 
    and a.id=c.id and c.name= b.table_name and c.name = '{tableName}' and b.OWNER = '{schemaName}'
) d on t.TABLE_NAME = d.TABLE_NAME
WHERE 
    t.OWNER = '{schemaName}' AND t.TABLE_NAME = '{tableName}'
GROUP BY 
    t.COLUMN_ID;
    
";
            }
            else if (DbType == DatabaseType.Mysql)
            {
                return @$"
SELECT DISTINCT
COLUMN_NAME AS Name,
DATA_TYPE AS Type,
COLUMN_COMMENT AS Comments,
CHARACTER_MAXIMUM_LENGTH AS Length,
IS_NULLABLE AS IsNullable
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = '{schemaName}' AND TABLE_NAME = '{tableName}';
";
            }
            else
            {
                return "";
            }
        }

        public string GetSelPrimaryKeySql(string schemaName, string tableName)
        {
            if (DbType == DatabaseType.Dm)
            {
                return @$"
SELECT WM_CONCAT(COLUMN_NAME) AS PK_COLUMNS
FROM 
(SELECT 
B.COLUMN_NAME
FROM 
    ALL_CONSTRAINTS A,
    ALL_CONS_COLUMNS B
WHERE 
    A.CONSTRAINT_TYPE = 'P'
    AND A.TABLE_NAME = '{tableName}'
    AND A.OWNER = '{schemaName}'
    AND B.OWNER = A.OWNER
    AND A.TABLE_NAME = B.TABLE_NAME
GROUP BY 
    A.TABLE_NAME
)
";
            }
            else if (DbType == DatabaseType.Mysql)
            {
                return @$"
SELECT GROUP_CONCAT(COLUMN_NAME ORDER BY ORDINAL_POSITION) AS PK_COLUMNS
FROM INFORMATION_SCHEMA.COLUMNS
WHERE 
TABLE_SCHEMA = '{schemaName}' AND TABLE_NAME = '{tableName}' AND COLUMN_KEY = 'PRI';
";
            }
            else
            {
                return "";
            }
        }
    }
}

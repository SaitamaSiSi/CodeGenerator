//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/19 16:39:23</date>
//------------------------------------------------------------------------------

using Zyh.Common.Entity;

namespace CodeGenerator.Model
{
    public class DatabaseConfig : GenerateConfig
    {
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 该参数目前仅针对Opengauss
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public string GetConnectStr(string database = "")
        {
            string databaseStr = string.Empty;
            if (DbType == DatabaseType.Dm)
            {
                databaseStr = string.IsNullOrEmpty(database) ? "" : $"Database={database.ToLower()};SCHEMA={database.ToLower()};";
                return $"Server={IP};PORT={Port};{databaseStr}USER ID={UserName};PWD={Password};"; // SCHEMA=TEST_DB;
            }
            else if (DbType == DatabaseType.Mysql)
            {
                databaseStr = string.IsNullOrEmpty(database) ? "" : $"Database={database.ToLower()};";
                return $"Server={IP};port={Port};{databaseStr}user={UserName};password={Password};"; // database=test_db;
            }
            else if (DbType == DatabaseType.OpenGauss)
            {
                database = string.IsNullOrEmpty(database) ? "postgres" : database;
                databaseStr = $"Database={database.ToLower()};";
                // PostgreSQL类型数据库必须指定已存在数据库连接，此处连接默认数据库
                return $"Host={IP};Port={Port};{databaseStr}Username ={UserName};PASSWORD={Password};No Reset On Close=true"; // database=test_db;
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
            else if (DbType == DatabaseType.OpenGauss)
            {
                return "SELECT datname AS database_name \r\nFROM pg_database \r\nWHERE datname NOT LIKE 'template%' \r\n  AND datname != 'postgres';";
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
            else if (DbType == DatabaseType.OpenGauss)
            {
                return $@"
SELECT 
    tablename AS TABLE_NAME, 
    COALESCE(description, '') AS COMMENTS 
FROM pg_tables 
LEFT JOIN pg_description 
ON pg_tables.tablename::regclass = pg_description.objoid 
WHERE schemaname = 'public';
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
IS_NULLABLE AS IsNullable,
CASE WHEN EXTRA = 'auto_increment' THEN TRUE ELSE FALSE END AS IsAutoIncrement
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = '{schemaName}' AND TABLE_NAME = '{tableName}';
";
            }
            else if (DbType == DatabaseType.OpenGauss)
            {
                return @$"
SELECT
        a.attname AS Name,
        format_type(a.atttypid, a.atttypmod) AS Type,
        COALESCE(d.description, '') AS Comments,
        CASE 
            WHEN format_type(a.atttypid, a.atttypmod) LIKE '%(%)%' THEN
                split_part(split_part(format_type(a.atttypid, a.atttypmod), '(', 2), ')', 1)::INT
            ELSE NULL 
        END AS Length,
        CASE 
            WHEN a.attnotnull THEN 'NO' 
            ELSE 'YES' 
        END AS IsNullable,
        CASE 
            WHEN pg_get_expr(ad.adbin, ad.adrelid) LIKE 'nextval%' THEN TRUE 
            ELSE FALSE 
        END AS IsAutoIncrement
      FROM
        pg_attribute a
      LEFT JOIN
        pg_description d 
        ON a.attrelid = d.objoid AND a.attnum = d.objsubid
      LEFT JOIN
        pg_attrdef ad 
        ON a.attrelid = ad.adrelid AND a.attnum = ad.adnum
      INNER JOIN
        pg_class c 
        ON a.attrelid = c.oid
      INNER JOIN
        pg_namespace n 
        ON c.relnamespace = n.oid
      WHERE
        n.nspname = 'public'
        AND c.relname = '{tableName.ToLower()}'
        AND a.attnum > 0
        AND NOT a.attisdropped;
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
            else if (DbType == DatabaseType.OpenGauss)
            {
                return @$"
SELECT string_agg(a.attname, ', ' ORDER BY a.attnum) AS PK_COLUMNS
FROM 
    pg_constraint c
    JOIN pg_namespace n ON n.oid = c.connamespace
    JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = ANY(c.conkey)
WHERE 
    c.contype = 'p' AND 
    n.nspname = 'public' AND 
    c.conrelid::regclass = '{tableName.ToLower()}'::regclass;
";
            }
            else
            {
                return "";
            }
        }
    }
}

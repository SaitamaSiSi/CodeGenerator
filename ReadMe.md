# 使用说明

生成数据库Entity、Provider、Service文件，仅封装基本CRUD数据库方法。
实体配置项为实体层命名空间、操作配置项为数据链路层配置项、服务配置项为业务逻辑层命名空间。
上述三个命名空间修改后，需同步修改Base中对应文件的命名空间。
移除前缀集为移除数据库表名前缀，通过逗号分割。
比如 T_TEST_STUDENT 的数据库表，通过在集合中配置 T_TEST_ 即可移除前缀生成 StudentEntity.cs文件。

## 依赖文件

Base文件夹中的文件需要添加到项目文件中引用使用，通过Dapper进行数据库通信，也可以自己改写具体内容。

## 数据库

### 1、达梦

需要依赖DM.DmProvider包进行数据库通信。

支持字段类型：

String：CHAR、VARCHAR、VARCHAR2、CLOB、TEXT。

Int16：SMALLINT。

Decimal：DECIMAL。

Int32：NUMBER、INT、INTEGER。

Int64：BIGINT。

Float：FLOAT。

Double：DOUBLE。

DateTime：DATE、DATETIME、TIMESTAMP。

Boolean：BOOLEAN、TINYINT、BIT。

Byte[]：BLOB。

注册以下数据库信息：

DbProviderFactories.RegisterFactory("DmClientFactory", Dm.DmClientFactory.Instance);

Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "DmClientFactory");

Environment.SetEnvironmentVariable("DefaultConnectionString", connStr);

### 2、Mysql

需要依赖MySqlConnector包进行数据库通信。

支持字段类型：

String：CHAR、VARCHAR、VARCHAR2、CLOB、TEXT、JSON。

Int16：SMALLINT。

Decimal：DECIMAL。

Int32：NUMBER、INT、INTEGER。

Int64：BIGINT。

Float：FLOAT。

Double：DOUBLE。

DateTime：DATE、DATETIME、TIMESTAMP。

Boolean：BOOLEAN、TINYINT、BIT。

Byte[]：BLOB。

注册以下数据库信息：

DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnector.MySqlConnectorFactory.Instance);

Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "MySqlConnector");

Environment.SetEnvironmentVariable("DefaultConnectionString", connStr);


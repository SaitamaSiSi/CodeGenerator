# 使用说明

生成数据库Entity、Provider、Service文件，仅封装基本CRUD数据库方法。

## 依赖文件

Base文件夹中的文件需要添加到项目文件中引用使用，通过Dapper进行数据库通信，也可以自己改写具体内容。

## 数据库

### 1、达梦

需要依赖DM.DmProvider包进行数据库通信。
支持字段类型：
String：CHAR、VARCHAR、VARCHAR2、CLOB、TEXT、
Int32：NUMBER、DECIMAL、INT、INTEGER、
Double：FLOAT、DOUBLE PRECISION、
DateTime：DATE、TIMESTAMP、
Boolean：BOOLEAN、TINYINT

注册以下数据库信息：
DbProviderFactories.RegisterFactory("DmClientFactory", Dm.DmClientFactory.Instance);
Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "DmClientFactory");
            Environment.SetEnvironmentVariable("DefaultConnectionString", connStr);

### 2、Mysql（仅展示、未实现生成）

需要依赖MySqlConnector包进行数据库通信。

注册以下数据库信息：
DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnector.MySqlConnectorFactory.Instance);
Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "MySqlConnector");
            Environment.SetEnvironmentVariable("DefaultConnectionString", connStr);
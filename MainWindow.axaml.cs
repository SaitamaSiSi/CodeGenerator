using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CodeGenerator.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Zyh.Common.Data;
using System.Linq;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeGenerator.Generator;
using CodeGenerator.Core;
using CodeGenerator.Command;
using Yitter.IdGenerator;
using System.Threading;
using System.Drawing;
using Avalonia.Controls.Generators;

namespace CodeGenerator
{
    public partial class MainWindow : Window
    {
        readonly DatabaseConfig config = new();

        #region 构造和初始化

        public MainWindow()
        {
            InitializeComponent();

            InitComponents();
            InitData();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitComponents()
        {
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;

            db_type.SelectionChanged += OnDbTypeChanged;
            db_port.TextChanging += PortChanging;
            tv.SelectionChanged += OnSelectionChanged;

            DbProviderFactories.RegisterFactory("DmClientFactory", Dm.DmClientFactory.Instance);
            DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnector.MySqlConnectorFactory.Instance);

            // 创建 IdGeneratorOptions 对象，可在构造函数中输入 WorkerId：
            var options = new IdGeneratorOptions();
            // options.WorkerIdBitLength = 10; // 默认值6，限定 WorkerId 最大值为2^6-1，即默认最多支持64个节点。
            // options.SeqBitLength = 6; // 默认值6，限制每毫秒生成的ID个数。若生成速度超过5万个/秒，建议加大 SeqBitLength 到 10。
            // options.BaseTime = Your_Base_Time; // 如果要兼容老系统的雪花算法，此处应设置为老系统的BaseTime。
            // ...... 其它参数参考 IdGeneratorOptions 定义。

            // 保存参数（务必调用，否则参数设置不生效）：
            YitIdHelper.SetIdGenerator(options);
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            // 初始化数据库连接信息
            db_type.SelectedIndex = 0;
            db_ip.Text = "192.168.100.198";
            db_port.Text = "5236";
            db_id.Text = "SYSDBA";
            db_pwd.Text = "654#@!qaz";
        }

        #endregion

        #region 数据库辅助方法

        /// <summary>
        /// 根据控件设置数据库信息
        /// </summary>
        /// <returns></returns>
        private bool SetDbConn()
        {
            if (!RegexHelper.IsValidIPv4(db_ip.Text ?? string.Empty))
            {
                ChangeShowMsg("IP地址不符合规范");
                return false;
            }
            config.IP = db_ip.Text ?? string.Empty;
            config.Port = Convert.ToInt32(db_port.Text);
            config.UserName = db_id.Text ?? string.Empty;
            config.Password = db_pwd.Text ?? string.Empty;
            CodeDomHelper.DbType = config.DbType;
            tv.ItemsSource = new List<string>();
            dg.ItemsSource = new List<string>();
            if (config.DbType == DatabaseType.Dm)
            {
                Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "DmClientFactory");
                Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr());
            }
            else if (config.DbType == DatabaseType.Mysql)
            {
                Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "MySqlConnector");
                Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr());
            }

            return true;
        }

        /// <summary>
        /// 查询某模式下的所有表信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetTableNames(string schemaName)
        {
            using (var scope = DataContextScope.GetCurrent().Begin())
            {
                string sql = config.GetSelTableSql(schemaName);
                DataTable dt = scope.DataContext.QueryDataTable(sql);

                Dictionary<string, string> result = new Dictionary<string, string>();
                foreach (DataRow row in dt.Rows)
                {
                    result.Add(
                        row["TABLE_NAME"].ToString().ToUpper(),
                        row["COMMENTS"].ToString()
                        );
                }
                return result;
            }
        }

        /// <summary>
        /// 查询某表下的所有字段信息
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private List<ColumnParam> GetTableColumns(string schemaName, string tableName)
        {
            using (var scope = DataContextScope.GetCurrent().Begin())
            {
                string sql = config.GetSelColumnSql(schemaName, tableName);
                List<ColumnParam> result = scope.DataContext.Query<ColumnParam>(sql);

                sql = config.GetSelPrimaryKeySql(schemaName, tableName);
                string primaryKeyStr = scope.DataContext.ExecuteScalar<string>(sql);
                List<string> primaryKeys = primaryKeyStr.Split(',').Distinct().ToList();
                foreach (var node in result)
                {
                    if (primaryKeys.Contains(node.Name))
                    {
                        node.IsPrimaryKey = true;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 查询数据库模式名
        /// </summary>
        /// <returns></returns>
        private List<TreeNode> GetSchameNames()
        {
            var treeNodes = new List<TreeNode>();
            using (var scope = DataContextScope.GetCurrent().Begin())
            {
                string sql = config.GetSelSchemeSql();
                List<string> schemaNames = scope.DataContext.Query<string>(sql);
                foreach (string schemaName in schemaNames)
                {
                    treeNodes.Add(new TreeNode()
                    {
                        Title = schemaName
                    });
                }
            }
            return treeNodes;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 修改页面提示信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="func"></param>
        private void ChangeShowMsg(string msg, Action? func = null)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                show_msg.Text = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} | {msg}";
                ToolTip.SetTip(show_msg, show_msg.Text);
                func?.Invoke();
            });
        }

        #endregion

        #region 控件触发事件

        /// <summary>
        /// 数据库类型改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDbTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                switch (db_type.SelectedIndex)
                {
                    case 0:
                        config.DbType = DatabaseType.Dm;
                        break;
                    case 1:
                        config.DbType = DatabaseType.Mysql;
                        break;
                }
            }
        }

        /// <summary>
        /// 端口改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortChanging(object? sender, TextChangingEventArgs e)
        {
            // 使用正则表达式确保只输入数字
            string txt = db_port.Text ?? string.Empty;
            if (!RegexHelper.IsValidNumber(txt))
            {
                db_port.Text = Regex.Replace(txt, "[^0-9]", "");
            }

            txt = db_port.Text ?? string.Empty;
            if (string.IsNullOrEmpty(txt))
            {
                db_port.Text = "0";
            }
            if (!string.IsNullOrEmpty(txt) && int.Parse(txt) > 65535)
            {
                db_port.Text = "65535";
            }
        }

        /// <summary>
        /// 树状表格选中改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ent = tv.SelectedItem as TreeNode;
            if (ent != null)
            {
                if (ent.Level == 0)
                {
                    // 模式
                    List<TreeNode> tableNodes = new List<TreeNode>();
                    Dictionary<string, string> tableInfos = GetTableNames(ent.Title);
                    foreach (var info in tableInfos)
                    {
                        tableNodes.Add(new TreeNode()
                        {
                            Level = 1,
                            Title = info.Key,
                            Comments = info.Value
                        });
                    }
                    ent.SubNodes.Clear();
                    ent.SubNodes = tableNodes;
                    //tv.ItemContainerGenerator.ItemContainerPrepared(tv, ent);
                    if (tableNodes.Count > 0 && tv.SelectedItem is TreeViewItem selItem)
                    {
                        selItem.IsExpanded = true;
                    }
                }
                else if (ent.Level == 1)
                {
                    // 表
                    string parentTitle = tv.ItemsSource?.Cast<TreeNode>().Where(m => m.SubNodes.Contains(ent)).Select(m => m.Title).FirstOrDefault() ?? string.Empty;
                    dg.ItemsSource = ent.Columns = GetTableColumns(parentTitle, ent.Title);
                }
            }
        }

        /// <summary>
        /// 连接按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnectClick(object sender, RoutedEventArgs e)
        {
            if (!SetDbConn())
            {
                return;
            }

            try
            {
                DataContextScope.ClearCurrent();
                tv.ItemsSource = GetSchameNames();
                ChangeShowMsg("连接成功");
            }
            catch (Exception ex)
            {
                ChangeShowMsg($"发生异常:{ex.Message}");
            }
        }

        /// <summary>
        /// 开始按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartClick(object sender, RoutedEventArgs e)
        {
            var treeNodes = tv.ItemsSource?.Cast<TreeNode>().ToList();
            if (treeNodes != null)
            {
                List<ClassParam> list = new List<ClassParam>();
                foreach (var schame in treeNodes)
                {
                    foreach (var table in schame.SubNodes)
                    {
                        if (table.IsChecked)
                        {
                            ClassParam param = new ClassParam();
                            param.TableName = table.Title;
                            param.ClassName = CodeDomHelper.ConvertToCamelCase(param.TableName);
                            param.ClassCom = table.Comments;
                            if (table.Columns.Count == 0)
                            {
                                table.Columns = GetTableColumns(schame.Title, table.Title);
                            }
                            param.Parameters = table.Columns;
                            param.TableKey = table.Columns.Where(m => m.IsPrimaryKey).Select(m => m.Name).ToList();
                            table.IsChecked = false;
                            list.Add(param);
                        }
                    }
                }

                if (list.Count == 0)
                {
                    ChangeShowMsg("没有选中表");
                    return;
                }
                ChangeShowMsg("生成中");
                btnStart.IsEnabled = false;
                _ = Task.Factory.StartNew((obj) =>
                {
                    Tuple<DatabaseType, List<ClassParam>>? tuple = (Tuple<DatabaseType, List<ClassParam>>?)obj;
                    void action() { btnStart.IsEnabled = true; }
                    if (tuple != null)
                    {
                        DatabaseType currentType = tuple.Item1;
                        List<ClassParam> classes = tuple.Item2;
                        if (currentType == DatabaseType.Dm)
                        {
                            var res = CommandBus.Execute<DmCmd, CmdResCode>(new DmCmd() { Command = DatabaseType.Dm, Classes = classes });
                            ChangeShowMsg(res.Msg, action);
                        }
                        else if (currentType == DatabaseType.Mysql)
                        {
                            var res = CommandBus.Execute<MysqlCmd, CmdResCode>(new MysqlCmd() { Command = DatabaseType.Mysql, Classes = classes });
                            ChangeShowMsg(res.Msg, action);
                        }
                        else
                        {
                            ChangeShowMsg("未知数据库类型", action);
                        }
                    }
                    else
                    {
                        ChangeShowMsg("生成失败，实体对象不可为空", action);
                    }
                }, Tuple.Create(config.DbType, list));
            }
            else
            {
                ChangeShowMsg("没有模式");
            }
        }

        /// <summary>
        /// 程序关闭中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            // 不退出
            // e.Cancel = true;
        }

        /// <summary>
        /// 程序关闭后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            DataContextScope.ClearCurrent();
        }

        #endregion

    }
}
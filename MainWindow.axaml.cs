using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CodeGenerator.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Zyh.Common.Entity;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using CodeGenerator.Generator;
using CodeGenerator.Core;
using CodeGenerator.Command;
using Yitter.IdGenerator;

namespace CodeGenerator
{
    public partial class MainWindow : Window
    {
        private readonly bool IsProduction = true;
        DatabaseConfig config = new();

        #region ����ͳ�ʼ��

        public MainWindow()
        {
            InitializeComponent();

            InitComponents();
            InitData();
        }

        /// <summary>
        /// ��ʼ�����
        /// </summary>
        private void InitComponents()
        {
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;


            btnTest.IsVisible = !IsProduction;
            tv.SelectionChanged += OnSelectionChanged;

            DbProviderFactories.RegisterFactory("DmClientFactory", Dm.DmClientFactory.Instance);
            DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnector.MySqlConnectorFactory.Instance);
            DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);

            // ���� IdGeneratorOptions ���󣬿��ڹ��캯�������� WorkerId��
            var options = new IdGeneratorOptions();
            // options.WorkerIdBitLength = 10; // Ĭ��ֵ6���޶� WorkerId ���ֵΪ2^6-1����Ĭ�����֧��64���ڵ㡣
            // options.SeqBitLength = 6; // Ĭ��ֵ6������ÿ�������ɵ�ID�������������ٶȳ���5���/�룬����Ӵ� SeqBitLength �� 10��
            // options.BaseTime = Your_Base_Time; // ���Ҫ������ϵͳ��ѩ���㷨���˴�Ӧ����Ϊ��ϵͳ��BaseTime��
            // ...... ���������ο� IdGeneratorOptions ���塣

            // �����������ص��ã�����������ò���Ч����
            YitIdHelper.SetIdGenerator(options);
        }

        /// <summary>
        /// ��ʼ������
        /// </summary>
        private void InitData()
        {
            // ��ʼ�����ݿ�������Ϣ

            /* ����
            config.DbType = DatabaseType.Dm;
            config.IP = "192.168.100.198";
            config.Port = 5236;
            config.UserName = "SYSDBA";
            config.Password = "654#@!qaz";
            Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "DmClientFactory");
            Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr());
            */

            config.DbType = DatabaseType.OpenGauss;
            config.IP = "192.168.100.168";
            config.Port = 5432;
            config.UserName = "test";
            config.Password = "test@123";
            Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "Npgsql");
            Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr());
        }

        #endregion

        #region ���ݿ⸨������

        /// <summary>
        /// ��ѯĳģʽ�µ����б���Ϣ
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetTableNames(string schemaName)
        {
            if (config.DbType == DatabaseType.OpenGauss)
            {
                DataContextScope.ClearCurrent();
                Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr(schemaName));
            }
            using (var scope = DataContextScope.GetCurrent().Begin())
            {
                string sql = config.GetSelTableSql(schemaName);
                DataTable dt = scope.DataContext.QueryDataTable(sql);

                Dictionary<string, string> result = new Dictionary<string, string>();
                foreach (DataRow row in dt.Rows)
                {
                    string tableName = (row["TABLE_NAME"].ToString() ?? string.Empty).ToUpper();
                    if (!result.ContainsKey(tableName))
                    {
                        result.Add(tableName, row["COMMENTS"].ToString() ?? string.Empty);
                    }
                }
                return new Dictionary<string, string>(result.Where(m => !string.IsNullOrEmpty(m.Key)));
            }
        }

        /// <summary>
        /// ��ѯĳ���µ������ֶ���Ϣ
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private List<ColumnParam> GetTableColumns(string schemaName, string tableName)
        {
            if (config.DbType == DatabaseType.OpenGauss)
            {
                DataContextScope.ClearCurrent();
                Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr(schemaName));
            }
            using (var scope = DataContextScope.GetCurrent().Begin())
            {
                string sql = config.GetSelColumnSql(schemaName, tableName);
                List<ColumnParam> result = scope.DataContext.Query<ColumnParam>(sql);

                sql = config.GetSelPrimaryKeySql(schemaName, tableName);
                string primaryKeyStr = scope.DataContext.ExecuteScalar<string>(sql);
                if (!string.IsNullOrEmpty(primaryKeyStr))
                {
                    List<string> primaryKeys = primaryKeyStr.Split(',').Distinct().ToList();
                    foreach (var node in result)
                    {
                        if (primaryKeys.Contains(node.Name))
                        {
                            node.IsPrimaryKey = true;
                        }
                        node.Name = node.Name.ToUpper();
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// ��ѯ���ݿ�ģʽ��
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
                        Title = schemaName.ToUpper()
                    });
                }
            }
            return treeNodes;
        }

        #endregion

        #region ��������

        /// <summary>
        /// �޸�ҳ����ʾ��Ϣ
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

        /// <summary>
        /// �������ô���
        /// </summary>
        private void ShowConfigWindow()
        {
            ConfigWindow signInWindow = new(config)
            {
                ConfigCallback = ConfigCallback
            };
            signInWindow.ShowDialog(this);
        }

        /// <summary>
        /// ���ڷ����¼�
        /// </summary>
        /// <param name="result"></param>
        private void ConfigCallback(DatabaseConfig result)
        {
            // �����ص�����
            if (result != null)
            {
                config = result;
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
                else if (config.DbType == DatabaseType.OpenGauss)
                {
                    Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "Npgsql");
                    Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr());
                }
            }
        }

        #endregion

        #region �ؼ������¼�

        /// <summary>
        /// ��״���ѡ�иı�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var ent = tv.SelectedItem as TreeNode;
            if (ent != null)
            {
                if (ent.Level == 0)
                {
                    // ģʽ
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
                    ent.SubNodes = tableNodes;
                    //tv.ItemContainerGenerator.ItemContainerPrepared(tv, ent);
                    if (tableNodes.Count > 0 && tv.SelectedItem is TreeViewItem selItem)
                    {
                        selItem.IsExpanded = true;
                    }
                }
                else if (ent.Level == 1)
                {
                    // ��
                    string parentTitle = tv.ItemsSource?.Cast<TreeNode>().Where(m => m.SubNodes.Contains(ent)).Select(m => m.Title).FirstOrDefault() ?? string.Empty;
                    dg.ItemsSource = ent.Columns = GetTableColumns(parentTitle, ent.Title);
                }
            }
        }

        /// <summary>
        /// ���ð�ť�¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConfigClick(object? sender, RoutedEventArgs e)
        {
            ShowConfigWindow();
        }

        /// <summary>
        /// ���Ӱ�ť�¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnConnectClick(object? sender, RoutedEventArgs e)
        {
            var result = await ConfirmWindow.Show(this, $"ȷ���������ݿ�{config.IP}:{config.Port}", "��ʾ");
            if (result)
            {
                try
                {
                    DataContextScope.ClearCurrent();
                    tv.ItemsSource = GetSchameNames();
                    ChangeShowMsg("���ӳɹ�");
                }
                catch (Exception ex)
                {
                    ChangeShowMsg($"�����쳣:{ex.Message}");
                }
            }
            else
            {
                _ = TipWindow.Show(this, "ȡ������", "��ʾ");
            }
        }

        /// <summary>
        /// ��ʼ��ť�¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStartClick(object? sender, RoutedEventArgs e)
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
                            param.ClassName = CodeDomHelper.ConvertToCamelCase(param.TableName, false, config.RemovePre);
                            param.ClassCom = table.Comments;
                            if (table.Columns.Count == 0)
                            {
                                table.Columns = GetTableColumns(schame.Title, table.Title);
                            }
                            param.Parameters = table.Columns;
                            table.IsChecked = false;
                            list.Add(param);
                        }
                    }
                }

                if (list.Count == 0)
                {
                    ChangeShowMsg("û��ѡ�б�");
                    return;
                }
                ChangeShowMsg("������");
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
                            var res = CommandBus.Execute<DmCmd, CmdResCode>(new DmCmd() { Config = config, Classes = classes });
                            ChangeShowMsg(res.Msg, action);
                        }
                        else if (currentType == DatabaseType.Mysql)
                        {
                            var res = CommandBus.Execute<MysqlCmd, CmdResCode>(new MysqlCmd() { Config = config, Classes = classes });
                            ChangeShowMsg(res.Msg, action);
                        }
                        else if (currentType == DatabaseType.OpenGauss)
                        {
                            ChangeShowMsg("OpenGauss���ݿ���δ���", action);
                        }
                        else
                        {
                            ChangeShowMsg("δ֪���ݿ�����", action);
                        }
                    }
                    else
                    {
                        ChangeShowMsg("����ʧ�ܣ�ʵ����󲻿�Ϊ��", action);
                    }
                }, Tuple.Create(config.DbType, list));
            }
            else
            {
                ChangeShowMsg("û��ģʽ");
            }
        }

        /// <summary>
        /// �Զ����������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTestClick(object? sender, RoutedEventArgs e)
        {
            // ���β���
            //Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "DmClientFactory");
            //Environment.SetEnvironmentVariable("DefaultConnectionString", "Server=192.168.100.198;PORT=5236;USER ID=SYSDBA;PWD=654#@!qaz;SCHEMA=TEST_DB;");

            // Mysql����
            //Environment.SetEnvironmentVariable("DefaultConnectionString.ProviderName", "MySqlConnector");
            //Environment.SetEnvironmentVariable("DefaultConnectionString", "Server=192.168.100.198;port=3306;user=root;password=654#@!qaz;Database=test_db;");

            //Zyh.Common.Service.TSysDictDataSqlService service = new Zyh.Common.Service.TSysDictDataSqlService();

            //var ent = new TSysDictDataEntity()
            //{
            //    CATG_ID = "CameraStatus",
            //    DICT_KEY = "TestKey",
            //    DICT_VALUE = "TestValue",
            //};
            //service.Add(ent);
            //var lsit = service.FindAll(string.Empty);
            //var pager = service.GetPager(1, 3, string.Empty);

            //var ent = service.Get(274);
            //ent.DICT_VALUE = "TestValue2";
            //service.Update(ent);
            //service.Delete(ent.ID);
        }

        /// <summary>
        /// ����ر����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            // ���˳�
            // e.Cancel = true;
        }

        /// <summary>
        /// ����رպ��¼�
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
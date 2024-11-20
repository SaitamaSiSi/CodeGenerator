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

namespace CodeGenerator
{
    public partial class MainWindow : Window
    {
        readonly DatabaseConfig config = new();

        #region ����ͳ�ʼ��

        public MainWindow()
        {
            InitializeComponent();

            InitComponents();
            InitData();
        }

        private void InitComponents()
        {
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;

            db_type.SelectionChanged += OnDbTypeChanged;
            db_port.TextChanging += PortChanging;
            tv.SelectionChanged += OnSelectionChanged;

            // ���� IdGeneratorOptions ���󣬿��ڹ��캯�������� WorkerId��
            var options = new IdGeneratorOptions();
            // options.WorkerIdBitLength = 10; // Ĭ��ֵ6���޶� WorkerId ���ֵΪ2^6-1����Ĭ�����֧��64���ڵ㡣
            // options.SeqBitLength = 6; // Ĭ��ֵ6������ÿ�������ɵ�ID�������������ٶȳ���5���/�룬����Ӵ� SeqBitLength �� 10��
            // options.BaseTime = Your_Base_Time; // ���Ҫ������ϵͳ��ѩ���㷨���˴�Ӧ����Ϊ��ϵͳ��BaseTime��
            // ...... ���������ο� IdGeneratorOptions ���塣

            // �����������ص��ã�����������ò���Ч����
            YitIdHelper.SetIdGenerator(options);
        }

        private void InitData()
        {
            // ��ʼ�����ݿ�������Ϣ
            db_type.SelectedIndex = 0;
            db_ip.Text = "192.168.100.198";
            db_port.Text = "5236";
            db_id.Text = "SYSDBA";
            db_pwd.Text = "654#@!qaz";
            DbProviderFactories.RegisterFactory("DmClientFactory", Dm.DmClientFactory.Instance);
            DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnector.MySqlConnectorFactory.Instance);
        }

        private bool SetDbConn()
        {
            if (!RegexHelper.IsValidIPv4(db_ip.Text ?? string.Empty))
            {
                ChangeShowMsg("IP��ַ�����Ϲ淶");
                return false;
            }
            config.IP = db_ip.Text ?? string.Empty;
            config.Port = Convert.ToInt32(db_port.Text);
            config.UserName = db_id.Text ?? string.Empty;
            config.Password = db_pwd.Text ?? string.Empty;
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

        #endregion

        #region ���ݿ⸨������

        /// <summary>
        /// ��ѯĳģʽ�µ����б���Ϣ
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
        /// ��ѯĳ���µ������ֶ���Ϣ
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
                        Title = schemaName
                    });
                }
            }
            return treeNodes;
        }

        #endregion

        #region ��������

        private void ChangeShowMsg(string msg)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                show_msg.Text = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} | {msg}";
                ToolTip.SetTip(show_msg, show_msg.Text);
            });
        }

        #endregion

        #region �ؼ������¼�

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

        private void PortChanging(object? sender, TextChangingEventArgs e)
        {
            // ʹ��������ʽȷ��ֻ��������
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

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    ent.SubNodes.Clear();
                    ent.SubNodes = tableNodes;
                }
                else if (ent.Level == 1)
                {
                    // ��
                    string parentTitle = tv.ItemsSource?.Cast<TreeNode>().Where(m => m.SubNodes.Contains(ent)).Select(m => m.Title).FirstOrDefault() ?? string.Empty;
                    dg.ItemsSource = ent.Columns = GetTableColumns(parentTitle, ent.Title);
                }
            }
        }

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
                ChangeShowMsg("���ӳɹ�");
            }
            catch (Exception ex)
            {
                ChangeShowMsg($"�����쳣:{ex.Message}");
            }
        }

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
                    ChangeShowMsg("û��ѡ�б�");
                }
                else
                {
                    ChangeShowMsg("������");
                }
                _ = Task.Factory.StartNew((obj) =>
                {
                    Tuple<DatabaseType, List<ClassParam>>? tuple = (Tuple<DatabaseType, List<ClassParam>>?)obj;
                    if (tuple != null)
                    {
                        DatabaseType currentType = tuple.Item1;
                        List<ClassParam> classes = tuple.Item2;
                        if (currentType == DatabaseType.Dm)
                        {
                            CommandBus.Execute<DmCmd, CmdResCode>(new DmCmd() { Command = DatabaseType.Dm, Classes = classes });
                            ChangeShowMsg("���ɳɹ�");
                        }
                        else
                        {
                            ChangeShowMsg("������δʵ��");
                        }
                    }
                }, Tuple.Create(config.DbType, list));
            }
            ChangeShowMsg("û��ģʽ");
        }

        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            // ���˳�
            // e.Cancel = true;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            DataContextScope.ClearCurrent();
        }

        #endregion

    }
}
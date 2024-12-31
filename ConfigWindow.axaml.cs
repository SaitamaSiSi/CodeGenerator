using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CodeGenerator.Core;
using CodeGenerator.Model;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Zyh.Common.Entity;

namespace CodeGenerator;

public partial class ConfigWindow : Window
{
    private readonly DatabaseConfig Config = new();
    public Action<DatabaseConfig>? ConfigCallback { get; set; }

    public ConfigWindow()
    {
        InitializeComponent();
    }

    public ConfigWindow(DatabaseConfig config)
    {
        InitializeComponent();
        title_bar.OnPointerMouseHander += TitleBarOnPointerMouseHander;
        title_bar.Title = "数据库配置";

        Config = config;
        if (Config.DbType == DatabaseType.Dm)
        {
            db_type.SelectedIndex = 0;
        }
        else if (Config.DbType == DatabaseType.Mysql)
        {
            db_type.SelectedIndex = 1;
        }
        db_ip.Text = Config.IP;
        db_port.Text = Config.Port.ToString();
        db_id.Text = Config.UserName;
        db_pwd.Text = Config.Password;

        entity_ns.Text = Config.ClassNameSpace;
        provider_ns.Text = Config.ProviderNameSpace;
        service_ns.Text = Config.ServiceNameSpace;
        del_pre.Text = string.Join(",", Config.RemovePre);

        db_type.SelectionChanged += OnDbTypeChanged;
        db_port.TextChanging += PortChanging;
    }

    private void TitleBarOnPointerMouseHander(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    /// <summary>
    /// 数据库类型改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDbTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            switch (db_type.SelectedIndex)
            {
                case 0:
                    Config.DbType = DatabaseType.Dm;
                    break;
                case 1:
                    Config.DbType = DatabaseType.Mysql;
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
    /// 确定事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BtnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (!RegexHelper.IsValidIPv4(db_ip.Text ?? string.Empty))
        {
            tbx_tip.Text = "IP地址不符合规范";
            return;
        }

        if (!string.IsNullOrEmpty(entity_ns.Text) && !string.Equals(entity_ns.Text, "Zyh.Common.Entity"))
        {
            var result = await ConfirmWindow.Show(this, $"实体层命名空间并非默认值，生成后可能需改动相关文件命名空间，是否继续", "提示");
            if (!result)
            {
                return;
            }
        }
        if (!string.IsNullOrEmpty(provider_ns.Text) && !string.Equals(provider_ns.Text, "Zyh.Common.Provider"))
        {
            var result = await ConfirmWindow.Show(this, $"数据链路层命名空间并非默认值，生成后可能需改动相关文件命名空间，是否继续", "提示");
            if (!result)
            {
                return;
            }
        }
        if (!string.IsNullOrEmpty(service_ns.Text) && !string.Equals(service_ns.Text, "Zyh.Common.Service"))
        {
            var result = await ConfirmWindow.Show(this, $"业务逻辑层层命名空间并非默认值，生成后可能需改动相关文件命名空间，是否继续", "提示");
            if (!result)
            {
                return;
            }
        }

        Config.IP = db_ip.Text ?? string.Empty;
        Config.Port = Convert.ToInt32(db_port.Text);
        Config.UserName = db_id.Text ?? string.Empty;
        Config.Password = db_pwd.Text ?? string.Empty;
        Config.ClassNameSpace = entity_ns.Text ?? "Zyh.Common.Entity";
        Config.ProviderNameSpace = provider_ns.Text ?? "Zyh.Common.Provider";
        Config.ServiceNameSpace = service_ns.Text ?? "Zyh.Common.Service";
        if (del_pre.Text != null && !string.IsNullOrEmpty(del_pre.Text))
        {
            Config.RemovePre = del_pre.Text.Split(",").Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        else
        {
            Config.RemovePre.Clear();
        }

        // 调用委托
        ConfigCallback?.Invoke(Config);
        Close();
    }

    /// <summary>
    /// 取消事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
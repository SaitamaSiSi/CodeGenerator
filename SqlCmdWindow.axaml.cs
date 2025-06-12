using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using CodeGenerator.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;
using Tmds.DBus.Protocol;
using Zyh.Common.Entity;
using static System.Formats.Asn1.AsnWriter;

namespace CodeGenerator;

public partial class SqlCmdWindow : Window
{
    public Action? SqlCallback { get; set; }

    public SqlCmdWindow()
    {
        InitializeComponent();

        InitComponents();
    }

    public SqlCmdWindow(DatabaseConfig config, string currentSchema)
    {
        InitializeComponent();

        Environment.SetEnvironmentVariable("DefaultConnectionString", config.GetConnectStr(currentSchema));
        DataContextScope.ClearCurrent();

        InitComponents();
    }

    private void InitComponents()
    {
        title_bar.Title = "SQL Ö´ÐÐ´°¿Ú";
        sql_table.Text = "SELECT * FROM GEOMETRY";

        Closed += ClosedEvent;
    }

    private void ClosedEvent(object? sender, EventArgs e)
    {
        SqlCallback?.Invoke();
        DataContextScope.ClearCurrent();
    }

    private void BtnExecuteClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(sql_table.Text))
        {
            return;
        }

        try
        {
            sql_data.IsVisible = true;
            sql_err.IsVisible = false;

            var dataGrid = this.FindControl<DataGrid>("sql_data");
            if (dataGrid == null)
            {
                return;
            }
            dataGrid.Columns.Clear();
            dataGrid.ItemsSource = null;

            using var scope = DataContextScope.GetCurrent().Begin();
            using var cmd = scope.DataContext.DatabaseObject.GetSqlStringCommand(sql_table.Text);
            using var reader = scope.DataContext.ExecuteReader(cmd);
            if (reader.FieldCount == 0)
            {
                sql_data.IsVisible = false;
                sql_err.IsVisible = true;
                sql_err.Text = $"Ö´ÐÐ SQL Óï¾ä³É¹¦";
            }
            else
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    dataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = columnName,
                        Binding = new Binding($"[{columnName}]")
                    });
                }
                var itemSource = new ObservableCollection<dynamic>();
                while (reader.Read())
                {
                    dynamic ent = new ExpandoObject();
                    var entDict = (IDictionary<string, object?>)ent;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var dataType = reader.GetDataTypeName(i);
                        if (dataType.ToLower().StartsWith("timestamp"))
                        {
                            DateTime timestamp = reader.GetDateTime(i);
                            string time = timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff").TrimEnd('0').TrimEnd('.');
                            entDict.Add(reader.GetName(i), time);
                        }
                        else if (string.Equals("date", dataType, StringComparison.OrdinalIgnoreCase))
                        {
                            DateTime timestamp = reader.GetDateTime(i);
                            string time = timestamp.ToString("yyyy-MM-dd");
                            entDict.Add(reader.GetName(i), time);
                        }
                        else
                        {
                            entDict.Add(reader.GetName(i), reader.GetValue(i));
                        }
                    }
                    itemSource.Add(ent);
                }
                dataGrid.ItemsSource = itemSource;
            }
        }
        catch (Exception ex)
        {
            sql_data.IsVisible = false;
            sql_err.IsVisible = true;
            sql_err.Text = $"Ö´ÐÐ SQL Óï¾äÊ§°Ü: {ex.Message}";
        }
    }
}
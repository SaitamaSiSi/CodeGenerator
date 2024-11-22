using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace CodeGenerator;

public partial class TipWindow : Window
{
    public TipWindow()
    {
        InitializeComponent();
    }

    public TipWindow(string msg, string title)
    {
        InitializeComponent();

        this.Title = title;
        show_text.Text = msg;
    }

    /// <summary>
    /// 显示方法
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static async Task<bool> Show(Window owner, string message, string title)
    {
        var dialog = new TipWindow(message, title);
        var result = await dialog.ShowDialog<bool?>(owner);
        return result.HasValue && result.Value;
    }

    /// <summary>
    /// 确定事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnSaveClick(object sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
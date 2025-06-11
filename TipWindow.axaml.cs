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

    public TipWindow(string msg)
    {
        InitializeComponent();

        show_text.Text = msg;
    }

    /// <summary>
    /// 显示方法
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static void Show(Window owner, string message, string title)
    {
        var dialog = new TipWindow(message)
        {
            Title = title
        };
        dialog.ShowDialog(owner);
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CodeGenerator;

public partial class WindowsTitleBar : UserControl
{
    private bool _isShining = false;
    private Window? _parentWindow;
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WindowsTitleBar, string>(nameof(Title));

    /// <summary>
    /// ´°¿ÚÒÆ¶¯
    /// </summary>
    public event EventHandler<PointerPressedEventArgs>? OnPointerMouseHander;

    public string Title
    {
        get { return GetValue(TitleProperty); }
        set
        {
            SetValue(TitleProperty, value);
            sys_title.Text = value;
        }
    }

    public WindowsTitleBar()
    {
        InitializeComponent();
        Loaded += WindowLoaded;
        PointerPressed += WindowsTitleBarPointerPressed;
        btn_close.Click += CloseWindow;
    }

    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        var root = this.GetVisualRoot();
        if (root != null && root is Window parentWindow && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _parentWindow = parentWindow;
            var handle = _parentWindow.TryGetPlatformHandle();
            if (handle != null)
            {
                // handle.Handle;
            }
        }
    }

    private void WindowsTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            OnPointerMouseHander?.Invoke(sender, e);
        }
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        if (VisualRoot != null && VisualRoot is Window hostWindow)
        {
            hostWindow.Close();
        }
    }

    private void StartShine()
    {
        if (!_isShining)
        {
            _isShining = true;
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            sys_bar.Opacity = i % 2 == 0 ? 0.5 : 1;
                        });
                        Thread.Sleep(200);
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    _isShining = false;
                }
            });
        }
    }
}
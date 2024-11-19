using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeGenerator.Generator;
using System;

namespace CodeGenerator
{
    public partial class MainWindow : Window
    {
        const string DmConnStr = "Server=192.168.100.198;PORT=5236;DATABASE=TEST_DB;USER ID=SYSDBA;PWD=654#@!qaz;SCHEMA=TEST_DB";
        DmGenerator dmGenerator = new DmGenerator(DmConnStr);

        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;
        }

        private void btnStartClick(object sender, RoutedEventArgs e)
        {
            dmGenerator.CreateEntities();
        }

        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            // ²»ÍË³ö
            // e.Cancel = true;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            dmGenerator?.Dispose();
        }
    }
}
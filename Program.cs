using Avalonia;
using Avalonia.Media;
using System;

namespace CodeGenerator
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new FontManagerOptions
                {
                    // jtbz_c.ttf|jtbz_c，jtbz_d.ttf|jtbz_d，STJTBZ.ttf|STJTBZ
                    // Source Han Sans CN.ttf|Source Han Sans CN，Source Han Serif CN.ttf|Source Han Serif CN
                    // 仿宋.ttf|FangSong，宋体.ttf|SimSun，楷体.ttf|KaiTi，黑体.ttf|SimHei
                    // 此处为全局设置，也可以在App.axaml配置并在页面中使用FontFamily="{StaticResource STJTBZ}配置字体
                    DefaultFamilyName = "avares://CodeGenerator/Resources/Fonts/STJTBZ.ttf#STJTBZ",
                    FontFallbacks = new[]{
                        new FontFallback
                        {
                            FontFamily = new FontFamily("avares://CodeGenerator/Resources/Fonts/STJTBZ.ttf#STJTBZ")
                        }
                    }
                })
                //.WithInterFont()
                .LogToTrace();
    }
}
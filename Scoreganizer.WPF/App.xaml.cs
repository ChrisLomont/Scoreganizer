using MvvmCross.Core;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Views;

namespace Lomont.Scoreganizer.WPF
{
public partial class App : MvxApplication
{
    public App()
    {
        this.RegisterSetupType<MvxWpfSetup<Lomont.Scoreganizer.Core.App>>();
    }
}
}
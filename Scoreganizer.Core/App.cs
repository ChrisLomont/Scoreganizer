using Lomont.Scoreganizer.Core.ViewModels;
using MvvmCross.IoC;
using MvvmCross.ViewModels;

namespace Lomont.Scoreganizer.Core
{
public class App : MvxApplication
{
    public override void Initialize()
    {
        CreatableTypes()
            .EndingWith("Service")
            .AsInterfaces()
            .RegisterAsLazySingleton();
        RegisterAppStart<SongsViewModel>();
    }
}
}
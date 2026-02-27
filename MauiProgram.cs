using Microsoft.Extensions.Logging;
using RagMaui.Services;
using RagMaui.ViewModels;
using RagMaui.Views;

namespace RagMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<RagService>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<WorkspaceSettingsViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<WorkspaceSettingsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
using Microsoft.Extensions.Logging;
using VectorDataBase.Services;
using SimiliVec.MAUI.ViewModels;

namespace SimiliVec.MAUI;

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
			});

		// Register VectorDataBase services
		builder.Services.AddVectorDataBaseServices();

		// Register MAUI services
		builder.Services.AddSingleton<SearchPageViewModel>();
		builder.Services.AddSingleton<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

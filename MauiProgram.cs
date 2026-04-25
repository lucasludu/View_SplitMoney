using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using SplitMoney.Client.Infrastructure;
using SplitMoney.Client.Services;

namespace SplitMoney.Client;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		try 
		{
			Console.WriteLine("STARTUP: Initializing Builder...");
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			Console.WriteLine("STARTUP: Adding Services...");
			builder.Services.AddMauiBlazorWebView();
			builder.Services.AddBlazoredLocalStorage();

	#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
	#endif

			// Authentication and Authorization services registration
			builder.Services.AddAuthorizationCore();
			builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IExpenseService, ExpenseService>();
			builder.Services.AddScoped<IToastService, ToastService>();
			builder.Services.AddScoped<IModalService, ModalService>();
			builder.Services.AddScoped<ITipService, TipService>();
			builder.Services.AddScoped<INotificationService, NotificationService>();
			builder.Services.AddScoped<ICacheService, CacheService>();
			builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

			// HTTP Handlers registration
			builder.Services.AddTransient<AuthenticationHeaderHandler>();
			builder.Services.AddTransient<RefreshTokenHandler>();
			builder.Services.AddTransient<ConnectivityHandler>();

			// Dynamic API URL for Android Emulator vs Windows (Using HTTPS with bypass)
			string baseAddress = DeviceInfo.Platform == DevicePlatform.Android 
				? "https://10.0.2.2:7042/" 
				: "https://localhost:7042/";

			Console.WriteLine($"STARTUP: API BaseAddress = {baseAddress}");
			builder.Services.AddTransient<DevelopmentHttpClientHandler>();

			builder.Services.AddHttpClient("SplitMoneyApi", cl => 
			{
				cl.BaseAddress = new Uri(baseAddress);
			})
			.ConfigurePrimaryHttpMessageHandler<DevelopmentHttpClientHandler>()
			.AddHttpMessageHandler<ConnectivityHandler>()
			.AddHttpMessageHandler<AuthenticationHeaderHandler>()
			.AddHttpMessageHandler<RefreshTokenHandler>();

			builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SplitMoneyApi"));

			Console.WriteLine("STARTUP: Building MauiApp...");
			var app = builder.Build();
			Console.WriteLine("STARTUP: MauiApp Built Successfully!");
			return app;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"CRITICAL STARTUP ERROR: {ex.Message}");
			Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
			throw;
		}
	}
}

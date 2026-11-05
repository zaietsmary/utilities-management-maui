using Microsoft.Extensions.Logging;
using Lab7.Data;
using Lab7.Views;
using Microsoft.EntityFrameworkCore;

namespace Lab7;

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

		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=Utilities;Trusted_Connection=True;TrustServerCertificate=True;"));

		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<ServicesPage>();
		builder.Services.AddTransient<CitiesPage>();
		builder.Services.AddTransient<TenantsPage>();
		builder.Services.AddTransient<AccountsPage>();
		builder.Services.AddTransient<BillingPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

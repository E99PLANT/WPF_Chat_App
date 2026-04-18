using Chat_Group_System.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace Chat_Group_System
{
    public partial class App : Application
    {
        public static Chat_Group_System.Models.Entities.User? CurrentUser { get; set; }
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        public static IConfiguration Configuration { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ── Load appsettings.json ──────────────────────────
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // ── DI Container ───────────────────────────────────
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // ── Apply pending migrations on startup ────────────
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NexChatDbContext>();
            db.Database.Migrate();

            // ── Launch LoginWindow ─────────────────────────
            var loginWindow = ServiceProvider.GetRequiredService<Views.LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add IConfiguration
            services.AddSingleton<IConfiguration>(Configuration);

            // DbContext — Code First với SQL Server
            services.AddDbContext<NexChatDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure(maxRetryCount: 3)
                ), ServiceLifetime.Transient
            );

            // Repositories
            services.AddTransient<Repositories.IUserRepository, Repositories.UserRepository>();
            services.AddTransient<Repositories.IConversationRepository, Repositories.ConversationRepository>();
            services.AddTransient<Repositories.IMessageRepository, Repositories.MessageRepository>();

            // Services
            services.AddTransient<Services.IUserService, Services.UserService>();
            services.AddTransient<Services.IConversationService, Services.ConversationService>();
            services.AddTransient<Services.IMessageService, Services.MessageService>();
            services.AddSingleton<Services.ISignalRService, Services.SignalRService>();

            // Controllers
            services.AddTransient<Controllers.UserController>();
            services.AddTransient<Controllers.ChatController>();

            // Windows / Views
            services.AddTransient<MainWindow>();
            services.AddTransient<Views.LoginWindow>();
            services.AddTransient<Views.RegisterWindow>();
            services.AddTransient<Views.ChangePasswordWindow>();
            services.AddTransient<Views.GroupSettingsWindow>();
            services.AddTransient<Views.CreateGroupWindow>();
        }
    }
}

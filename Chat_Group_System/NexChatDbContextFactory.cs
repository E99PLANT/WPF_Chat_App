using Chat_Group_System.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Chat_Group_System
{
    /// <summary>
    /// Design-time factory — giúp EF Tools (dotnet ef migrations add)
    /// khởi tạo NexChatDbContext mà không cần chạy WPF app.
    /// </summary>
    public class NexChatDbContextFactory : IDesignTimeDbContextFactory<NexChatDbContext>
    {
        public NexChatDbContext CreateDbContext(string[] args)
        {
            // Load appsettings.json từ thư mục project
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<NexChatDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new NexChatDbContext(optionsBuilder.Options);
        }
    }
}

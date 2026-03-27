using System;
using Abp.AspNetCore.Dependency;
using Abp.Dependency;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace backend.Web.Host.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Npgsql requires UTC timestamps; this switch makes it accept Local/Unspecified DateTimes
            // by treating them as UTC — matches ABP's internal seeding behaviour.
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            CreateHostBuilder(args).Build().Run();
        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseCastleWindsor(IocManager.Instance.IocContainer);
    }
}



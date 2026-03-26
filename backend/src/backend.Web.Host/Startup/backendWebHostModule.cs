using Abp.Modules;
using Abp.Reflection.Extensions;
using backend.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace backend.Web.Host.Startup
{
    [DependsOn(
       typeof(backendWebCoreModule))]
    public class backendWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public backendWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(backendWebHostModule).GetAssembly());
        }
    }
}



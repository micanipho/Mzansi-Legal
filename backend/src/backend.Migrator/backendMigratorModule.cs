using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using backend.Configuration;
using backend.EntityFrameworkCore;
using backend.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace backend.Migrator;

[DependsOn(typeof(backendEntityFrameworkModule))]
public class backendMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public backendMigratorModule(backendEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(backendMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            backendConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(backendMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}



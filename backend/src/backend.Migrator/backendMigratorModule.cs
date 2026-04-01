using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using backend.Configuration;
using backend.EntityFrameworkCore;
using backend.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;
using System;

namespace backend.Migrator;

[DependsOn(typeof(backendEntityFrameworkModule), typeof(backendApplicationModule))]
public class backendMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public backendMigratorModule(backendEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        _appConfiguration = AppConfigurations.Get(
            typeof(backendMigratorModule).GetAssembly().GetDirectoryPathOrNull(),
            environmentName
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
        ServiceCollectionRegistrar.Register(IocManager, _appConfiguration);
    }
}

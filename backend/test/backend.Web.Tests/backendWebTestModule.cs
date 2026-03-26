using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using backend.EntityFrameworkCore;
using backend.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace backend.Web.Tests;

[DependsOn(
    typeof(backendWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class backendWebTestModule : AbpModule
{
    public backendWebTestModule(backendEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(backendWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(backendWebMvcModule).Assembly);
    }
}


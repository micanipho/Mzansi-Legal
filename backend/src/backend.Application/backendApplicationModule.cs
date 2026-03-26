using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using backend.Authorization;

namespace backend;

[DependsOn(
    typeof(backendCoreModule),
    typeof(AbpAutoMapperModule))]
public class backendApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<backendAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(backendApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}



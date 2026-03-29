using Abp.Dependency;
using backend.Identity;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace backend.Migrator.DependencyInjection;

public static class ServiceCollectionRegistrar
{
    public static void Register(IIocManager iocManager, IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddSingleton(configuration);
        IdentityRegistrar.Register(services);
        services.AddHttpClient("OpenAI", client =>
        {
            var baseUrl = configuration["OpenAI:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }

            client.Timeout = TimeSpan.FromSeconds(30);
        });

        WindsorRegistrationHelper.CreateServiceProvider(iocManager.IocContainer, services);
    }
}

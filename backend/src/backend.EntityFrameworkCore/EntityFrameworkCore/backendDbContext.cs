using Abp.Zero.EntityFrameworkCore;
using backend.Authorization.Roles;
using backend.Authorization.Users;
using backend.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace backend.EntityFrameworkCore;

public class backendDbContext : AbpZeroDbContext<Tenant, Role, User, backendDbContext>
{
    /* Define a DbSet for each entity of the application */

    public backendDbContext(DbContextOptions<backendDbContext> options)
        : base(options)
    {
    }
}



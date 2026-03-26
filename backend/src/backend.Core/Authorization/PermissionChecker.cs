using Abp.Authorization;
using backend.Authorization.Roles;
using backend.Authorization.Users;

namespace backend.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}



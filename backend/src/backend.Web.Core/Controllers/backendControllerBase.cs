using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace backend.Controllers
{
    public abstract class backendControllerBase : AbpController
    {
        protected backendControllerBase()
        {
            LocalizationSourceName = backendConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}



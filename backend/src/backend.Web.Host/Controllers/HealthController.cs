using backend.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Host.Controllers
{
    /// <summary>
    /// Provides a lightweight health check endpoint used by Railway's deploy health check
    /// and any external uptime monitoring tools. No authentication is required.
    /// </summary>
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class HealthController : backendControllerBase
    {
        /// <summary>
        /// Returns HTTP 200 with a JSON body confirming the service is running.
        /// Railway polls this endpoint after each deployment to verify the container is healthy.
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy" });
        }
    }
}

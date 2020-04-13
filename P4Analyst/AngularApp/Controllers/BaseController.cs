using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AngularApp.Controllers
{
    [ApiController]
    public class BaseController<T> : ControllerBase
    {
        protected readonly ISession session;
        protected readonly ILogger<T> logger;

        public BaseController(ILogger<T> logger, IHttpContextAccessor http)
        {
            this.session = http.HttpContext.Session;
            this.logger = logger;
        }

        protected IActionResult ReturnAction(ActionResult action)
        {
            try
            {
                return action;
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("Váratlan hiba!");
            }
        }
    }
}
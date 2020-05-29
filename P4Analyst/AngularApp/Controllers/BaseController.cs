using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularApp.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly ISession session;

        public BaseController(IHttpContextAccessor http)
        {
            session = http.HttpContext.Session;
        }

        protected IActionResult ActionExecute(Func<IActionResult> logic)
        {
            try
            {
                return logic();
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
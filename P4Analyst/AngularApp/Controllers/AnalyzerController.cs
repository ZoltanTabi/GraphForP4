using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngularApp.Extensions;
using GraphForP4.Models;
using GraphForP4.Services;
using GraphForP4.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AngularApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AnalyzerController : BaseController<AnalyzerController>
    {
        public AnalyzerController(ILogger<AnalyzerController> logger, IHttpContextAccessor http)
            : base(logger, http) { }

        [HttpGet]
        public IActionResult GetVariables()
        {
            logger.LogInformation("Header rész lekérdezése");

            try
            {
                var file = SessionExtension.Get<string>(session, Key.File);

                if (file == null) return BadRequest("Kérem töltsön fel először fájlt!");

                var structs = Analyzer.GetVariables(file);

                SessionExtension.Set(session, Key.Struct, structs);

                return Ok(structs);
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

        [HttpPut]
        public List<Struct> Update([FromBody]List<Struct> structs)
        {
            if (structs.Count > 0)
            {
                
            }

            return structs;
        }
    }
}

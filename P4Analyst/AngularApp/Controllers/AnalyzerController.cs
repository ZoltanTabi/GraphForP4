using System;
using System.Collections.Generic;
using AngularApp.Extensions;
using GraphForP4.Models;
using GraphForP4.Services;
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
            return ActionExecute(() =>
            {
                logger.LogInformation("Header rész lekérdezése");

                var file = SessionExtension.Get<string>(session, Key.File);

                if (file == null) return BadRequest("Kérem töltsön fel először fájlt!");

                var structs = Analyzer.GetVariables(file);

                SessionExtension.Set(session, Key.Struct, structs);

                return Ok(structs);
            });
        }

        [HttpPut]
        public IActionResult Update([FromBody]List<AnalyzeData> analyzeDatas)
        {
            return ActionExecute(() =>
            {
                return Ok(analyzeDatas);
            });
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
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
        public IActionResult GetStructs()
        {
            return ActionExecute(() =>
            {
                logger.LogInformation("Header rész lekérdezése");

                var file = SessionExtension.Get<string>(session, Key.File);

                if (file == null) return BadRequest("Kérem töltsön fel először fájlt!");

                var structs = Analyzer.GetStructs(file);

                SessionExtension.Set(session, Key.Struct, structs);

                return Ok(structs);
            });
        }

        [HttpPut]
        public IActionResult Update([FromBody]List<AnalyzeData> analyzeDatas)
        {
            return ActionExecute(() =>
            {
                var file = SessionExtension.Get<string>(session, Key.File);
                var controlFlowGraphJson = SessionExtension.GetGraph(session, Key.ControlFlowGraph).ToJson();
                var dataFlowGraphJson = SessionExtension.GetGraph(session, Key.DataFlowGraph).ToJson();
                var analyzers = new List<Analyzer>();

                analyzeDatas.ForEach(x =>
                {
                    analyzers.Add(new Analyzer(controlFlowGraphJson, dataFlowGraphJson, x, file));
                });

                Parallel.ForEach(analyzers, (analyzer) =>
                {
                    analyzer.Analyze();
                });

                return Ok(analyzeDatas);
            });
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngularApp.Extensions;
using GraphForP4.Helpers;
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
                var controlFlowGraphJson = session.GetString(Key.ControlFlowGraph.ToString("g"));
                var dataFlowGraphJson = session.GetString(Key.DataFlowGraph.ToString("g"));
                var analyzers = new List<Analyzer>();

                analyzeDatas.ForEach(x =>
                {
                    analyzers.Add(new Analyzer(controlFlowGraphJson, dataFlowGraphJson, x, file));
                });

                analyzers = analyzers.Distinct().ToList();

                Parallel.ForEach(analyzers, (analyzer) =>
                {
                    analyzer.Analyze();
                    analyzer.FinishOperations();
                });

                var calculateData = new CalculatedData
                {
                    ControlFlowGraphs = analyzers.Select(x => GraphToAngular.Serialize(x.ControlFlowGraph)),
                    DataFlowGraphs = analyzers.Select(x => GraphToAngular.Serialize(x.DataFlowGraph)),
                    BarChartData = analyzers.BarChartData()
                };

                return Ok(calculateData);
            });
        }
    }
}

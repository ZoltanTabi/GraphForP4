using System.Collections.Generic;
using System.Threading.Tasks;
using AngularApp.Extensions;
using GraphForP4.Helpers;
using GraphForP4.Services;
using GraphForP4.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AnalyzerController : BaseController
    {
        public AnalyzerController(IHttpContextAccessor http)
            : base(http) { }

        [HttpGet]
        public IActionResult GetStructs()
        {
            return ActionExecute(() =>
            {
                var file = SessionExtension.Get<FileData>(session, Key.File);

                if (file == null) return BadRequest("Kérem töltsön fel először fájlt!");

                var structs = Analyzer.GetStructs(file.Content);

                SessionExtension.Set(session, Key.Struct, structs);

                return Ok(structs);
            });
        }

        [HttpPut]
        public IActionResult Update([FromBody]List<AnalyzeData> analyzeDatas)
        {
            return ActionExecute(() =>
            {
                var file = SessionExtension.Get<FileData>(session, Key.File);
                var controlFlowGraphJson = session.GetString(Key.ControlFlowGraph.ToString("g"));
                var dataFlowGraphJson = session.GetString(Key.DataFlowGraph.ToString("g"));
                var analyzers = new List<Analyzer>();

                analyzeDatas.ForEach(x =>
                {
                    analyzers.Add(new Analyzer(controlFlowGraphJson, dataFlowGraphJson, x, file.Content));
                });

                Parallel.ForEach(analyzers, (analyzer) =>
                {
                    analyzer.Analyze();
                    analyzer.FinishOperations();
                });

                analyzers.DistinctGraphs(out List<List<AngularNode>> controlFlowGraphs, out List<List<AngularNode>> dataFlowGraphs);
                analyzers.CreateCharts(out BarChartData readAndWriteChartData, out PieChartData useVariable, out PieChartData useful, out BarChartData headers);

                var calculateData = new CalculatedData
                {
                    ControlFlowGraphs = controlFlowGraphs,
                    DataFlowGraphs = dataFlowGraphs,
                    ReadAndWriteChartData = readAndWriteChartData,
                    UseVariable = useVariable,
                    Useful = useful,
                    Headers = headers,
                    File = file
                };

                return Ok(calculateData);
            });
        }
    }
}

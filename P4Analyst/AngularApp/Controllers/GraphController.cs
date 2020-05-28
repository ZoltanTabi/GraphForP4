using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GraphForP4.Services;
using GraphForP4.ViewModels;
using AngularApp.Extensions;

namespace AngularApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class GraphController : BaseController
    {
        public GraphController(IHttpContextAccessor http)
            : base(http) { }


        [HttpGet("{type}")]
        public IActionResult GetGraph(string type)
        {
            return ActionExecute(() =>
            {
                var success = Enum.TryParse(type, true, out Key key);

                if (!success) return BadRequest("Érvénytelen behívás!");

                var graph = SessionExtension.GetGraph(session, key);

                if (graph == null || !graph.Nodes.Any()) return BadRequest("Kérem töltsön fel először fájlt!");

                return Ok(GraphToAngular.Serialize(graph));
            });
        }

        [HttpPost]
        public IActionResult FileUpload([FromBody]FileData file)
        {
            return ActionExecute(() =>
            {
                if (file.Content == null || string.IsNullOrWhiteSpace(file.Content)) return BadRequest("Üres fájl!");

                var content = file.Content;

                SessionExtension.Set(session, Key.File, file);

                var controlFlowGraph = P4ToGraph.ControlFlowGraph(ref content);
                SessionExtension.SetGraph(session, Key.ControlFlowGraph, controlFlowGraph);

                var dataFlowGraph = P4ToGraph.DataFlowGraph(content, controlFlowGraph);
                SessionExtension.SetGraph(session, Key.DataFlowGraph, dataFlowGraph);

                return Ok(file);
            });
        }
    }
}

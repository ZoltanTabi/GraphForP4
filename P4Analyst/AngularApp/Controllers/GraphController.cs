using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GraphForP4.Services;
using GraphForP4.Models;
using GraphForP4.ViewModels;
using AngularApp.Extensions;

namespace AngularApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class GraphController : BaseController<GraphController>
    {
        public GraphController(ILogger<GraphController> logger, IHttpContextAccessor http)
            : base(logger, http) { }


        [HttpGet("{type}")]
        public IActionResult GetGraph(string type)
        {
            logger.LogInformation("Gráf lekérdezése", type);

            try
            {
                var success = Enum.TryParse(type, true, out Key key);

                if (!success) return BadRequest("Érvénytelen behívás!");

                var graph = SessionExtension.GetGraph(session, key);

                if (graph == null || !graph.Nodes.Any()) return BadRequest("Kérem töltsön fel először fájlt!");

                return Ok(GraphToAngular.Serialize(graph));
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

        [HttpPost]
        public IActionResult FileUpload([FromBody]FileData file)
        {
            logger.LogInformation("Fájl beküldése", file);

            try
            {
                if (file.Content == null || string.IsNullOrWhiteSpace(file.Content)) return BadRequest("Üres fájl!");

                var content = file.Content;

                SessionExtension.Set(session, Key.File, content);

                var controlFlowGraph = P4ToGraph.ControlFlowGraph(ref content);
                SessionExtension.SetGraph(session, Key.ControlFlowGraph, controlFlowGraph);

                var dataFlowGraph = P4ToGraph.DataFlowGraph(content, controlFlowGraph);
                SessionExtension.SetGraph(session, Key.DataFlowGraph, dataFlowGraph);

                return Ok(file);
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
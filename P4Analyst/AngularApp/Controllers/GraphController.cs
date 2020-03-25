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
    public class GraphController : BaseController<GraphController>
    {
        public GraphController(ILogger<GraphController> logger, IHttpContextAccessor http)
            : base(logger, http) { }


        [HttpGet("{type}")]
        public IActionResult GetGraph(string type)
        {
            logger.LogInformation("Gráf lekérdezése", type);

            var success = Enum.TryParse(type, true, out Key key);

            if(success)
            {
                var file = SessionExtension.Get<FileData>(session, key);
                //return Ok(GraphToAngular.Serialize(SessionExtension.Get<Graph>(session, key)));
                return Ok(GraphToAngular.Serialize(P4ToGraph.Create(file.Content)));
            }
            else
            {
                return NotFound();
            }
           
        }

        [HttpPost]
        public FileData FileUpload([FromBody]FileData file)
        {
            logger.LogInformation("Fájl beküldése", file);

            file.Name = "Failed";
            if(file.Content != null && !string.IsNullOrWhiteSpace(file.Content))
            {
                var graph = P4ToGraph.Create(file.Content);


                var currentNodes = new List<Node>();
                Parallel.ForEach(graph.Nodes, (node) =>
                {
                    if (MainNode(graph, node)) currentNodes.Add(node);
                });



                //SessionExtension.Set(session, Key.ControlFlowGraph, currentNodes);
                SessionExtension.Set(session, Key.ControlFlowGraph, file);
                file.Name = "Success";
                logger.LogInformation("Gráf megalkotása", graph);
            }

            return file;
        }



        private static bool MainNode(Graph graph, Node node)
        {
            var find = false;

            for (var i = 0; i < graph.Nodes.Count && !find; ++i)
            {
                for (var j = 0; j < graph.Nodes[i].Edges.Count && !find; ++j)
                {
                    find = graph.Nodes[i].Edges[j].Child == node;
                }
            }

            return !find;
        }
    }
}
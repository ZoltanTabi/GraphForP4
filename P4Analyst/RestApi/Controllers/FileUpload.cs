using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GraphForP4.Models;
using GraphForP4.Services;
using GraphForP4.ViewModel;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUpload : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(await reader.ReadLineAsync());
            }
            var content = result.ToString();

            var graph = P4ToGraph.Create(content);

            return Ok();
        }
    }
}

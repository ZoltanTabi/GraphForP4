using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GraphForP4.Services;
using GraphForP4.ViewModels;

namespace AngularApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(ILogger<FileUploadController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public FileData Post([FromBody]FileData file)
        {
            file.Name = "Failed";
            if(file.Content != null && !String.IsNullOrWhiteSpace(file.Content))
            {
                var graph = P4ToGraph.Create(file.Content);
                file.Name = "Success";
            }

            return file;
        }
    }
}
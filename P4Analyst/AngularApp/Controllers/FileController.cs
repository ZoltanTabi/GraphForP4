using System.Collections.Generic;
using System.Linq;
using AngularApp.Extensions;
using GraphForP4.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Persistence;

namespace AngularApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class FileController : BaseController<FileController>
    {
        private readonly P4Context context;

        public FileController(ILogger<FileController> logger, IHttpContextAccessor http, P4Context context)
            : base(logger, http)
        {
            this.context = context;        
        }


        [HttpGet("{id}")]
        public IActionResult GetFile(int id)
        {
            return ActionExecute(() =>
            {
                logger.LogInformation($"Fájl lekérdezés: {id}");

                using var service = new Service(context);

                return Ok(service.GetP4File(id).ToFileData());
            });
        }

        [HttpGet]
        public IActionResult GetFiles()
        {
            return ActionExecute(() =>
            {
                logger.LogInformation("Fájlok lekérdezés");

                using var service = new Service(context);
                var fileDatas = new List<FileData>();
                service.GetP4Files().ForEach(x => fileDatas.Add(x.ToFileData()));

                return Ok(fileDatas);
            });
        }

        [HttpPost]
        public IActionResult UploadFileToDataBase([FromBody]FileData file)
        {
            return ActionExecute(() =>
            {
                logger.LogInformation("Fájl feltöltés");

                using var service = new Service(context);

                return Ok(service.SetP4File(file.ToP4File()).ToFileData());
            });
        }
    }
}

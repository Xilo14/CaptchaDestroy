using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace CaptchaDestroy.Web.Api
{
    public class MetaController : BaseApiController
    {
        public MetaController(IDiagnosticContext DiagnosticContext) : base(DiagnosticContext)
        {
        }

        [HttpGet("/info")]
        [SwaggerOperation(
            Summary = "Get meta info",
            Description = "Get meta info",
            OperationId = "GetInfo",
            Tags = new[] { "Meta" })
        ]
        public ActionResult<string> Info()
        {
            var assembly = typeof(Startup).Assembly;

            var creationDate = System.IO.File.GetCreationTime(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            return Ok($"Version: {version}, Last Updated: {creationDate}");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Core.ProjectAggregate.Specifications;
using CaptchaDestroy.SharedKernel.Interfaces;
using CaptchaDestroy.Web.ApiModels;
using Microsoft.AspNetCore.Mvc;
using CaptchaDestroy.Core.ProjectAggregate.Events;
using Swashbuckle.AspNetCore.Annotations;
using Serilog;

namespace CaptchaDestroy.Web.Api
{
    /// <summary>
    /// A sample API Controller. Consider using API Endpoints (see Endpoints folder) for a more SOLID approach to building APIs
    /// https://github.com/ardalis/ApiEndpoints
    /// </summary>
    public class CaptchaController : BaseApiController
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<Captcha> _captchaRepository;
        private readonly ICaptchaService _captchaService;

        public CaptchaController(IRepository<Account> accountRepository,
                                IRepository<Captcha> captchaRepository,
                                ICaptchaService captchaService,
                                IDiagnosticContext diagnosticContext) : base(diagnosticContext)
        {
            _accountRepository = accountRepository;
            _captchaRepository = captchaRepository;
            _captchaService = captchaService;  
        }

        // GET: api/Captcha
        [HttpGet("{secretKey}")]
        [SwaggerOperation(
            Summary = "Get captcha",
            Description = "Gets information about the captcha added earlier. If it is solved the answer includes the solution string.",
            OperationId = "GetCaptcha",
            Tags = new[] { "Captcha" })
        ]
        public async Task<ActionResult<CaptchaDTO>> GetById(string secretKey, int captchaId)
        {
            var captchaSpec = new CaptchaByIdWithAccount(captchaId);
            var captcha = await _captchaRepository.GetBySpecAsync(captchaSpec);

            if (captcha == null)
                return BadRequest("Captcha not found");

            if (captcha.Account.SecretKey != secretKey)
                return BadRequest("Wrong secret key!");

            var result = CaptchaDTO.FromCaptcha(captcha);

            return Ok(result);
        }

        // POST: api/Captcha
        [HttpPost("{secretKey}")]
        [SwaggerOperation(
            Summary = "Adds a new captcha to queue",
            Description = "Adds a new captcha to the solution queue if you have enough funds in your account. Use your private key for authentication.",
            OperationId = "AddCaptcha",
            Tags = new[] { "Captcha" })
        ]
        public async Task<ActionResult<CaptchaDTO>> Post(string secretKey,
            [FromBody] CreateCaptchaDTO request,
            [FromQuery] bool isWait = false)
        {
            Result<Captcha> captcha;
            if (isWait)
                captcha = await _captchaService.AddNewCaptcha(secretKey, request.CaptchaUri);
            else
                captcha = await _captchaService.AddNewCaptcha(secretKey, request.CaptchaUri, PublishStrategy.ParallelNoWait);
            if (captcha.Status == ResultStatus.Ok)
            {
                return Ok(CaptchaDTO.FromCaptcha(captcha.Value));
            }
            else if (captcha.Status == ResultStatus.Invalid)
            {
                return BadRequest(captcha.ValidationErrors);
            }
            else
            {
                return NotFound();
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.Result;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.SharedKernel.Interfaces;
using CaptchaDestroy.Core.ProjectAggregate.Specifications;
using Ardalis.GuardClauses;
using CaptchaDestroy.Core.ProjectAggregate.Events;

namespace CaptchaDestroy.Core.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IRepository<Account> _repository;

        public CaptchaService(IRepository<Account> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Captcha>> AddNewCaptcha(string secretKey,
                                                         Uri captchaUri,
                                                         PublishStrategy? eventsPublishStrategy = null)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrEmpty(secretKey))
                errors.Add(new ValidationError()
                {
                    Identifier = nameof(secretKey),
                    ErrorMessage = $"{nameof(secretKey)} is required."
                });

            if (string.IsNullOrEmpty(captchaUri?.ToString()))
                errors.Add(new ValidationError()
                {
                    Identifier = nameof(captchaUri),
                    ErrorMessage = $"{nameof(captchaUri)} is required."
                });

            if (errors.Any())
                return Result<Captcha>.Invalid(errors);

            var accountSpec = new AccountBySecretKey(secretKey);
            var account = await _repository.GetBySpecAsync(accountSpec);

            if (account == null)
            {
                errors.Add(new ValidationError()
                {
                    Identifier = nameof(secretKey),
                    ErrorMessage = $"{nameof(secretKey)} is wrong."
                });
                return Result<Captcha>.Invalid(errors);
            }

            if (account.Points < 100)
            {
                errors.Add(new ValidationError()
                {
                    Identifier = nameof(account.Points),
                    ErrorMessage = $"{nameof(account.Points)} not enough."
                });
                return Result<Captcha>.Invalid(errors);
            }
            var captcha = new Captcha(account, captchaUri);

            account.AddCaptcha(captcha, eventsPublishStrategy);

            await _repository.UpdateAsync(account);

            return Result<Captcha>.Success(captcha);
        }
    }
}

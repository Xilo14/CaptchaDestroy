using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.Result;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Core.ProjectAggregate.Events;

namespace CaptchaDestroy.Core.Interfaces
{
    public interface ICaptchaService
    {
        Task<Result<Captcha>> AddNewCaptcha(string secretKey, Uri captchaUri, PublishStrategy? eventsPublishStrategy = null);
    }
}

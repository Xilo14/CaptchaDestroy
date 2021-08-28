using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.Result;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Core.ProjectAggregate.Events;

namespace CaptchaDestroy.Core.Interfaces
{
    public interface IAccountService
    {
        Task<Result<Account>> GetOrCreateAccount(long TgId, PublishStrategy? eventsPublishStrategy = null);
        Task<Result<Account>> DepositPoints(long TgId, int Points, PublishStrategy? eventsPublishStrategy = null);
    }
}

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
    public class AccountService : IAccountService
    {
        private readonly IRepository<Account> _repository;

        public AccountService(IRepository<Account> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Account>> GetOrCreateAccount(
            long TgId, PublishStrategy? eventsPublishStrategy = null)
        {
            var accountSpec = new AccountByTgId(TgId);
            var account = await _repository.GetBySpecAsync(accountSpec);

            if (account == null)
            {
                await _repository.AddAsync(account = new Account(TgId));
            }

            await _repository.UpdateAsync(account);

            return Result<Account>.Success(account);
        }
        public async Task<Result<Account>> DepositPoints(
            long TgId, int Amount, PublishStrategy? eventsPublishStrategy = null)
        {
            if (Amount <= 0)
                return Result<Account>.Invalid(new()
                {
                    new ValidationError()
                    {
                        Identifier = nameof(Amount),
                        ErrorMessage = $"{nameof(Amount)} is zero or below."
                    }
                });

            var accountSpec = new AccountByTgId(TgId);
            var account = await _repository.GetBySpecAsync(accountSpec);

            if (account == null)
            {
                return Result<Account>.Invalid(new()
                {
                    new ValidationError()
                    {
                        Identifier = nameof(account),
                        ErrorMessage = $"{nameof(account)} with TgId {TgId} does not exist."
                    }
                });
            }
            account.DepositPoints(Amount, eventsPublishStrategy);

            await _repository.UpdateAsync(account);

            return Result<Account>.Success(account);
        }
    }
}

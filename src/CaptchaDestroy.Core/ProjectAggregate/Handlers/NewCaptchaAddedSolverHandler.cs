using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Autofac;
using Autofac.Features.OwnedInstances;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Core.ProjectAggregate.Events;
using CaptchaDestroy.SharedKernel.Interfaces;
using MediatR;

namespace CaptchaDestroy.Core.ProjectAggregate.Handlers
{
    public class NewCaptchaAddedSolverHandler : INotificationHandler<NewCaptchaAddedEvent>
    {
        private readonly IRepository<Captcha> _repository;
        private readonly IVkCaptchaSolver _vkCaptchaSolver;
        private readonly ICaptchaService _captchaService;
        private readonly ILifetimeScope _lifetimeScope;
        public NewCaptchaAddedSolverHandler(
            IRepository<Captcha> repository,
            IVkCaptchaSolver vkCaptchaSolver,
            ILifetimeScope lifetimeScope)
        {
            _repository = repository;
            _vkCaptchaSolver = vkCaptchaSolver;
            _lifetimeScope = lifetimeScope;
        }

        public async Task Handle(NewCaptchaAddedEvent domainEvent, CancellationToken cancellationToken)
        {
            Guard.Against.Null(domainEvent, nameof(domainEvent));
            var captcha = domainEvent.Captcha;
            var solution = await _vkCaptchaSolver.SolveCaptcha(captcha.CaptchaUri);
            captcha.MarkSolved(solution);
            await _repository.UpdateAsync(captcha, cancellationToken);
            return;
        }
    }
}

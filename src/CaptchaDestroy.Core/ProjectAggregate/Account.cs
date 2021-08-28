using Ardalis.GuardClauses;
using CaptchaDestroy.Core.ProjectAggregate.Events;
using CaptchaDestroy.SharedKernel;
using CaptchaDestroy.SharedKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CaptchaDestroy.Core.ProjectAggregate
{
    public class Account : BaseEntity, IAggregateRoot
    {
        public long? TgId { get; private set; }
        private List<Captcha> _captchas = new();
        public IEnumerable<Captcha> Captchas => _captchas.AsReadOnly();
        public string SecretKey { get; private set; }
        public long Points { get; private set; } = 10000;

        private void GenerateNewSecretKey()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            SecretKey = new string(Enumerable.Repeat(chars, 16)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public Account()
        {
            GenerateNewSecretKey();
        }
        public Account(long tgId)
        {
            GenerateNewSecretKey();
            TgId = tgId;
        }

        public void AddCaptcha(Captcha newCaptcha, PublishStrategy? eventsPublishStrategy = null)
        {
            Guard.Against.Null(newCaptcha, nameof(newCaptcha));
            if (newCaptcha.Account != this)
                throw new ArgumentException(
                    "The account in the captcha instance does not match the accounts in which you are trying to add the captcha", nameof(newCaptcha));

            Points -= 100;
            _captchas.Add(newCaptcha);

            var newCaptchaAddedEvent = new NewCaptchaAddedEvent(this, newCaptcha, eventsPublishStrategy);
            Events.Add(newCaptchaAddedEvent);
        }
        public void DepositPoints(int Amount, PublishStrategy? eventsPublishStrategy = null)
        {
            Guard.Against.NegativeOrZero(Amount, nameof(Amount));
            
            Points += Amount;

            //var newCaptchaAddedEvent = new NewCaptchaAddedEvent(this, newCaptcha, eventsPublishStrategy);
            //Events.Add(newCaptchaAddedEvent);
        }

        public void UpdateTgId(long tgId)
        {
            TgId = Guard.Against.Null(tgId, nameof(tgId));
        }
    }
}

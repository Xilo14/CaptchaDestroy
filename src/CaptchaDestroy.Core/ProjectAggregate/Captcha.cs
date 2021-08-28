using System;
using Ardalis.GuardClauses;
using CaptchaDestroy.Core.ProjectAggregate.Events;
using CaptchaDestroy.SharedKernel;
using CaptchaDestroy.SharedKernel.Interfaces;

namespace CaptchaDestroy.Core.ProjectAggregate
{
    public class Captcha : BaseEntity, IAggregateRoot
    {
        public bool IsSolved { get; private set; } = false;
        public Uri CaptchaUri { get; private set; }
        public CaptchaType CaptchaType { get; private set; } = CaptchaType.Vk;
        public string SolutionString { get; private set; }

        public Account Account { get; private set; }

        private Captcha() { }
        public Captcha(Account account, Uri captchaUri)
        {
            Account = account;
            CaptchaUri = captchaUri;
        }
        public void MarkSolved(string solutionString)
        {
            Guard.Against.NullOrEmpty(solutionString, nameof(solutionString));

            IsSolved = true;

            SolutionString = solutionString;
            Events.Add(new CaptchaSolvedEvent(this));
        }

        public override string ToString()
        {
            string status = IsSolved ? "Solved!" : "Not solved.";
            return $"{Id}: Status: {status}";
        }
    }
}

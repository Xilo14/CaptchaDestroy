using Ardalis.Specification;
using CaptchaDestroy.Core.ProjectAggregate;

namespace CaptchaDestroy.Core.ProjectAggregate.Specifications
{
    public class AccountByTgIdWithCaptchas : Specification<Account>, ISingleResultSpecification
    {
        public AccountByTgIdWithCaptchas(long tgId)
        {
            Query
                .Where(acc => acc.TgId == tgId)
                .Include(acc => acc.Captchas);
        }
    }
}

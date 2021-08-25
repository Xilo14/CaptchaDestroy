using Ardalis.Specification;
using CaptchaDestroy.Core.ProjectAggregate;

namespace CaptchaDestroy.Core.ProjectAggregate.Specifications
{
    public class AccountByTgId : Specification<Account>, ISingleResultSpecification
    {
        public AccountByTgId(long tgId)
        {
            Query
                .Where(acc => acc.TgId == tgId);
        }
    }
}

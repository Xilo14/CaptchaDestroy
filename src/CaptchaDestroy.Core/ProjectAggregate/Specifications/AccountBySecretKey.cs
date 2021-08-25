using Ardalis.Specification;
using CaptchaDestroy.Core.ProjectAggregate;

namespace CaptchaDestroy.Core.ProjectAggregate.Specifications
{
    public class AccountBySecretKey : Specification<Account>, ISingleResultSpecification
    {
        public AccountBySecretKey(string secretKey)
        {
            Query
                .Where(acc => acc.SecretKey == secretKey);
        }
    }
}

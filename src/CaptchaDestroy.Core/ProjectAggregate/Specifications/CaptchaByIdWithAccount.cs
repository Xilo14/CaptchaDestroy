using Ardalis.Specification;
using CaptchaDestroy.Core.ProjectAggregate;

namespace CaptchaDestroy.Core.ProjectAggregate.Specifications
{
    public class CaptchaByIdWithAccount : Specification<Captcha>, ISingleResultSpecification
    {
        public CaptchaByIdWithAccount(int id)
        {
            Query
                .Where(c => c.Id == id)
                .Include(c => c.Account);
        }
    }
}

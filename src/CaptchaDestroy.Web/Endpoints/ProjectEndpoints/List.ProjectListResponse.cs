using System.Collections.Generic;
using CaptchaDestroy.Core.ProjectAggregate;

namespace CaptchaDestroy.Web.Endpoints.ProjectEndpoints
{
    public class ProjectListResponse
    {
        public List<ProjectRecord> Projects { get; set; } = new();
    }
}

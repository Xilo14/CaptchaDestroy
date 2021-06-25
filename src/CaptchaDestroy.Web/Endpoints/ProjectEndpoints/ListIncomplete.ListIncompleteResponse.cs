using System.Collections.Generic;

namespace CaptchaDestroy.Web.Endpoints.ProjectEndpoints
{
    public class ListIncompleteResponse
    {
        public int ProjectId { get; set; }
        public List<ToDoItemRecord> IncompleteItems { get; set; }
    }
}

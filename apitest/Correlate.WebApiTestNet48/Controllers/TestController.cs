
using System.Web.Http;

namespace Correlate.WebApiTestNet48.Controllers;

public class TestController : ApiController
{
    [HttpGet]
    public IHttpActionResult Index()
    {
        return Json(new { Message = "hello there"});
    }
}

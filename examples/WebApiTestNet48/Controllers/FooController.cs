using System.Web.Http;

namespace WebApiTestNet48.Controllers;

public class FooController : ApiController
{
    [HttpGet]
    public IHttpActionResult Index()
    {
        return Ok(new { Message = "foo" });
    }
}

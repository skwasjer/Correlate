using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Services.Description;

namespace WebApiTestNet48.Controllers;

public class FooController : ApiController
{
    [HttpGet]
    public IHttpActionResult Index()
    {
        return Ok(new { Message = "foo"});
    }
}

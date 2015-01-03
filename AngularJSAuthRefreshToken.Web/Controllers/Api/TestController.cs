using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthRefreshToken.Web.Controllers.Api
{
    public class BindingObject
    {
        public string Id { get; set; }
    }
    [Authorize]
    public class TestController : ApiController
    {
        public IEnumerable<string> Get(string id)
        {
            var isa = this.User.IsInRole("Admin");
            return new[] { "test1", "test2", "test3", "test4", id };
        }

        public IEnumerable<string> Post(BindingObject id)
        {
            var isa = this.User.IsInRole("Admin");
            return new[] { "test1", "test2", "test3", "test4", id.Id };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    public class RefreshToken
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ClientId { get; set; }
        public DateTimeOffset IssuedUtc { get; set; }
        public DateTimeOffset ExpiresUtc { get; set; }
    }
}

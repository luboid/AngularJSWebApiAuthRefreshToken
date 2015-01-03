using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Data.Poco
{
    public class ExternalLoginProvider
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    public class RefreshTokenClient
    {
        public string Id { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public bool Active { get; set; }
        public int TokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }
        public string Secret { get; set; }
        public string Description { get; set; }
    }
}

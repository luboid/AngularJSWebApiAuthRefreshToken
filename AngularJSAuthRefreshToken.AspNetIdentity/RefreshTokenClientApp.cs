using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthRefreshToken.AspNetIdentity
{
    public class RefreshTokenClientApp
    {
        public string Id { get; set; }
        public RefreshTokenClientAppType ApplicationType { get; set; }
        public bool Active { get; set; }
        public int TokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }
        public string Secret { get; set; }
        public string Description { get; set; }
    }
}

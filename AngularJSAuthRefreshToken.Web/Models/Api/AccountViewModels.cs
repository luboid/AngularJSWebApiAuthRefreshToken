using System;
using System.Collections.Generic;

namespace AngularJSAuthRefreshToken.Web.Models.Api
{
    // Models returned by AccountController actions.

    public class ExternalLoginViewModel
    {
        public string Provider { get; set; }
        public string Caption { get; set; }
        public string Url { get; set; }
        public string State { get; set; }
    }

    public class ProfileViewModel
    {
        public IEnumerable<UserLoginProvidersViewModel> Logins { get; set; }
    }

    public class UserLoginProvidersViewModel
    {
        public string Provider { get; set; }
        public string Key { get; set; }
    }
}

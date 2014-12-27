using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    public class IdentityRole : IRole<string>
	{
        public string Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}
		
        public IdentityRole()
		{ }
    }
}

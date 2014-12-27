using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;

namespace AngularJSAuthRefreshToken.Web.AspNetIdentity
{
    public class IdentityUser : IUser<string>
    {
        public IdentityUser()
        { }

        public string Id
        {
            get;
            set;
        }

		public string Email
		{
			get;
			set;
		}
		
        public bool EmailConfirmed
		{
			get;
			set;
		}
		
        public string PasswordHash
		{
			get;
			set;
		}
		
        public string SecurityStamp
		{
			get;
			set;
		}
		
        public string PhoneNumber
		{
			get;
			set;
		}
		
        public bool PhoneNumberConfirmed
		{
			get;
			set;
		}

		public bool TwoFactorEnabled
		{
			get;
			set;
		}

        public DateTime? LockoutEndDateUtc
        {
            get;
            set;
        }

        public bool LockoutEnabled
        {
            get;
            set;
        }

        public int AccessFailedCount
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string PushRegistrationId
        {
            get;
            set;
        }

        public bool Confirmed
        {
            get;
            set;
        }
    }
}

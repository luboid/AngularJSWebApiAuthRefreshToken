using AngularJSAuthRefreshToken.Data.Poco;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;

namespace AngularJSAuthRefreshToken.AspNetIdentity.DTO
{
    public class IdentityUser
    {
        public IdentityUser()
        { }

        public string Id
        {
            get;
            set;
        }

        public string UserName
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
    }
}

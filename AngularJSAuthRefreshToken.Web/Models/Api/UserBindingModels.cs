using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using FluentValidation;
using Microsoft.AspNet.Identity;
using AngularJSAuthRefreshToken.AspNetIdentity;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace AngularJSAuthRefreshToken.Web.Models.Api
{
    // Models used as parameters to UserController actions.

    public class RoleBindingModel
    {
        public string Name { get; set; }
    }
    
    public class RoleBindingModelValidator : AbstractValidator<RoleBindingModel>
    {
        public RoleBindingModelValidator()
        {
            RuleFor(m => m.Name).NotEmpty();
        }
    }
}

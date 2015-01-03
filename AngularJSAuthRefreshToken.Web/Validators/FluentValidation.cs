using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace AngularJSAuthRefreshToken.Web.Validators
{
    public static class FluentValidation
    {
        public static IRuleBuilderOptions<T, TElement> Guid<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder, string format, Expression<Func<string>> errorMessageResourceSelector)
        {
            return ruleBuilder.SetValidator(new GuidValidator(format, errorMessageResourceSelector));
        }
        
        public static IRuleBuilderOptions<T, TElement> Guid<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new GuidValidator());
        }

    }
}
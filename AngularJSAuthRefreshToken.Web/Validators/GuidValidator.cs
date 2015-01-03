using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace AngularJSAuthRefreshToken.Web.Validators
{
    public class GuidValidator : PropertyValidator
    {
        string format;

        public GuidValidator(string format = "D")
            : this(format, () => Resources.FluentValidation.InvalidGuid)
		{ 

        }

        public GuidValidator(string format, Expression<Func<string>> errorMessageResourceSelector)
            : base(errorMessageResourceSelector)
		{
            this.format = format ?? "D";
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            string val; Guid g;
            if (context.PropertyValue != null)
            {
                if (context.PropertyValue is string)
                {
                    val = context.PropertyValue as string;
                    if (!string.IsNullOrWhiteSpace(val) && !Guid.TryParseExact(val, format, out g))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AngularJSAuthRefreshToken.Web.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Common {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Common() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AngularJSAuthRefreshToken.Web.Resources.Common", typeof(Common).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t edit you self..
        /// </summary>
        public static string CantEditYouSelf {
            get {
                return ResourceManager.GetString("CantEditYouSelf", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirm you email if used app {0} by this &lt;a href=&quot;{1}&quot;&gt;link&lt;/a&gt;..
        /// </summary>
        public static string EmailConfirmBody {
            get {
                return ResourceManager.GetString("EmailConfirmBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirm you email if used app {0}.
        /// </summary>
        public static string EmailConfirmSubject {
            get {
                return ResourceManager.GetString("EmailConfirmSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Click this &lt;a href=&quot;{1}&quot;&gt;link&lt;/a&gt; to change yours password..
        /// </summary>
        public static string EmailPasswordBody {
            get {
                return ResourceManager.GetString("EmailPasswordBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request to change password {0}.
        /// </summary>
        public static string EmailPasswordSubject {
            get {
                return ResourceManager.GetString("EmailPasswordSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The external login is already associated with an account..
        /// </summary>
        public static string ExternalLoginAlreadyAssociated {
            get {
                return ResourceManager.GetString("ExternalLoginAlreadyAssociated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid password or name..
        /// </summary>
        public static string InvalidGrant {
            get {
                return ResourceManager.GetString("InvalidGrant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown user..
        /// </summary>
        public static string UnknownUser {
            get {
                return ResourceManager.GetString("UnknownUser", resourceCulture);
            }
        }
    }
}

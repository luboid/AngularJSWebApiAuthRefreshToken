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
    internal class Common {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Common() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirm you email if used app {0} by this &lt;a href=&quot;{1}&quot;&gt;link&lt;/a&gt;..
        /// </summary>
        internal static string EmailConfirmBody {
            get {
                return ResourceManager.GetString("EmailConfirmBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirm you email if used app {0}.
        /// </summary>
        internal static string EmailConfirmSubject {
            get {
                return ResourceManager.GetString("EmailConfirmSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Click this &lt;a href=&quot;{1}&quot;&gt;link&lt;/a&gt; to change yours password..
        /// </summary>
        internal static string EmailPasswordBody {
            get {
                return ResourceManager.GetString("EmailPasswordBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request to change password {0}.
        /// </summary>
        internal static string EmailPasswordSubject {
            get {
                return ResourceManager.GetString("EmailPasswordSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid password or name..
        /// </summary>
        internal static string InvalidGrant {
            get {
                return ResourceManager.GetString("InvalidGrant", resourceCulture);
            }
        }
    }
}

﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.VisualStudio.TestPlatform.CoreUtilities {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal Resources() {
        }
        
        /// <summary>
        ///    Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.TestPlatform.CoreUtilities.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    Overrides the current thread's CurrentUICulture property for all
        ///    resource lookups using this strongly typed resource class.
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
        ///    Looks up a localized string similar to The parameter cannot be null or empty..
        /// </summary>
        public static string CannotBeNullOrEmpty {
            get {
                return ResourceManager.GetString("CannotBeNullOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Error: {0}.
        /// </summary>
        public static string CommandLineError {
            get {
                return ResourceManager.GetString("CommandLineError", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Information: {0}.
        /// </summary>
        public static string CommandLineInformational {
            get {
                return ResourceManager.GetString("CommandLineInformational", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Warning: {0}.
        /// </summary>
        public static string CommandLineWarning {
            get {
                return ResourceManager.GetString("CommandLineWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Unhandled exception occurred while processing a job from the &apos;{0}&apos; queue: {1}.
        /// </summary>
        public static string ExceptionFromJobProcessor {
            get {
                return ResourceManager.GetString("ExceptionFromJobProcessor", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The {0} queue has already been disposed..
        /// </summary>
        public static string QueueAlreadyDisposed {
            get {
                return ResourceManager.GetString("QueueAlreadyDisposed", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The {0} queue cannot be disposed while paused..
        /// </summary>
        public static string QueuePausedDisposeError {
            get {
                return ResourceManager.GetString("QueuePausedDisposeError", resourceCulture);
            }
        }

        /// <summary>
        ///    Looks up a localized string similar to Error getting process name..
        /// </summary>
        public static string Utility_ProcessNameWhenCannotGetIt {
            get {
                return ResourceManager.GetString("Utility_ProcessNameWhenCannotGetIt", resourceCulture);
            }
        }
    }
}

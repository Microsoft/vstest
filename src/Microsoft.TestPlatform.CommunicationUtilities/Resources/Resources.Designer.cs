﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources {
    using System;
    using System.Reflection;
    
    
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources.Resources", typeof(Resources).GetTypeInfo().Assembly);
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
        ///   Looks up a localized string similar to The active Test Discovery was aborted. Due to {0}.
        /// </summary>
        internal static string AbortedTestDiscovery {
            get {
                return ResourceManager.GetString("AbortedTestDiscovery", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The active Test Run was aborted. Due to {0}.
        /// </summary>
        internal static string AbortedTestRun {
            get {
                return ResourceManager.GetString("AbortedTestRun", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An existing connection was forcibly closed by the remote host..
        /// </summary>
        internal static string ConnectionClosed {
            get {
                return ResourceManager.GetString("ConnectionClosed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to communicate with test execution process..
        /// </summary>
        internal static string UnableToCommunicateToTestHost {
            get {
                return ResourceManager.GetString("UnableToCommunicateToTestHost", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to communicate with test execution process..
        /// </summary>
        internal static string DataCollectorUriForLogMessage
        {
            get
            {
                return ResourceManager.GetString("DataCollectorUriForLogMessage", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Unexpected message received. Expected MessageType : {0} Actual MessageType: {1}.
        /// </summary>
        internal static string UnexpectedMessage
        {
            get
            {
                return ResourceManager.GetString("UnexpectedMessage", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Protocol version check failed. Make sure test runner and host are compatible..
        /// </summary>
        internal static string VersionCheckFailed
        {
            get
            {
                return ResourceManager.GetString("VersionCheckFailed", resourceCulture);
            }
        }
    }
}

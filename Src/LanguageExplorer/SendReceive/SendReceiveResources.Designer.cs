﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LanguageExplorer.SendReceive {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SendReceiveResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SendReceiveResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LanguageExplorer.SendReceive.SendReceiveResources", typeof(SendReceiveResources).Assembly);
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
        ///   Looks up a localized string similar to There is already a copy of FLExBridge running.
        ///You probably have a Conflict Report open. It will need to be closed before you can access any of the other FLExBridge functions such as:
        ///-- Send/Receive Project
        ///-- Get project from a colleague
        ///-- View Conflict Report (can&apos;t have two open)
        ///If you don&apos;t see any FLExBridge windows open, it may just be that FLExBridge is still shutting down. Please wait 15 seconds and try again..
        /// </summary>
        internal static string kBridgeAlreadyRunning {
            get {
                return ResourceManager.GetString("kBridgeAlreadyRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to FLExBridge.
        /// </summary>
        internal static string kFlexBridge {
            get {
                return ResourceManager.GetString("kFlexBridge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Creating a project to import LIFT data into.
        /// </summary>
        internal static string ksCreatingLiftProject {
            get {
                return ResourceManager.GetString("ksCreatingLiftProject", resourceCulture);
            }
        }
    }
}

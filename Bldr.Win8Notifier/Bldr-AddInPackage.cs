using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

using System.Windows.Forms;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using Microsoft.VisualStudio.Shell;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace Bldr.Win8Notifier
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidBldr_AddInPkgString)]
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82" /*VSConstants.UICONTEXT_SolutionExists*/)]
    public sealed class Bldr_AddInPackage : Package, IVsSolutionEvents, IVsUpdateSolutionEvents
    {
        private uint solutionEventsCookie;
        private uint updateSolutionEventsCookie;

        private IVsSolution2 solution = null;
        private IVsSolutionBuildManager2 sbm = null;


        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public Bldr_AddInPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Unadvise all events
            if (sbm != null && updateSolutionEventsCookie != 0)
                sbm.UnadviseUpdateSolutionEvents(updateSolutionEventsCookie);

            if (solution != null && solutionEventsCookie != 0)
                solution.UnadviseSolutionEvents(solutionEventsCookie);
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Get solution
            solution = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution)) as IVsSolution2;
            if (solution != null)
            {
                // Get count of any currently loaded projects
                object count;
                solution.GetProperty((int)__VSPROPID.VSPROPID_ProjectCount, out count);

                // Register for solution events
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }

            // Get solution build manager
            sbm = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            if (sbm != null)
            {
                sbm.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);
            }
        }
        
        #endregion

        #region IVsSolutionEvents

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsUpdateSolutionEvents

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            return VSConstants.S_OK;
        }


        private readonly string toastXML = @"<toast><visual><binding template='ToastText01'><text id='1'>{0}</text></binding></visual></toast>";
        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            var success = Convert.ToBoolean(fSucceeded);

            var msg = success ? "Build complete." : "Build failed.";

            try
            {
                // This is a dogshow, I have no idea why this doesn't work. Bizarre references to assemblies that don't exist.

                //MessageBox.Show(msg);
                // Get a toast XML template
                //var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                var toastXml = new XmlDocument();
                toastXml.LoadXml(string.Format(toastXML, msg));

                //// Fill in the text elements
                //var text = toastXml.GetElementsByTagName("text").Item(0);
                //text.AppendChild(toastXml.CreateTextNode(msg));

                //// Create the toast and attach event listeners
                ToastNotification toast = new ToastNotification(toastXml);

                // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
                var toastManager = ToastNotificationManager.CreateToastNotifier("VisualStudio.11.0");
                toastManager.Show(toast);
            }
            catch (Exception ex)
            {
                // Something horrible has happened. Rather just die than screwing with VS any further.
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "An exception has occurred: {0}", ex.Message));
            }
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }


        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}

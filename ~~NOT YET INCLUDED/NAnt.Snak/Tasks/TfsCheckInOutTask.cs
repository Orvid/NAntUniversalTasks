using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core.Attributes;
using NAnt.Core;
using NAnt.Core.Types;
using System.IO;
using Microsoft.TeamFoundation.VersionControl.Client;
using Snak.Types;

namespace Snak.Tasks
{
    [TaskName("tfsCheckInOut")]
    public class TfsCheckInOutTask : AbstractTFSTask
    {
        private RecursionType _recursionTypeToUse = RecursionType.Full;

        private FileSet _filesToCheckInOut;

        [BuildElement("filesToCheckInOut", Required = true)]
        public FileSet FilesToCheckInOut
        {
            get { return _filesToCheckInOut; }
            set { _filesToCheckInOut = value; }
        }

        private string _checkInComment = String.Empty;

        [TaskAttribute("checkInComment", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string CheckInComment
        {
            get { return _checkInComment; }
            set { _checkInComment = value; }
        }

        private string _propertyName = String.Empty;

        [TaskAttribute("property", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        private TfsPolicyOverride _tfsPolicyOverride;

        [BuildElement("tfsPolicyOverride", Required = false)]
        public TfsPolicyOverride TfsPolicyOverride
        {
            get { return _tfsPolicyOverride; }
            set { _tfsPolicyOverride = value; }
        }
        
        private TaskContainer _taskContainer = null;

        [BuildElement("taskContainer")]
        public TaskContainer TaskContainer
        {
            get { return _taskContainer; }
            set { _taskContainer = value; }
        }

        protected override void ExecuteTask()
        {
            Workspace workspace = this.GetCurrentWorkspace();
            string[] filesToCheckInOut = new string[this.FilesToCheckInOut.FileNames.Count];
            this.FilesToCheckInOut.FileNames.CopyTo(filesToCheckInOut, 0);

            CheckOut(workspace, filesToCheckInOut);

            try
            {
                Log(Level.Verbose, "About to call tasks within tfsCheckInOut's taskContainer.");
                if (_taskContainer != null)
                    _taskContainer.Execute();
            }
            catch
            {
                try
                {
                    Log(Level.Info, "An error occurred within a task inside tfsCheckInOut taskContainer, about to undo checkouts previously made");
                    UndoCheckout(workspace, filesToCheckInOut);
                }
                catch (Exception ex)
                {
                    Log(Level.Error, "An error occurred while performing actions within the " + this.GetType().Name + " task, an attempt to undo checkouts on files '" + String.Join(", ", filesToCheckInOut) + "'  failed. The exception will be logged and swallowed so the original exception can bubble up. The undo exception was: " + ex.ToString());
                }

                throw;
            }

            CheckIn(workspace, filesToCheckInOut);
        }

        private void CheckOut(Workspace workspace, string[] items)
        {
            if (items.Length > 0)
            {
                Log(Level.Info, "Checking out files '" + String.Join(",", items) + "'.");

                int numberOfFilesCheckedOut = workspace.PendEdit(items, _recursionTypeToUse);

                if (numberOfFilesCheckedOut <= 0)
                    throw new ApplicationException("Failed to checkout the following items: '" + String.Join(", ", items) + "'");

                Log(Level.Info, "Checked out " + numberOfFilesCheckedOut.ToString() + " file/s.");
            }
        }

        private void UndoCheckout(Workspace workspace, string[] items)
        {
            if (items.Length > 0)
            {
                Log(Level.Info, "Undoing checkout of files '" + String.Join(",", items) + "'.");

                int numberOfUndoneChanges = workspace.Undo(items, _recursionTypeToUse);

                if (numberOfUndoneChanges <= 0)
                    throw new ApplicationException("Failed to undo the following items: '" + String.Join(", ", items) + "'");

                Log(Level.Info, "Undone checkouts on " + numberOfUndoneChanges.ToString() + " file/s.");
            }
        }

        private void CheckIn(Workspace workspace, string[] items)
        {
            if (items.Length > 0)
            {
                PendingChange[] pendingChanges = workspace.GetPendingChanges(items, _recursionTypeToUse);

                if (pendingChanges.Length > 0)
                {
                    Log(Level.Info, "Checking in files '" + String.Join(",", items) + "'.");

                    PolicyOverrideInfo policyOverrideInfo = null;

                    if (_tfsPolicyOverride != null && _tfsPolicyOverride.If)
                        policyOverrideInfo = _tfsPolicyOverride.CreatePolicyOverrideInfo();

                    // NOTE: even when you have pendingChanges.Length > 0, you may not have actually modified 
                    // the files, if so this call to CheckIn automagically undoes the checkouts for those items... 
                    // That got me for a while, I also tried workspace.EvaluateCheckin() to try to pre-empt check in errors
                    // but this seems to not report on pending changes that are actually unmodified
                    int changeSetNumber = workspace.CheckIn(pendingChanges, this.CheckInComment, null, null, policyOverrideInfo);

                    if (changeSetNumber <= 0)
                    {
                        Log(Level.Info, "Failed to check in the following items: '" + String.Join(", ", items) + "'. This often happens where there are no modifications on those items. If so the call to TFS automagically undoes the check outs.");
                    }
                    else
                        Log(Level.Info, "Checkin successful, changeset number was: " + changeSetNumber.ToString());
                }
                else
                    Log(Level.Info, "No pending changes found to checkin.");
            }
        }
    }
}

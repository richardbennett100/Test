﻿//-----------------------------------------------------------------------
// <copyright file="Iis6AppPool.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Web
{
    using System;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Security.Permissions;
    using Microsoft.Build.Framework;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Create</i> (<b>Required: </b> Name <b>Optional:</b> Properties)</para>
    /// <para><i>CheckExists</i> (<b>Required: </b> Name <b>Output: </b>Exists)</para>
    /// <para><i>Delete</i> (<b>Required: </b> Name)</para>
    /// <para><i>Modify</i> (<b>Required: </b> Name, Properties)</para>
    /// <para><i>Recycle</i> (<b>Required: </b> Name)</para>
    /// <para><i>Start</i> (<b>Required: </b> Name)</para>
    /// <para><i>Stop</i> (<b>Required: </b> Name)</para>
    /// <para><b>Remote Execution Support:</b> Yes</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Project ToolsVersion="3.5" DefaultTargets="Default" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///     <PropertyGroup>
    ///         <TPath>$(MSBuildProjectDirectory)\..\MSBuild.ExtensionPack.tasks</TPath>
    ///         <TPath Condition="Exists('$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks')">$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks</TPath>
    ///     </PropertyGroup>
    ///     <Import Project="$(TPath)"/>
    ///     <Target Name="Default">
    ///         <!-- Delete an AppPool -->
    ///         <MSBuild.ExtensionPack.Web.Iis6AppPool TaskAction="Delete" Name="AnAppPool"/>
    ///         <!-- Create an AppPool -->
    ///         <MSBuild.ExtensionPack.Web.Iis6AppPool TaskAction="Create" Name="AnAppPool" Properties="AppPoolAutoStart=TRUE;PeriodicRestartTime=0;PeriodicRestartRequests=0;PeriodicRestartMemory=0;PeriodicRestartPrivateMemory=0;PeriodicRestartSchedule=04:00;IdleTimeout=0;AppPoolQueueLength=2000;CPULimit=0;CPUResetInterval=5;CPUAction=0;MaxProcesses=1;PingingEnabled=TRUE;PingInterval=60;PingResponseTime=90;RapidFailProtection=FALSE;RapidFailProtectionMaxCrashes=5;RapidFailProtectionInterval=5;StartupTimeLimit=60;ShutdownTimeLimit=60;AppPoolIdentityType=3;"/>
    ///         <!-- Modify an AppPool -->
    ///         <MSBuild.ExtensionPack.Web.Iis6AppPool TaskAction="Modify" Name="AnAppPool" Properties="AppPoolAutoStart=TRUE;PeriodicRestartTime=0;PeriodicRestartRequests=0;PeriodicRestartMemory=0;PeriodicRestartPrivateMemory=0;PeriodicRestartSchedule=04:00;IdleTimeout=0;AppPoolQueueLength=1952;CPULimit=0;CPUResetInterval=5;CPUAction=0;MaxProcesses=6;PingingEnabled=TRUE;PingInterval=60;PingResponseTime=90;RapidFailProtection=FALSE;RapidFailProtectionMaxCrashes=5;RapidFailProtectionInterval=5;StartupTimeLimit=60;ShutdownTimeLimit=60;AppPoolIdentityType=3;"/>
    ///         <!-- Check whether an AppPool exists -->
    ///         <MSBuild.ExtensionPack.Web.Iis6AppPool TaskAction="CheckExists" Name="AnAppPool">
    ///             <Output PropertyName="DoesExist" TaskParameter="Exists"/>
    ///         </MSBuild.ExtensionPack.Web.Iis6AppPool>
    ///         <Message Text="AnAppPool exists: $(DoesExist)"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.4.0/html/7e174b6e-9b42-5fe3-728b-cf4049753fba.htm")]
    public class Iis6AppPool : BaseTask
    {
        private const string CreateTaskAction = "Create";
        private const string CheckExistsTaskAction = "CheckExists";
        private const string DeleteTaskAction = "Delete";
        private const string ModifyTaskAction = "Modify";
        private const string StartTaskAction = "Start";
        private const string StopTaskAction = "Stop";
        private const string RecycleTaskAction = "Recycle";
        
        private string properties;

        [DropdownValue(CreateTaskAction)]
        [DropdownValue(CheckExistsTaskAction)]
        [DropdownValue(DeleteTaskAction)]
        [DropdownValue(ModifyTaskAction)]
        [DropdownValue(RecycleTaskAction)]
        [DropdownValue(StartTaskAction)]
        [DropdownValue(StopTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }
        
        /// <summary>
        /// Sets the Properties. Use a semi-colon delimiter. See <a href="http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/cde669f1-5714-4159-af95-f334251c8cbd.mspx?mfr=true">Metabase Property Reference (IIS 6.0)</a>
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public string Properties
        {
            get { return System.Web.HttpUtility.HtmlDecode(this.properties); }
            set { this.properties = value; }
        }

        /// <summary>
        /// Sets the name of the AppPool. Required.
        /// </summary>
        [Required]
        [TaskAction(CreateTaskAction, true)]
        [TaskAction(CheckExistsTaskAction, true)]
        [TaskAction(DeleteTaskAction, true)]
        [TaskAction(ModifyTaskAction, true)]
        [TaskAction(RecycleTaskAction, true)]
        [TaskAction(StartTaskAction, true)]
        [TaskAction(StopTaskAction, true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets whether the app pool exists. Output
        /// </summary>
        [Output]
        [TaskAction(CheckExistsTaskAction, false)]
        public bool Exists { get; set; }

        internal string IisPath
        {
            get { return "IIS://" + this.MachineName + "/W3SVC"; }
        }

        internal string AppPoolsPath
        {
            get { return "IIS://" + this.MachineName + "/W3SVC/AppPools"; }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.TaskAction)
            {
                case CreateTaskAction:
                    this.Create();
                    break;
                case ModifyTaskAction:
                    this.Modify();
                    break;
                case StartTaskAction:
                case DeleteTaskAction:
                case StopTaskAction:
                case RecycleTaskAction:
                    this.ControlAppPool(this.TaskAction);
                    break;
                case "CheckExists":
                    this.Exists = this.CheckExists();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void UpdateMetaBaseProperty(DirectoryEntry entry, string metaBasePropertyName, string metaBaseProperty)
        {
            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Applying Property: {0}({1})", metaBasePropertyName, metaBaseProperty));

            if (metaBaseProperty.IndexOf('|') == -1)
            {
                entry.Properties[metaBasePropertyName].Value = metaBaseProperty;
            }
            else
            {
                entry.Properties[metaBasePropertyName].Value = string.Empty;
                string[] metabaseProperties = metaBaseProperty.Split('|');
                foreach (string metabasePropertySplit in metabaseProperties)
                {
                    entry.Properties[metaBasePropertyName].Add(metabasePropertySplit);
                }

                entry.CommitChanges();
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void Modify()
        {
            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Modifying AppPool: {0}", this.Name));

            // We'll try and find the app pool first.
            using (DirectoryEntry appPoolEntry = this.LoadAppPool(this.Name))
            {
                if (appPoolEntry == null)
                {
                    this.Log.LogError(string.Format(CultureInfo.InvariantCulture, "The AppPool does not exist: {0}", this.Name));
                    return;
                }

                if (string.IsNullOrEmpty(this.Properties) == false)
                {
                    string[] propList = this.Properties.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string s in propList)
                    {
                        string[] propPair = s.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        string propValue = propPair.Length > 1 ? propPair[1] : string.Empty;
                        this.UpdateMetaBaseProperty(appPoolEntry, propPair[0], propValue);
                    }

                    appPoolEntry.CommitChanges();
                }
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private bool CheckExists()
        {
            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Checking AppPool Exists: {0}", this.Name));
            return DirectoryEntry.Exists(this.AppPoolsPath + @"/" + this.Name);
        }

        private DirectoryEntry LoadAppPools()
        {
            string poolsPath = string.Format(CultureInfo.InvariantCulture, "{0}/AppPools", this.IisPath);
            DirectoryEntry appPools = new DirectoryEntry(poolsPath);
            if (appPools == null)
            {
                throw new ApplicationException(string.Format(CultureInfo.CurrentUICulture, "IIS DirectoryServices unavailable: {0}", poolsPath));
            }

            return appPools;
        }

        private DirectoryEntry LoadAppPool(string appPoolName)
        {
            using (DirectoryEntry appPoolsEntry = this.LoadAppPools())
            {
                DirectoryEntries appPools = appPoolsEntry.Children;

                foreach (DirectoryEntry appPool in appPools)
                {
                    if (appPool.SchemaClassName == "IIsApplicationPool")
                    {
                        if (string.Compare(appPoolName, appPool.Name, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            return appPool;
                        }
                    }

                    appPool.Dispose();
                }

                return null;
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void ControlAppPool(string appPoolAction)
        {
            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "AppPool: {0} - Action: {1}", this.Name, appPoolAction));

            // First locate the app pool.
            using (DirectoryEntry appPoolEntry = this.LoadAppPool(this.Name))
            {
                if (appPoolEntry != null)
                {
                    switch (appPoolAction)
                    {
                        case DeleteTaskAction:
                            using (DirectoryEntry appPoolsEntry = this.LoadAppPools())
                            {
                                appPoolsEntry.Invoke("Delete", "IIsApplicationPool", appPoolEntry.Name);
                            }

                            break;
                        case StopTaskAction:
                            appPoolEntry.Invoke("Stop");
                            break;
                        case StartTaskAction:
                            appPoolEntry.Invoke("Start");
                            break;
                        case RecycleTaskAction:
                            appPoolEntry.Invoke("Start");
                            appPoolEntry.Invoke("Recycle", null);
                            break;
                    }
                }
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void Create()
        {
            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Creating AppPool: {0}", this.Name));

            // We'll try and find the app pool first.
            using (DirectoryEntry appPoolEntry = this.LoadAppPool(this.Name))
            {
                if (appPoolEntry != null)
                {
                    this.Log.LogError(string.Format(CultureInfo.InvariantCulture, "The AppPool already exists: {0}", this.Name));
                    return;
                }
            }

            using (DirectoryEntry appPoolsEntry = new DirectoryEntry(this.AppPoolsPath))
            {
                using (DirectoryEntry appPoolEntry = appPoolsEntry.Children.Add(this.Name, "IIsApplicationPool"))
                {
                    if (string.IsNullOrEmpty(this.Properties) == false)
                    {
                        string[] propList = this.Properties.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string s in propList)
                        {
                            string[] propPair = s.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            string propValue = propPair.Length > 1 ? propPair[1] : string.Empty;
                            this.UpdateMetaBaseProperty(appPoolEntry, propPair[0], propValue);
                        }

                        appPoolEntry.CommitChanges();
                    }
                }
            }
        }
    }
}
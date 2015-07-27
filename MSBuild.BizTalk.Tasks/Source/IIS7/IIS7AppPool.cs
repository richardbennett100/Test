﻿//-----------------------------------------------------------------------
// <copyright file="Iis7AppPool.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Web
{
    using System;
    using System.Globalization;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.Web.Administration;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>CheckExists</i> (<b>Required: </b> Name <b>Output:</b> Exists)</para>
    /// <para><i>Create</i> (<b>Required: </b> Name <b>Optional: </b>Force, IdentityType, PoolIdentity, IdentityPassword, ManagedRuntimeVersion, AutoStart, Enable32BitAppOnWin64, PipelineMode, QueueLength, IdleTimeout, PeriodicRestartPrivateMemory, PeriodicRestartTime, MaxProcesses, RecycleRequests, RecycleInterval, RecycleTimes)</para>
    /// <para><i>Delete</i> (<b>Required: </b> Name)</para>
    /// <para><i>GetInfo</i> (<b>Required: </b> Name)</para>
    /// <para><i>Modify</i> (<b>Required: </b> Name <b>Optional: </b>Force, ManagedRuntimeVersion, AutoStart, Enable32BitAppOnWin64, QueueLength, IdleTimeout, PeriodicRestartPrivateMemory, PeriodicRestartTime, MaxProcesses, RecycleRequests, RecycleInterval, RecycleTimes)</para>
    /// <para><i>Recycle</i> (<b>Required: </b> Name)</para>
    /// <para><i>SetIdentity</i> (<b>Optional: </b>IdentityType, PoolIdentity, IdentityPassword)</para>
    /// <para><i>SetPipelineMode</i> (<b>Optional: </b>  PipelineMode)</para>
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
    ///         <!-- Create an AppPool -->
    ///         <MSBuild.ExtensionPack.Web.Iis7AppPool TaskAction="Create" Name="NewAppPool100" RecycleRequests="45" RecycleInterval="1987" Force="true" MaxProcesses="5"/>
    ///         <!-- Modify an AppPool -->
    ///         <MSBuild.ExtensionPack.Web.Iis7AppPool TaskAction="Modify" Name="NewAppPool100" RecycleRequests="-1" RecycleInterval="-1"/>
    ///         <MSBuild.ExtensionPack.Web.Iis7AppPool TaskAction="Create" Name="NewAppPool200" Force="true"  MaxProcesses="4" IdentityType="SpecificUser" PoolIdentity="MiniMe" IdentityPassword="MiniPass" QueueLength="400" IdleTimeOut="5000"/>
    ///         <MSBuild.ExtensionPack.Web.Iis7AppPool TaskAction="Modify" Name="NewAppPool200" RecycleRequests="222" RecycleInterval="443" RecycleTimes="07:33,08:44,17:54" MaxProcesses="3"  QueueLength="598"/>
    ///         <!-- Set the PipelineMode in the AppPool -->
    ///         <MSBuild.ExtensionPack.Web.Iis7AppPool TaskAction="SetPipelineMode" Name="NewAppPool200" PipelineMode="Classic"/>
    ///         <MSBuild.ExtensionPack.Web.Iis7AppPool TaskAction="SetPipelineMode" Name="NewAppPool200" PipelineMode="Integrated"/>
    ///         <!-- Set the Identity for the AppPool -->
    ///         <MSBuild.ExtensionPack.Web.Iis7AppPool TaskAction="SetIdentity" Name="NewAppPool200" IdentityType="LocalService"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>  
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.4.0/html/628dad3f-8d9e-7287-53f0-d96dbf2be0e6.htm")]
    public class Iis7AppPool : BaseTask
    {
        private const string CheckExistsTaskAction = "CheckExists";
        private const string CreateTaskAction = "Create";
        private const string DeleteTaskAction = "Delete";
        private const string GetInfoTaskAction = "GetInfo";
        private const string ModifyTaskAction = "Modify";
        private const string SetIdentityTaskAction = "SetIdentity";
        private const string SetPipelineModeTaskAction = "SetPipelineMode";
        private const string StartTaskAction = "Start";
        private const string StopTaskAction = "Stop";
        private const string RecycleTaskAction = "Recycle";
        private ServerManager iisServerManager;
        private bool autoStart = true;
        private ManagedPipelineMode managedPM = ManagedPipelineMode.Integrated;
        private ProcessModelIdentityType processModelType = ProcessModelIdentityType.LocalService;
        private ApplicationPool pool;

        /// <summary>
        /// Sets the TaskAction.
        /// </summary>
        [DropdownValue(CheckExistsTaskAction)]
        [DropdownValue(CreateTaskAction)]
        [DropdownValue(DeleteTaskAction)]
        [DropdownValue(GetInfoTaskAction)]
        [DropdownValue(ModifyTaskAction)]
        [DropdownValue(RecycleTaskAction)]
        [DropdownValue(SetIdentityTaskAction)]
        [DropdownValue(SetPipelineModeTaskAction)]
        [DropdownValue(StartTaskAction)]
        [DropdownValue(StopTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Sets the private memory (kb) a process can use before the process is recycled. Default is 0. Set > 0 to use. Set to -1 to restore the Application Pool Default.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public long PeriodicRestartPrivateMemory { get; set; }

        /// <summary>
        /// Sets the maximum number of requests to queue before rejecting additional requests. Default is 0. Set > 0 to use. Set to -1 to restore the Application Pool Default.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public long QueueLength { get; set; }

        /// <summary>
        /// Sets a TimeSpan value in minutes for the period of time a process should remain idle. Set > 0 to use. Set to -1 to restore the Application Pool Default.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public long IdleTimeout { get; set; }

        /// <summary>
        /// Sets the maximum number of worker processes allowed for the AppPool. Set to -1 to restore the Application Pool Default.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public long MaxProcesses { get; set; }

        /// <summary>
        /// Sets a TimeSpan value in minutes for the period of time that should elapse before a worker process is recycled. Default is 29 hours. Set > 0 to use. Set to -1 to restore the Application Pool Default for Modify or -1 to Disable Recycling.PeriodicRestartTime for Create
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public long PeriodicRestartTime { get; set; }

        /// <summary>
        /// Sets the times that the application pool should recycle. Format is 'hh:mm,hh:mm,hh:mm'. Set to "-1" to clear the RecycleTimes
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public string RecycleTimes { get; set; }

        /// <summary>
        /// Sets the fixed number of requests to recycle the application pool. Set to -1 to restore the Application Pool Default.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public int RecycleRequests { get; set; }

        /// <summary>
        /// Sets the RecycleInterval in minutes for the application pool. Set to -1 to restore the Application Pool Default.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public int RecycleInterval { get; set; }      

        /// <summary>
        /// Set whether the application pool should start automatically. Default is true.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public bool AutoStart
        {
            get { return this.autoStart; }
            set { this.autoStart = value; }
        }

        /// <summary>
        /// Sets whether 32-bit applications are enabled on 64-bit processors. Default is false.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public bool Enable32BitAppOnWin64 { get; set; }

        /// <summary>
        /// Sets the ProcessModelIdentityType. Default is LocalService
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(SetIdentityTaskAction, false)]
        public string IdentityType
        {
            get { return this.processModelType.ToString(); }
            set { this.processModelType = (ProcessModelIdentityType)Enum.Parse(typeof(ProcessModelIdentityType), value); }
        }

        /// <summary>
        /// Sets the user name associated with the security identity under which the application pool runs.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(SetIdentityTaskAction, false)]
        public string PoolIdentity { get; set; }

        /// <summary>
        /// Sets the password associated with the PoolIdentity property.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(SetIdentityTaskAction, false)]
        public string IdentityPassword { get; set; }

        /// <summary>
        /// Sets the version number of the .NET Framework used by the application pool. Default is "v2.0".
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public string ManagedRuntimeVersion { get; set; }

        /// <summary>
        /// Sets the ManagedPipelineMode. Default is ManagedPipelineMode.Integrated.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(SetPipelineModeTaskAction, false)]
        public string PipelineMode
        {
            get { return this.managedPM.ToString(); }
            set { this.managedPM = (ManagedPipelineMode)Enum.Parse(typeof(ManagedPipelineMode), value); }
        }

        /// <summary>
        /// Sets the name of the AppPool
        /// </summary>
        [Required]
        [TaskAction(CheckExistsTaskAction, true)]
        [TaskAction(CreateTaskAction, true)]
        [TaskAction(DeleteTaskAction, true)]
        [TaskAction(GetInfoTaskAction, true)]
        [TaskAction(ModifyTaskAction, true)]
        [TaskAction(RecycleTaskAction, true)]
        [TaskAction(StartTaskAction, true)]
        [TaskAction(StopTaskAction, true)]
        public string Name { get; set; }

        /// <summary>
        /// Set to true to force the creation of a website, even if it exists.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [TaskAction(ModifyTaskAction, false)]
        public bool Force { get; set; }

        /// <summary>
        /// Gets the AppPoolInfo Item. Identity = Name, MetaData = ApplicationPoolName, PhysicalPath, Id, State
        /// </summary>
        [Output]
        public ITaskItem AppPoolInfo { get; set; }

        /// <summary>
        /// Gets whether the Application Pool exists
        /// </summary>
        [Output]
        [TaskAction(CheckExistsTaskAction, false)]
        public bool Exists { get; set; }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        protected override void InternalExecute()
        {
            try
            {
                this.iisServerManager = System.Environment.MachineName != this.MachineName ? ServerManager.OpenRemote(this.MachineName) : new ServerManager();

                switch (this.TaskAction)
                {
                    case CreateTaskAction:
                        this.Create();
                        break;
                    case GetInfoTaskAction:
                        this.GetInfo();
                        break;
                    case ModifyTaskAction:
                        this.Modify();
                        break;
                    case DeleteTaskAction:
                        this.Delete();
                        break;
                    case CheckExistsTaskAction:
                        this.CheckExists();
                        break;
                    case SetIdentityTaskAction:
                        this.SetIdentity();
                        break;
                    case SetPipelineModeTaskAction:
                        this.SetPipelineMode();
                        break;
                    case StartTaskAction:
                    case StopTaskAction:
                    case RecycleTaskAction:
                        this.ControlAppPool();
                        break;
                    default:
                        this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                        return;
                }
            }
            finally
            {
                if (this.iisServerManager != null)
                {
                    this.iisServerManager.Dispose();
                }
            }
        }

        private void CheckExists()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Checking whether Application Pool: {0} exists on: {1}", this.Name, this.MachineName));
            this.Exists = this.AppPoolExists();
        }

        private void GetInfo()
        {
            if (!this.AppPoolExists())
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The ApplicationPool: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            ITaskItem iappPool = new TaskItem(this.Name);
            iappPool.SetMetadata("ManagedPipelineMode", this.pool.ManagedPipelineMode.ToString());
            iappPool.SetMetadata("IdentityType", this.pool.ProcessModel.IdentityType.ToString());
            iappPool.SetMetadata("ManagedRuntimeVersion", this.pool.ManagedRuntimeVersion);
            iappPool.SetMetadata("QueueLength", this.pool.QueueLength.ToString(CultureInfo.CurrentCulture));
            iappPool.SetMetadata("IdleTimeout", this.pool.ProcessModel.IdleTimeout.ToString());
            iappPool.SetMetadata("PrivateMemory", this.pool.Recycling.PeriodicRestart.PrivateMemory.ToString(CultureInfo.CurrentCulture));
            iappPool.SetMetadata("PeriodicRestartTime", this.pool.Recycling.PeriodicRestart.Time.ToString());
            iappPool.SetMetadata("MaxProcesses", this.pool.ProcessModel.MaxProcesses.ToString(CultureInfo.CurrentCulture));
            iappPool.SetMetadata("RecycleTimes", this.pool.Recycling.PeriodicRestart.Schedule.ToString());
            iappPool.SetMetadata("RecycleRequests", this.pool.Recycling.PeriodicRestart.Requests.ToString(CultureInfo.CurrentCulture));
            iappPool.SetMetadata("RecycleInterval", this.pool.Recycling.PeriodicRestart.Time.ToString());

            this.AppPoolInfo = iappPool;
        }

        private void SetPipelineMode()
        {
            if (!this.AppPoolExists())
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The ApplicationPool: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Modifying ApplicationPool: {0} on: {1}", this.Name, this.MachineName));
            this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting ManagedPipelineMode to: {0}", this.PipelineMode));
            this.pool.ManagedPipelineMode = this.managedPM;
            this.iisServerManager.CommitChanges();
        }

        private void SetIdentity()
        {
            if (!this.AppPoolExists())
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The ApplicationPool: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            if (this.IdentityType == "SpecificUser" && (string.IsNullOrEmpty(this.PoolIdentity) || string.IsNullOrEmpty(this.IdentityPassword)))
            {
                Log.LogError("PoolIdentity and PoolPassword must be specified if the IdentityType is SpecificUser");
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Modifying ApplicationPool: {0} on: {1}", this.Name, this.MachineName));
            this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting ProcessModelIdentityType to: {0}", this.IdentityType));
            this.pool.ProcessModel.IdentityType = this.processModelType;

            if (this.IdentityType == "SpecificUser")
            {
                this.pool.ProcessModel.UserName = this.PoolIdentity;
                this.pool.ProcessModel.Password = this.IdentityPassword;
            }

            this.iisServerManager.CommitChanges();
        }

        private void Delete()
        {
            if (!this.AppPoolExists())
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The ApplicationPool: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Deleting ApplicationPool: {0} on: {1}", this.Name, this.MachineName));
            this.iisServerManager.ApplicationPools.Remove(this.pool);
            this.iisServerManager.CommitChanges();
        }

        private void ControlAppPool()
        {
            if (!this.AppPoolExists())
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The ApplicationPool: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "{0} ApplicationPool: {1} on: {2}", this.TaskAction, this.Name, this.MachineName));

            switch (this.TaskAction)
            {
                case StartTaskAction:
                    this.pool.Start();
                    break;
                case StopTaskAction:
                    if (this.pool.State != ObjectState.Stopped && this.pool.State != ObjectState.Stopping)
                    {
                        this.pool.Stop();
                    }

                    break;
                case RecycleTaskAction:
                    this.pool.Start();
                    this.pool.Recycle();
                    break;
            }
        }

        private void Create()
        {
            if (this.AppPoolExists())
            {
                if (!this.Force)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, "The ApplicationPool: {0} already exists on: {1}", this.Name, this.MachineName));
                    return;
                }

                this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Deleting ApplicationPool: {0} on: {1}", this.Name, this.MachineName));
                this.iisServerManager.ApplicationPools.Remove(this.pool);
                this.iisServerManager.CommitChanges();
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Creating ApplicationPool: {0} on: {1}", this.Name, this.MachineName));

            if (this.IdentityType == "SpecificUser" && (string.IsNullOrEmpty(this.PoolIdentity) || string.IsNullOrEmpty(this.IdentityPassword)))
            {
                Log.LogError("PoolIdentity and PoolPassword must be specified if the IdentityType is SpecificUser");
                return;
            }

            this.pool = this.iisServerManager.ApplicationPools.Add(this.Name);
            this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting ManagedPipelineMode to: {0}", this.PipelineMode));
            this.pool.ManagedPipelineMode = this.managedPM;
            this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting ProcessModelIdentityType to: {0}", this.IdentityType));
            this.pool.ProcessModel.IdentityType = this.processModelType;
            if (this.IdentityType == "SpecificUser")
            {
                this.pool.ProcessModel.UserName = this.PoolIdentity;
                this.pool.ProcessModel.Password = this.IdentityPassword;
            }

            this.SetCommonInfo();
            this.iisServerManager.CommitChanges();
        }

        private void Modify()
        {
            if (!this.AppPoolExists())
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The ApplicationPool: {0} was not found on: {1}", this.Name, this.MachineName));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Modifying ApplicationPool: {0} on: {1}", this.Name, this.MachineName));
            this.SetCommonInfo();
            this.iisServerManager.CommitChanges();
        }

        private void SetCommonInfo()
        {
            this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting AutoStart to: {0}", this.AutoStart));
            this.pool.AutoStart = this.AutoStart;
            this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Enable32BitAppOnWin64 to: {0}", this.Enable32BitAppOnWin64));
            this.pool.Enable32BitAppOnWin64 = this.Enable32BitAppOnWin64;

            if (!string.IsNullOrEmpty(this.ManagedRuntimeVersion))
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting ManagedRuntimeVersion to: {0}", this.ManagedRuntimeVersion));
                this.pool.ManagedRuntimeVersion = this.ManagedRuntimeVersion;
            }

            if (this.QueueLength > 0)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting QueueLength to: {0}", this.QueueLength));
                this.pool.QueueLength = this.QueueLength;
            }
            else if (this.QueueLength == -1)
            {
                this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Setting QueueLength to: {0}", this.iisServerManager.ApplicationPoolDefaults.QueueLength));
                this.pool.QueueLength = this.iisServerManager.ApplicationPoolDefaults.QueueLength;
            }

            if (this.IdleTimeout > 0)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting IdleTimeout to: {0} minutes", this.IdleTimeout));
                this.pool.ProcessModel.IdleTimeout = TimeSpan.FromMinutes(this.IdleTimeout);
            }
            else if (this.IdleTimeout == -1)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting IdleTimeout to: {0}", this.iisServerManager.ApplicationPoolDefaults.ProcessModel.IdleTimeout));
                this.pool.ProcessModel.IdleTimeout = this.iisServerManager.ApplicationPoolDefaults.ProcessModel.IdleTimeout;
            }

            if (this.PeriodicRestartPrivateMemory > 0)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.PrivateMemory to: {0}", this.PeriodicRestartPrivateMemory));
                this.pool.Recycling.PeriodicRestart.PrivateMemory = this.PeriodicRestartPrivateMemory;
            }
            else if (this.PeriodicRestartPrivateMemory == -1)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.PrivateMemory to: {0}", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.PrivateMemory));
                this.pool.Recycling.PeriodicRestart.PrivateMemory = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.PrivateMemory;
            }

            if (this.PeriodicRestartTime > 0)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestartTime to: {0} minutes", this.PeriodicRestartTime));
                this.pool.Recycling.PeriodicRestart.Time = TimeSpan.FromMinutes(this.PeriodicRestartTime);
            }
            else if (this.PeriodicRestartTime == -1)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestartTime to: {0} minutes", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time));
                this.pool.Recycling.PeriodicRestart.Time = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time;
            }

            if (this.MaxProcesses > 0)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting ProcessModel.MaxProcesses to: {0}", this.MaxProcesses));
                this.pool.ProcessModel.MaxProcesses = this.MaxProcesses;
            }
            else if (this.MaxProcesses == -1)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting ProcessModel.MaxProcesses to: {0}", this.MaxProcesses));
                this.pool.ProcessModel.MaxProcesses = this.iisServerManager.ApplicationPoolDefaults.ProcessModel.MaxProcesses;
            }

            if (!string.IsNullOrEmpty(this.RecycleTimes))
            {
                string[] times = this.RecycleTimes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string time in times)
                {
                    double hours = Convert.ToDouble(time.Split(new[] { ':' })[0], CultureInfo.CurrentCulture);
                    double minutes = Convert.ToDouble(time.Split(new[] { ':' })[1], CultureInfo.CurrentCulture);
                    this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.Schedule to: {0}:{1}", hours, minutes));
                    this.pool.Recycling.PeriodicRestart.Schedule.Add(TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes)));
                }
            }
            else if (this.RecycleTimes == "-1")
            {
                this.LogTaskMessage(MessageImportance.Low, "Clearing the Recycling.PeriodicRestart.Schedule");
                this.pool.Recycling.PeriodicRestart.Schedule.Clear();
            }

            if (this.RecycleRequests > 0)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.RecycleRequests to: {0}", this.RecycleRequests));
                this.pool.Recycling.PeriodicRestart.Requests = this.RecycleRequests;
            }
            else if (this.RecycleRequests == -1)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.RecycleRequests to: {0}", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Requests));
                this.pool.Recycling.PeriodicRestart.Requests = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Requests;
            }

            if (this.RecycleInterval > 0)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.Time to: {0}", this.RecycleInterval));
                this.pool.Recycling.PeriodicRestart.Time = TimeSpan.FromMinutes(this.RecycleInterval);
            }
            else if (this.RecycleInterval == -1)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Setting Recycling.PeriodicRestart.Time to: {0}", this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time));
                this.pool.Recycling.PeriodicRestart.Time = this.iisServerManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.Time;
            }
        }

        private bool AppPoolExists()
        {
            this.pool = this.iisServerManager.ApplicationPools[this.Name];
            return this.pool != null;
        }
    }
}
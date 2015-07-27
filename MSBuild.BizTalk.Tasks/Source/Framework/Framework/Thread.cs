//-----------------------------------------------------------------------
// <copyright file="Thread.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Framework
{
    using System.Globalization;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Abort</i> (Warning: use only in exceptional circumstances to force an abort)</para>
    /// <para><i>Sleep</i> (<b>Required: </b> Timeout)</para>
    /// <para><i>SpinWait</i> (<b>Required: </b> Iterations)</para>
    /// <para><b>Remote Execution Support:</b> NA</para>
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
    ///         <!-- Set a thread to sleep for a period -->
    ///         <MSBuild.ExtensionPack.Framework.Thread TaskAction="Sleep" Timeout="2000"/>
    ///         <!-- Set a thread to spinwait for a period -->
    ///         <MSBuild.ExtensionPack.Framework.Thread TaskAction="SpinWait" Iterations="1000000000"/>
    ///         <!-- Abort a thread. Only use in exceptional circumstances -->
    ///         <!--<MSBuild.ExtensionPack.Framework.Thread TaskAction="Abort"/>-->
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>  
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.4.0/html/c73b9da3-3269-a0b1-2f17-b51c2db37293.htm")]
    public class Thread : BaseTask
    {
        private const string AbortTaskAction = "Abort";
        private const string SleepTaskAction = "Sleep";
        private const string SpinWaitTaskAction = "SpinWait";

        [DropdownValue(AbortTaskAction)]
        [DropdownValue(SleepTaskAction)]
        [DropdownValue(SpinWaitTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Number of millseconds to sleep for
        /// </summary>
        [TaskAction(SleepTaskAction, true)]
        public int Timeout { get; set; }

        /// <summary>
        /// Number of iterations to wait for
        /// </summary>
        [TaskAction(SpinWaitTaskAction, true)]
        public int Iterations { get; set; }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        protected override void InternalExecute()
        {
            if (!this.TargetingLocalMachine())
            {
                return;
            }

            switch (this.TaskAction)
            {
                case "Abort":
                    this.LogTaskMessage("Aborting Current Thread");
                    System.Threading.Thread thisThread = System.Threading.Thread.CurrentThread;
                    thisThread.Abort();
                    break;
                case "Sleep":
                    this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Sleeping all threads for: {0}ms", this.Timeout));
                    System.Threading.Thread.Sleep(this.Timeout);
                    break;
                case "SpinWait":
                    this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "SpinWait all threads for: {0} iterations", this.Iterations));
                    System.Threading.Thread.SpinWait(this.Iterations);
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }
    }
}
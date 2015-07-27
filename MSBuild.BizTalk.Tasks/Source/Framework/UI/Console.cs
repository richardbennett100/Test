//-----------------------------------------------------------------------
// <copyright file="Console.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.UI
{
    using System.Globalization;
    using Microsoft.Build.Framework;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Beep</i> (<b>Optional: </b>Title, Repeat, Duration, Frequency, Interval)</para>
    /// <para><i>ReadLine</i> (<b>Optional: </b>Title, UserPrompt, ToLower, ToUpper <b>Output: </b> UserResponse)</para>
    /// <para><b>Remote Execution Support:</b> No</para>
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
    ///         <!-- Read input from the user -->
    ///         <MSBuild.ExtensionPack.UI.Console TaskAction="ReadLine">
    ///             <Output TaskParameter="UserResponse" PropertyName="Line"/>
    ///         </MSBuild.ExtensionPack.UI.Console>
    ///         <Message Text="User Typed: $(Line)"/>
    ///          <!-- Read input from the user and uppercase it all -->
    ///         <MSBuild.ExtensionPack.UI.Console TaskAction="ReadLine" UserPrompt="Please enter your password and press the [Enter] key" ToUpper="true">
    ///             <Output TaskParameter="UserResponse" PropertyName="Line"/>
    ///         </MSBuild.ExtensionPack.UI.Console>
    ///         <Message Text="User Typed: $(Line)"/>
    ///         <!-- Play some beeps -->
    ///         <MSBuild.ExtensionPack.UI.Console TaskAction="Beep" Repeat="3"/>
    ///         <MSBuild.ExtensionPack.UI.Console TaskAction="Beep" Repeat="4" Duration="500" Frequency="1000"/>
    ///         <MSBuild.ExtensionPack.UI.Console TaskAction="Beep" Repeat="3" Interval="2000"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>
    /// </example>
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.4.0/html/e7981fca-ab45-8126-d9e8-e905a1176cb2.htm")]
    public class Console : BaseTask
    {
        private const string BeepTaskAction = "Beep";
        private const string ReadLineTaskAction = "ReadLine";
        
        private int duration = 333;
        private int frequency = 600;
        private int repeat = 1;
        private int interval = 10;

        [DropdownValue(BeepTaskAction)]
        [DropdownValue(ReadLineTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Sets the interval between beebs. Default is 10ms. Value must be between 10 and 5000
        /// </summary>
        [TaskAction(BeepTaskAction, false)]
        public int Interval
        {
            get { return this.interval; }
            set { this.interval = value; }
        }

        /// <summary>
        /// Sets the duration. Default is 333ms. Value must be between 1 and 10000
        /// </summary>
        [TaskAction(BeepTaskAction, false)]
        public int Duration
        {
            get { return this.duration; }
            set { this.duration = value; }
        }

        /// <summary>
        /// Sets the repeat. Default is 1. Value must be between 1 and 20
        /// </summary>
        [TaskAction(BeepTaskAction, false)]
        public int Repeat
        {
            get { return this.repeat; }
            set { this.repeat = value; }
        }

        /// <summary>
        /// Sets the frequency. Default is 600hz. Value must be between 37 and 32767
        /// </summary>
        [TaskAction(BeepTaskAction, false)]
        public int Frequency
        {
            get { return this.frequency; }
            set { this.frequency = value; }
        }

        /// <summary>
        /// Set the title of the console
        /// </summary>
        [TaskAction(BeepTaskAction, false)]
        [TaskAction(ReadLineTaskAction, false)]
        public string Title { get; set; }

        /// <summary>
        /// The message to prompt the user for input. Default is "Please enter a response and press [Enter]:"
        /// </summary>
        [TaskAction(ReadLineTaskAction, false)]
        public string UserPrompt { get; set; }

        /// <summary>
        /// Sets the UserResponse to lower text
        /// </summary>
        [TaskAction(ReadLineTaskAction, false)]
        public bool ToLower { get; set; }

        /// <summary>
        /// Sets the UserResponse to uppper text
        /// </summary>
        [TaskAction(ReadLineTaskAction, false)]
        public bool ToUpper { get; set; }

        /// <summary>
        /// Gets the response that the user typed
        /// </summary>
        [Output]
        [TaskAction(ReadLineTaskAction, false)]
        public string UserResponse { get; set; }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        protected override void InternalExecute()
        {
            if (!this.TargetingLocalMachine())
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.Title))
            {
                System.Console.Title = this.Title;
            }

            switch (this.TaskAction)
            {
                case "Beep":
                    this.Beep();
                    break;
                case "ReadLine":
                    this.ReadLine();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        private void ReadLine()
        {
            if (string.IsNullOrEmpty(this.UserPrompt))
            {
                this.UserPrompt = "Please enter a response and press [Enter]:";
            }

            this.LogTaskMessage(MessageImportance.High, this.UserPrompt);
            this.UserResponse = System.Console.ReadLine();

            if (this.UserResponse != null)
            {
                if (this.ToLower)
                {
                    this.UserResponse = this.UserResponse.ToLower(CultureInfo.CurrentCulture);
                }

                if (this.ToUpper)
                {
                    this.UserResponse = this.UserResponse.ToUpper(CultureInfo.CurrentCulture);
                }
            }
        }

        private void Beep()
        {
            if (this.Frequency < 37 || this.Frequency > 32767)
            {
                this.LogTaskWarning(string.Format(CultureInfo.CurrentCulture, "Invalid Frequency: {0}. Value must be between 37 and 32767. Using default of 600.", this.Frequency));
                this.Frequency = 600;
            }

            if (this.Duration < 1 || this.Duration > 10000)
            {
                this.LogTaskWarning(string.Format(CultureInfo.CurrentCulture, "Invalid Duration: {0}. Value must be between 1 and 10000. Using default of 333.", this.Duration));
                this.Duration = 333;
            }

            if (this.Repeat < 1 || this.Repeat > 20)
            {
                this.LogTaskWarning(string.Format(CultureInfo.CurrentCulture, "Invalid Repeat: {0}. Value must be between 1 and 20. Using default of 1.", this.Repeat));
                this.Repeat = 1;
            }

            if (this.Interval < 10 || this.Interval > 5000)
            {
                this.LogTaskWarning(string.Format(CultureInfo.CurrentCulture, "Invalid Interval: {0}. Value must be between 10 and 5000. Using default of 10.", this.Interval));
                this.Interval = 10;
            }

            for (int i = 1; i <= this.Repeat; i++)
            {
                System.Console.Beep(this.Frequency, this.Duration);
                System.Threading.Thread.Sleep(this.Interval);
            }
        }
    }
}
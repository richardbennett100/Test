//-----------------------------------------------------------------------
// <copyright file="Iis6Website.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Web
{
    using System;
    using System.DirectoryServices;
    using System.Globalization;
    using System.IO;
    using Microsoft.Build.Framework;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Create</i> (<b>Required: </b> Name <b>Optional:</b> Force, Properties, Identifier <b>OutPut: </b>Identifier)</para>
    /// <para><i>CheckExists</i> (<b>Required: </b> Name <b>Output: </b>Exists)</para>
    /// <para><i>Continue</i> (<b>Required: </b> Name)</para>
    /// <para><i>Delete</i> (<b>Required: </b> Name)</para>
    /// <para><i>Start</i> (<b>Required: </b> Name)</para>
    /// <para><i>Stop</i> (<b>Required: </b> Name)</para>
    /// <para><i>Pause</i> (<b>Required: </b> Name)</para>
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
    ///         <!-- Create a website -->
    ///         <MSBuild.ExtensionPack.Web.Iis6Website TaskAction="Create"  Name="awebsite" Force="true" Properties="AspEnableApplicationRestart=False;AspScriptTimeout=1200;ContentIndexed=False;LogExtFileFlags=917455;ScriptMaps=;ServerBindings=:80:www.free2todev.com;SecureBindings=;ServerAutoStart=True;UseHostName=True"/>
    ///         <!-- Pause a website -->
    ///         <MSBuild.ExtensionPack.Web.Iis6Website TaskAction="Pause" Name="awebsite" />
    ///         <!-- Stop a website -->
    ///         <MSBuild.ExtensionPack.Web.Iis6Website TaskAction="Stop" Name="awebsite" />
    ///         <!-- Start a website -->
    ///         <MSBuild.ExtensionPack.Web.Iis6Website TaskAction="Start" Name="awebsite" />
    ///         <!-- Check whether a website exists -->
    ///         <MSBuild.ExtensionPack.Web.Iis6Website TaskAction="CheckExists" Name="awebsite">
    ///             <Output PropertyName="SiteExists" TaskParameter="Exists"/>
    ///         </MSBuild.ExtensionPack.Web.Iis6Website>
    ///         <Message Text="Website Exists: $(SiteExists)"/>
    ///         <!-- Check whether a website exists -->
    ///         <MSBuild.ExtensionPack.Web.Iis6Website TaskAction="CheckExists" Name="anonwebsite">
    ///             <Output PropertyName="SiteExists" TaskParameter="Exists"/>
    ///         </MSBuild.ExtensionPack.Web.Iis6Website>
    ///         <Message Text="Website Exists: $(SiteExists)"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.4.0/html/2849df01-25a8-6f99-5a0c-0fa7a6df5084.htm")]
    public class Iis6Website : BaseTask
    {
        private const string CreateTaskAction = "Create";
        private const string CheckExistsTaskAction = "CheckExists";
        private const string ContinueTaskAction = "Continue";
        private const string DeleteTaskAction = "Delete";
        private const string StartTaskAction = "Start";
        private const string StopTaskAction = "Stop";
        private const string PauseTaskAction = "Pause";

        private DirectoryEntry websiteEntry;
        private string properties;
        private int sleep = 250;

        [DropdownValue(CreateTaskAction)]
        [DropdownValue(CheckExistsTaskAction)]
        [DropdownValue(ContinueTaskAction)]
        [DropdownValue(DeleteTaskAction)]
        [DropdownValue(StartTaskAction)]
        [DropdownValue(StopTaskAction)]
        [DropdownValue(PauseTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Sets the Properties. Use a semi-colon delimiter. See <a href="http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/cde669f1-5714-4159-af95-f334251c8cbd.mspx?mfr=true">Metabase Property Reference (IIS 6.0)</a>
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        public string Properties
        {
            get { return System.Web.HttpUtility.HtmlDecode(this.properties); }
            set { this.properties = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        [TaskAction(CreateTaskAction, true)]
        [TaskAction(CheckExistsTaskAction, true)]
        [TaskAction(ContinueTaskAction, true)]
        [TaskAction(DeleteTaskAction, true)]
        [TaskAction(StartTaskAction, true)]
        [TaskAction(StopTaskAction, true)]
        [TaskAction(PauseTaskAction, true)]
        public string Name { get; set; }

        /// <summary>
        /// Set force to true to delete an existing website when calling Create. Default is false.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        public bool Force { get; set; }

        /// <summary>
        /// Set the sleep time in ms for when calling Start, Stop, Pause or Continue. Default is 250ms.
        /// </summary>
        public int Sleep
        {
            get { return this.sleep; }
            set { this.sleep = value; }
        }

        /// <summary>
        /// Gets or sets the Identifier for the website. If specified for Create and the Identifier already exists, an error is logged.
        /// </summary>
        [TaskAction(CreateTaskAction, false)]
        [Output]
        public int Identifier { get; set; }

        /// <summary>
        /// Gets whether the website exists.
        /// </summary>
        [Output]
        [TaskAction(CheckExistsTaskAction, false)]
        public bool Exists { get; set; }

        /// <summary>
        /// Gets the IIS path.
        /// </summary>
        /// <value>The IIS path.</value>
        internal string IisPath
        {
            get { return "IIS://" + this.MachineName + "/W3SVC"; }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.TaskAction)
            {
                case "Create":
                    this.Create();
                    break;
                case "Delete":
                    this.Delete();
                    break;
                case "Start":
                case "Stop":
                case "Pause":
                case "Continue":
                    this.ControlWebsite();
                    break;
                case "CheckExists":
                    this.CheckWebsiteExists();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        private static void UpdateMetaBaseProperty(DirectoryEntry entry, string metaBasePropertyName, string metaBaseProperty)
        {
            if (metaBaseProperty.IndexOf('|') == -1)
            {
                string propertyTypeName = (string) new DirectoryEntry(entry.SchemaEntry.Parent.Path + "/" + metaBasePropertyName).Properties["Syntax"].Value;
                if (string.Compare(propertyTypeName, "binary", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    object[] metaBasePropertyBinaryFormat = new object[metaBaseProperty.Length / 2];
                    for (int i = 0; i < metaBasePropertyBinaryFormat.Length; i++)
                    {
                        metaBasePropertyBinaryFormat[i] = metaBaseProperty.Substring(i * 2, 2);
                    }

                    PropertyValueCollection propValues = entry.Properties[metaBasePropertyName];
                    propValues.Clear();
                    propValues.Add(metaBasePropertyBinaryFormat);
                    entry.CommitChanges();
                }
                else
                {
                    if (string.Compare(metaBasePropertyName, "path", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DirectoryInfo f = new DirectoryInfo(metaBaseProperty);
                        metaBaseProperty = f.FullName;
                    }

                    entry.Invoke("Put", metaBasePropertyName, metaBaseProperty);
                    entry.Invoke("SetInfo");
                }
            }
            else
            {
                entry.Invoke("Put", metaBasePropertyName, string.Empty);
                entry.Invoke("SetInfo");
                string[] metabaseProperties = metaBaseProperty.Split('|');
                foreach (string metabasePropertySplit in metabaseProperties)
                {
                    entry.Properties[metaBasePropertyName].Add(metabasePropertySplit);
                }

                entry.CommitChanges();
            }
        }

        private bool CheckWebsiteExists()
        {
            this.LoadWebsite();
            if (this.websiteEntry != null)
            {
                this.Exists = true;
            }

            return this.Exists;
        }

        private DirectoryEntry LoadWebService()
        {
            return new DirectoryEntry(this.IisPath);
        }

        private void LoadWebsite()
        {
            using (DirectoryEntry webService = this.LoadWebService())
            {
                DirectoryEntries webEntries = webService.Children;

                foreach (DirectoryEntry webEntry in webEntries)
                {
                    if (webEntry.SchemaClassName == "IIsWebServer")
                    {
                        if (string.Compare(this.Name, webEntry.Properties["ServerComment"][0].ToString(), StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            this.websiteEntry = webEntry;
                            break;
                        }
                    }

                    webEntry.Dispose();
                }
            }
        }

        private void Create()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentUICulture, "Creating Website: {0}", this.Name));
            using (DirectoryEntry webserviceEntry = this.LoadWebService())
            {
                // We'll try and find the website first.
                this.LoadWebsite();
                if (this.websiteEntry != null)
                {
                    if (this.Force)
                    {
                        this.LogTaskMessage(string.Format(CultureInfo.CurrentUICulture, "Website exists. Deleting Website: {0}", this.Name));
                        this.Delete();
                    }
                    else
                    {
                        Log.LogError(string.Format(CultureInfo.CurrentUICulture, "The Website already exists: {0}", this.Name));
                        return;
                    }
                }

                if (this.Identifier > 0)
                {
                    try
                    {
                        this.websiteEntry = (DirectoryEntry)webserviceEntry.Invoke("Create", "IIsWebServer", this.Identifier);
                        this.websiteEntry.CommitChanges();
                        webserviceEntry.CommitChanges();
                    }
                    catch
                    {
                        Log.LogError(string.Format(CultureInfo.CurrentUICulture, "WebsiteIdentifier {0} already exists. Aborting: {1}", this.Identifier, this.Name));
                        return;
                    }
                }
                else
                {
                    bool foundSlot = false;
                    this.Identifier = 1;
                    do
                    {
                        try
                        {
                            this.websiteEntry = (DirectoryEntry)webserviceEntry.Invoke("Create", "IIsWebServer", this.Identifier);
                            this.websiteEntry.CommitChanges();
                            webserviceEntry.CommitChanges();
                            foundSlot = true;
                        }
                        catch
                        {
                            if (this.Identifier > 1000)
                            {
                                Log.LogError(string.Format(CultureInfo.CurrentUICulture, "websiteIdentifier > 1000. Aborting: {0}", this.Name));
                                return;
                            }

                            ++this.Identifier;
                        }
                    }
                    while (foundSlot == false);
                }

                using (DirectoryEntry vdirEntry = (DirectoryEntry)this.websiteEntry.Invoke("Create", "IIsWebVirtualDir", "ROOT"))
                {
                    vdirEntry.CommitChanges();
                    this.websiteEntry.Invoke("Put", "AppFriendlyName", this.Name);
                    this.websiteEntry.Invoke("Put", "ServerComment", this.Name);
                    this.websiteEntry.Invoke("SetInfo");

                    // Now loop through all the metabase properties specified.
                    if (string.IsNullOrEmpty(this.Properties) == false)
                    {
                        string[] propList = this.Properties.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string s in propList)
                        {
                            string[] propPair = s.Split(new[] { '=' });
                            string propName = propPair[0];
                            string propValue = propPair.Length > 1 ? propPair[1] : string.Empty;
                            this.LogTaskMessage(string.Format(CultureInfo.CurrentUICulture, "Adding Property: {0}({1})", propName, propValue));
                            UpdateMetaBaseProperty(this.websiteEntry, propName, propValue);
                        }
                    }

                    vdirEntry.CommitChanges();
                    this.websiteEntry.CommitChanges();
                    this.websiteEntry.Dispose();
                }
            }
        }

        private void Delete()
        {
            if (this.CheckWebsiteExists())
            {
                using (DirectoryEntry webService = this.LoadWebService())
                {
                    object[] args = { "IIsWebServer", Convert.ToInt32(this.websiteEntry.Name, CultureInfo.InvariantCulture) };
                    webService.Invoke("Delete", args);
                }
            }
        }

        private void ControlWebsite()
        {
            if (this.CheckWebsiteExists())
            {
                this.LogTaskMessage(string.Format(CultureInfo.CurrentUICulture, "{0} Website: {1}", this.TaskAction, this.Name));
                
                // need to insert a sleep as the code occasionaly fails to work without a wait.
                System.Threading.Thread.Sleep(this.Sleep);
                this.websiteEntry.Invoke(this.TaskAction, null);
            }
            else
            {
                Log.LogError(string.Format(CultureInfo.CurrentUICulture, "Website not found: {0}", this.Name));
            }
        }
    }
}
//-----------------------------------------------------------------------
// <copyright file="Registry.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Computer
{
    using System;
    using System.Globalization;
    using Microsoft.Build.Framework;
    using Microsoft.Win32;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>CheckEmpty</i> (<b>Required: </b> RegistryHive, Key <b>Output: </b>Empty)</para>
    /// <para><i>CreateKey</i> (<b>Required: </b> RegistryHive, Key)</para>
    /// <para><i>DeleteKey</i> (<b>Required: </b> RegistryHive, Key)</para>
    /// <para><i>DeleteKeyTree</i> (<b>Required: </b> RegistryHive, Key)</para>
    /// <para><i>Get</i> (<b>Required: </b> RegistryHive, Key, Value <b>Output: </b>Data)</para>
    /// <para><i>Set</i> (<b>Required: </b> RegistryHive, Key, Value)</para>
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
    ///         <!-- Create a key -->
    ///         <MSBuild.ExtensionPack.Computer.Registry TaskAction="CreateKey" RegistryHive="LocalMachine" Key="SOFTWARE\ANewTemp"/>
    ///         <!-- Check if a key is empty -->
    ///         <MSBuild.ExtensionPack.Computer.Registry TaskAction="CheckEmpty" RegistryHive="LocalMachine" Key="SOFTWARE\ANewTemp">
    ///             <Output PropertyName="REmpty" TaskParameter="Empty"/>
    ///         </MSBuild.ExtensionPack.Computer.Registry>
    ///         <Message Text="SOFTWARE\ANewTemp is empty: $(REmpty)"/>
    ///         <!-- Set a value -->
    ///         <MSBuild.ExtensionPack.Computer.Registry TaskAction="Set" RegistryHive="LocalMachine" Key="SOFTWARE\ANewTemp" Value="MySetting" Data="21"/>
    ///         <!-- Get the value out -->
    ///         <MSBuild.ExtensionPack.Computer.Registry TaskAction="Get" RegistryHive="LocalMachine" Key="SOFTWARE\ANewTemp" Value="MySetting">
    ///             <Output PropertyName="RData" TaskParameter="Data"/>
    ///         </MSBuild.ExtensionPack.Computer.Registry>
    ///         <Message Text="Registry Value: $(RData)"/>
    ///         <!-- Check if a key is empty again -->
    ///         <MSBuild.ExtensionPack.Computer.Registry TaskAction="CheckEmpty" RegistryHive="LocalMachine" Key="SOFTWARE\ANewTemp">
    ///             <Output PropertyName="REmpty" TaskParameter="Empty"/>
    ///         </MSBuild.ExtensionPack.Computer.Registry>
    ///         <Message Text="SOFTWARE\ANewTemp is empty: $(REmpty)"/>
    ///         <!-- Delete a key -->
    ///         <MSBuild.ExtensionPack.Computer.Registry TaskAction="DeleteKey" RegistryHive="LocalMachine" Key="SOFTWARE\ANewTemp"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.4.0/html/9c8ecf24-3d8d-2b2d-e986-3e026dda95fe.htm")]
    public class Registry : BaseTask
    {
        private const string CheckEmptyTaskAction = "CheckEmpty";
        private const string CreateKeyTaskAction = "CreateKey";
        private const string DeleteKeyTaskAction = "DeleteKey";
        private const string DeleteKeyTreeTaskAction = "DeleteKeyTree";
        private const string GetTaskAction = "Get";
        private const string SetTaskAction = "Set";
        
        private RegistryKey registryKey;
        private RegistryHive hive;

        [DropdownValue(CheckEmptyTaskAction)]
        [DropdownValue(CreateKeyTaskAction)]
        [DropdownValue(DeleteKeyTaskAction)]
        [DropdownValue(DeleteKeyTreeTaskAction)]
        [DropdownValue(GetTaskAction)]
        [DropdownValue(SetTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Sets the type of the data.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        [Output]
        [TaskAction(GetTaskAction, false)]
        public string Data { get; set; }

        /// <summary>
        /// Sets the value. If Value is not provided, an attempt will be made to read the Default Value.
        /// </summary>
        [TaskAction(GetTaskAction, true)]
        [TaskAction(SetTaskAction, true)]        
        public string Value { get; set; }

        /// <summary>
        /// Sets the registry hive.
        /// </summary>
        [Required]
        [TaskAction(CheckEmptyTaskAction, true)]
        [TaskAction(CreateKeyTaskAction, true)]
        [TaskAction(DeleteKeyTaskAction, true)]
        [TaskAction(DeleteKeyTreeTaskAction, true)]
        [TaskAction(GetTaskAction, true)]
        [TaskAction(SetTaskAction, true)]
        public string RegistryHive { get; set; }

        /// <summary>
        /// Sets the key.
        /// </summary>
        [Required]
        [TaskAction(CheckEmptyTaskAction, true)]
        [TaskAction(CreateKeyTaskAction, true)]
        [TaskAction(DeleteKeyTaskAction, true)]
        [TaskAction(DeleteKeyTreeTaskAction, true)]
        [TaskAction(GetTaskAction, true)]
        [TaskAction(SetTaskAction, true)]
        public string Key { get; set; }

        /// <summary>
        /// Indicates whether the Registry Key is empty or not
        /// </summary>
        [Output]
        [TaskAction(CheckEmptyTaskAction, false)]
        public bool Empty { get; set; }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        protected override void InternalExecute()
        {
            try
            {
                this.hive = (Microsoft.Win32.RegistryHive)Enum.Parse(typeof(Microsoft.Win32.RegistryHive), this.RegistryHive, true);
                this.registryKey = RegistryKey.OpenRemoteBaseKey(this.hive, this.MachineName);
            }
            catch (System.ArgumentException)
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The Registry Hive provided is not valid: {0}", this.RegistryHive));
                return;
            }

            switch (this.TaskAction)
            {
                case "CreateKey":
                    this.CreateKey();
                    break;
                case "DeleteKey":
                    this.DeleteKey();
                    break;
                case "DeleteKeyTree":
                    this.DeleteKeyTree();
                    break;
                case "Get":
                    this.Get();
                    break;
                case "Set":
                    this.Set();
                    break;
                case "CheckEmpty":
                    this.CheckEmpty();
                    break;
                default:
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        /// <summary>
        /// Checks if a Registry Key contains values or subkeys.
        /// </summary>
        private void CheckEmpty()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Checking if Registry Key: {0} is empty in Hive: {1} on: {2}", this.Key, this.RegistryHive, this.MachineName));
            RegistryKey subKey = this.registryKey.OpenSubKey(this.Key, true);
            if (subKey != null)
            {
                if (subKey.SubKeyCount <= 0)
                {
                    this.Empty = subKey.ValueCount <= 0;
                }
                else
                {
                    this.Empty = false;
                }
            }
            else
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "Registry Key: {0} not found in Hive: {1} on: {2}", this.Key, this.RegistryHive, this.MachineName));
            }
        }

        private void Set()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Setting Registry Value: {0} for Key: {1} in Hive: {2} on: {3}", this.Value, this.Key, this.RegistryHive, this.MachineName));
            bool changed = false;
            RegistryKey subKey = this.registryKey.OpenSubKey(this.Key, true);
            if (subKey != null)
            {
                object oldData = subKey.GetValue(this.Value);
                if (oldData == null || oldData.ToString() != this.Data)
                {
                    if (string.IsNullOrEmpty(this.DataType))
                    {
                        subKey.SetValue(this.Value, this.Data);
                    }
                    else
                    {
                        // assumption that ',' is separator for binary and multistring value types.
                        char[] separator = { ',' };
                        object registryValue;

                        RegistryValueKind valueKind = (Microsoft.Win32.RegistryValueKind) Enum.Parse(typeof(RegistryValueKind), this.DataType, true);
                        switch (valueKind)
                        {
                            case RegistryValueKind.Binary:
                                string[] parts = this.Data.Split(separator);
                                byte[] val = new byte[parts.Length];
                                for (int i = 0; i < parts.Length; val[i++] = Byte.Parse(parts[i], CultureInfo.CurrentCulture))
                                {
                                }

                                registryValue = val;
                                break;
                            case RegistryValueKind.DWord:
                                registryValue = uint.Parse(this.Data, CultureInfo.CurrentCulture);
                                break;
                            case RegistryValueKind.MultiString:
                                parts = this.Data.Split(separator);
                                registryValue = parts;
                                break;
                            case RegistryValueKind.QWord:
                                registryValue = ulong.Parse(this.Data, CultureInfo.CurrentCulture);
                                break;
                            default:
                                registryValue = this.Data;
                                break;
                        }

                        subKey.SetValue(this.Value, registryValue, valueKind);
                    }

                    changed = true;
                }

                subKey.Close();
            }
            else
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "Registry Key: {0} not found in Hive: {1} on: {2}", this.Key, this.RegistryHive, this.MachineName));
            }

            if (changed)
            {
                // Broadcast config change
                if (0 == NativeMethods.SendMessageTimeout(NativeMethods.HWND_BROADCAST, NativeMethods.WM_SETTINGCHANGE, 0, "Environment", NativeMethods.SMTO_ABORTIFHUNG, 0, 0))
                {
                    this.LogTaskWarning("NativeMethods.SendMessageTimeout returned 0");
                }
            }

            this.registryKey.Close();
        }

        private void Get()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Getting Registry value: {0} from Key: {1} in Hive: {2} on: {3}", this.Value, this.Key, this.RegistryHive, this.MachineName));
            RegistryKey subKey = this.registryKey.OpenSubKey(this.Key, false);
            if (subKey == null)
            {
                Log.LogError(string.Format(CultureInfo.CurrentCulture, "The Reqistry Key provided is not valid: {0}", this.Key));
                return;
            }

            object v = subKey.GetValue(this.Value);
            if (v == null)
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, "A Default value was not found for the Registry Key: {0}", this.Key));
                }
                else
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, "The Reqistry value provided is not valid: {0}", this.Value));
                }

                return;
            }

            this.Data = v.ToString();
            subKey.Close();
            this.registryKey.Close();
        }

        private void DeleteKeyTree()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Deleting Key Tree: {0} in Hive: {1} on: {2}", this.Key, this.RegistryHive, this.MachineName));
            RegistryKey.OpenRemoteBaseKey(this.hive, this.MachineName).DeleteSubKeyTree(this.Key);
        }

        private void DeleteKey()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Deleting Registry Key: {0} in Hive: {1} on: {2}", this.Key, this.RegistryHive, this.MachineName));
            RegistryKey.OpenRemoteBaseKey(this.hive, this.MachineName).DeleteSubKey(this.Key, false);
        }

        private void CreateKey()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Creating Registry Key: {0} in Hive: {1} on: {2}", this.Key, this.RegistryHive, this.MachineName));
            this.registryKey = RegistryKey.OpenRemoteBaseKey(this.hive, this.MachineName).CreateSubKey(this.Key);
            
            if (this.registryKey != null)
            {
                this.registryKey.Close();
            }
        }
    }
}
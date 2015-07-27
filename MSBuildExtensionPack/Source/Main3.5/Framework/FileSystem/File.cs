﻿//-----------------------------------------------------------------------
// <copyright file="File.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// Parts of this task are based on code from (http://sedodream.codeplex.com). It is used here with permission.
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>AddAttributes</i> (<b>Required: </b>Files)</para>
    /// <para><i>CountLines</i> (<b>Required: </b>Files <b>Optional: </b>CommentIdentifiers, MazSize, MinSize <b>Output: </b>TotalLinecount, CommentLinecount, EmptyLinecount, CodeLinecount, TotalFilecount, IncludedFilecount, IncludedFiles, ExcludedFilecount, ExcludedFiles, ElapsedTime)</para>
    /// <para><i>GetChecksum</i> (<b>Required: </b>Path <b>Output: </b>Checksum)</para>
    /// <para><i>GetTempFileName</i> (<b>Output: </b>Path)</para>
    /// <para><i>FilterByContent</i> (<b>Required: </b>Files, RegexPattern <b>Output: </b>IncludedFiles, IncludedFilecount, ExcludedFilecount, ExcludedFiles)</para>
    /// <para><i>Move</i> (<b>Required: </b>Path, TargetPath)</para>
    /// <para><i>RemoveAttributes</i> (<b>Required: </b>Files)</para>
    /// <para><i>Replace</i> (<b>Required: </b>RegexPattern <b>Optional: </b>Replacement, Path, TextEncoding, Files)</para>
    /// <para><i>SetAttributes</i> (<b>Required: </b>Files)</para>
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
    ///     <ItemGroup>
    ///         <FilesToParse Include="c:\demo\file.txt"/>
    ///         <FilesToCount Include="C:\Demo\**\*.cs"/>
    ///         <AllFilesToCount Include="C:\Demo\**\*"/>
    ///         <AtFiles Include="c:\demo\file1.txt">
    ///             <Attributes>ReadOnly;Hidden</Attributes>
    ///         </AtFiles>
    ///         <AtFiles2 Include="c:\demo\file1.txt">
    ///             <Attributes>Normal</Attributes>
    ///         </AtFiles2>
    ///         <MyFiles Include="C:\demo\**\*.csproj"/>
    ///     </ItemGroup>
    ///     <Target Name="Default">
    ///         <!-- Get a temp file -->
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="GetTempFileName">
    ///             <Output TaskParameter="Path" PropertyName="TempPath"/>
    ///         </MSBuild.ExtensionPack.FileSystem.File>
    ///         <Message Text="TempPath: $(TempPath)"/>
    ///         <!-- Filter a collection of files based on their content -->
    ///         <Message Text="MyProjects %(MyFiles.Identity)"/>
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="FilterByContent" RegexPattern="Microsoft.WebApplication.targets" Files="@(MyFiles)">
    ///             <Output TaskParameter="IncludedFiles" ItemName="WebProjects"/>
    ///             <Output TaskParameter="ExcludedFiles" ItemName="NonWebProjects"/>
    ///             <Output TaskParameter="IncludedFileCount" PropertyName="WebProjectsCount"/>
    ///             <Output TaskParameter="ExcludedFileCount" PropertyName="NonWebProjectsCount"/>
    ///         </MSBuild.ExtensionPack.FileSystem.File>
    ///         <Message Text="WebProjects: %(WebProjects.Identity)"/>
    ///         <Message Text="NonWebProjects: %(NonWebProjects.Identity)"/>
    ///         <Message Text="WebProjectsCount: $(WebProjectsCount)"/>
    ///         <Message Text="NonWebProjectsCount: $(NonWebProjectsCount)"/>
    ///         <!-- Get the checksum of a file -->
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="GetChecksum" Path="C:\Projects\CodePlex\MSBuildExtensionPack\Solutions\Main3.5\SampleScratchpad\SampleBuildBinaries\AssemblyDemo.dll">
    ///             <Output TaskParameter="Checksum" PropertyName="chksm"/>
    ///         </MSBuild.ExtensionPack.FileSystem.File>
    ///         <Message Text="$(chksm)"/>
    ///         <!-- Replace file content using a regular expression -->
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="Replace" RegexPattern="regex" Replacement="iiiii" Files="@(FilesToParse)"/>
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="Replace" RegexPattern="regex" Replacement="idi" Path="c:\Demo*"/>
    ///         <!-- Count the number of lines in a file and exclude comments -->
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="CountLines" Files="@(FilesToCount)" CommentIdentifiers="//">
    ///             <Output TaskParameter="CodeLinecount" PropertyName="csharplines"/>
    ///             <Output TaskParameter="IncludedFiles" ItemName="MyIncludedFiles"/>
    ///             <Output TaskParameter="ExcludedFiles" ItemName="MyExcludedFiles"/>
    ///         </MSBuild.ExtensionPack.FileSystem.File>
    ///         <Message Text="C# CodeLinecount: $(csharplines)"/>
    ///         <Message Text="MyIncludedFiles: %(MyIncludedFiles.Identity)"/>
    ///         <Message Text="MyExcludedFiles: %(MyExcludedFiles.Identity)"/>
    ///         <!-- Count all lines in a file -->
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="CountLines" Files="@(AllFilesToCount)">
    ///             <Output TaskParameter="TotalLinecount" PropertyName="AllLines"/>
    ///         </MSBuild.ExtensionPack.FileSystem.File>
    ///         <Message Text="All Files TotalLinecount: $(AllLines)"/>
    ///         <!-- Set some attributes -->
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="SetAttributes" Files="@(AtFiles)"/>
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="SetAttributes" Files="@(AtFiles2)"/>
    ///         <!-- Move a file -->
    ///         <MSBuild.ExtensionPack.FileSystem.File TaskAction="Move" Path="c:\demo\file.txt" TargetPath="c:\dddd\d\oo\d\mee.txt"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>
    /// </example>
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.5.0/html/f8c545f9-d58f-640e-3fce-b10aa158ca95.htm")]
    public class File : BaseTask
    {
        private const string CountLinesTaskAction = "CountLines";
        private const string GetChecksumTaskAction = "GetChecksum";
        private const string FilterByContentTaskAction = "FilterByContent";
        private const string ReplaceTaskAction = "Replace";
        private const string SetAttributesTaskAction = "SetAttributes";
        private const string AddAttributesTaskAction = "AddAttributes";
        private const string MoveTaskAction = "Move";
        private const string RemoveAttributesTaskAction = "RemoveAttributes";
        private const string GetTempFileNameTaskAction = "GetTempFileName";

        private Encoding fileEncoding = Encoding.UTF8;
        private string replacement = string.Empty;
        private Regex parseRegex;
        private string[] commentIdentifiers;
        private List<ITaskItem> excludedFiles;
        private List<ITaskItem> includedFiles;

        [DropdownValue(AddAttributesTaskAction)]
        [DropdownValue(CountLinesTaskAction)]
        [DropdownValue(GetChecksumTaskAction)]
        [DropdownValue(GetTempFileNameTaskAction)]
        [DropdownValue(FilterByContentTaskAction)]
        [DropdownValue(MoveTaskAction)]
        [DropdownValue(RemoveAttributesTaskAction)]
        [DropdownValue(ReplaceTaskAction)]
        [DropdownValue(SetAttributesTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Sets the regex pattern.
        /// </summary>
        [TaskAction(ReplaceTaskAction, true)]
        [TaskAction(FilterByContentTaskAction, true)]
        public string RegexPattern { get; set; }

        /// <summary>
        /// The replacement text to use. Default is String.Empty
        /// </summary>
        [TaskAction(ReplaceTaskAction, false)]
        public string Replacement
        {
            get { return this.replacement; }
            set { this.replacement = value; }
        }

        /// <summary>
        /// A path to process or get. Use * for recursive folder processing. For the GetChecksum TaskAction, this indicates the path to the file to create a checksum for.
        /// </summary>
        [TaskAction(GetChecksumTaskAction, true)]
        [TaskAction(MoveTaskAction, true)]
        [TaskAction(ReplaceTaskAction, false)]
        [Output]
        public ITaskItem Path { get; set; }

        /// <summary>
        /// The file encoding to write the new file in. The task will attempt to default to the current file encoding.
        /// </summary>
        [TaskAction(ReplaceTaskAction, false)]
        public string TextEncoding { get; set; }

        /// <summary>
        /// Sets characters to be interpreted as comment identifiers. Semi-colon delimited. Only single line comments are currently supported.
        /// </summary>
        [TaskAction(CountLinesTaskAction, false)]
        public string CommentIdentifiers
        { 
            set { this.commentIdentifiers = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        /// <summary>
        /// An ItemList of files to process. If calling SetAttributes, RemoveAttributes or AddAttributes, include the attributes in an Attributes metadata tag, separated by a semicolon.
        /// </summary>
        [TaskAction(CountLinesTaskAction, true)]
        [TaskAction(SetAttributesTaskAction, true)]
        [TaskAction(AddAttributesTaskAction, true)]
        [TaskAction(MoveTaskAction, true)]
        [TaskAction(RemoveAttributesTaskAction, true)]
        [TaskAction(ReplaceTaskAction, false)]
        [TaskAction(FilterByContentTaskAction, true)]
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// Sets the TargetPath for a renamed file
        /// </summary>
        [TaskAction(MoveTaskAction, true)]
        public ITaskItem TargetPath { get; set; }

        /// <summary>
        /// Gets the total number of lines counted
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        public int TotalLinecount { get; set; }

        /// <summary>
        /// Gets the number of comment lines counted
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        public int CommentLinecount { get; set; }

        /// <summary>
        /// Gets the number of empty lines countered. Whitespace is ignored.
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        public int EmptyLinecount { get; set; }
      
        /// <summary>
        /// Gets the number of files counted
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        public int TotalFilecount { get; set; }

        /// <summary>
        /// Gets the number of code lines countered. This is calculated as Total - Comment - Empty
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        public int CodeLinecount { get; set; }

        /// <summary>
        /// Gets the number of excluded files
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        [TaskAction(FilterByContentTaskAction, false)]
        public int ExcludedFilecount { get; set; }

        /// <summary>
        /// Gets the number of included files
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        [TaskAction(FilterByContentTaskAction, false)]
        public int IncludedFilecount { get; set; }

        /// <summary>
        /// Sets the maximum size of files to count
        /// </summary>
        [TaskAction(CountLinesTaskAction, false)]
        public int MaxSize { get; set; }

        /// <summary>
        /// sets the minimum size of files to count
        /// </summary>
        [TaskAction(CountLinesTaskAction, false)]
        public int MinSize { get; set; }

        /// <summary>
        /// Gets the time taken to count the files. Value in seconds.
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        public string ElapsedTime { get; set; }

        /// <summary>
        /// Gets the file checksum
        /// </summary>
        [Output]
        [TaskAction(GetChecksumTaskAction, false)]
        public string Checksum { get; set; }

        /// <summary>
        /// Item collection of files Excluded from the count.
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        [TaskAction(FilterByContentTaskAction, false)]
        public ITaskItem[] ExcludedFiles
        {
            get { return this.excludedFiles == null ? null : this.excludedFiles.ToArray(); }
            set { this.excludedFiles = new List<ITaskItem>(value); }
        }

        /// <summary>
        /// Item collection of files included after filtering operations
        /// </summary>
        [Output]
        [TaskAction(CountLinesTaskAction, false)]
        [TaskAction(FilterByContentTaskAction, false)]
        public ITaskItem[] IncludedFiles
        {
            get { return this.includedFiles == null ? null : this.includedFiles.ToArray(); }
            set { this.includedFiles = new List<ITaskItem>(value); }
        }

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
                case CountLinesTaskAction:
                    this.CountLines();
                    break;
                case FilterByContentTaskAction:
                    this.FilterByContent();
                    break;
                case GetChecksumTaskAction:
                    this.GetChecksum();
                    break;
                case ReplaceTaskAction:
                    this.Replace();
                    break;
                case SetAttributesTaskAction:
                case AddAttributesTaskAction:
                case RemoveAttributesTaskAction:
                    this.SetAttributes();
                    break;
                case GetTempFileNameTaskAction:
                    this.LogTaskMessage("Getting temp file name");
                    this.Path = new TaskItem(System.IO.Path.GetTempFileName());
                    break;
                case MoveTaskAction:
                    this.Move();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        private static FileAttributes SetAttributes(string[] attributes)
        {
            FileAttributes flags = new FileAttributes();
            if (Array.IndexOf(attributes, "Archive") >= 0)
            {
                flags |= FileAttributes.Archive;
            }

            if (Array.IndexOf(attributes, "Compressed") >= 0)
            {
                flags |= FileAttributes.Compressed;
            }

            if (Array.IndexOf(attributes, "Encrypted") >= 0)
            {
                flags |= FileAttributes.Encrypted;
            }

            if (Array.IndexOf(attributes, "Hidden") >= 0)
            {
                flags |= FileAttributes.Hidden;
            }

            if (Array.IndexOf(attributes, "Normal") >= 0)
            {
                flags |= FileAttributes.Normal;
            }

            if (Array.IndexOf(attributes, "ReadOnly") >= 0)
            {
                flags |= FileAttributes.ReadOnly;
            }

            if (Array.IndexOf(attributes, "System") >= 0)
            {
                flags |= FileAttributes.System;
            }

            return flags;
        }

        private void FilterByContent()
        {
            if (this.Files == null)
            {
                Log.LogError("Files is required");
                return;
            }

            if (string.IsNullOrEmpty(this.RegexPattern))
            {
                Log.LogError("RegexPattern is required.");
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Filter file collection by content: {0}", this.RegexPattern));

            this.includedFiles = new List<ITaskItem>();
            this.excludedFiles = new List<ITaskItem>();
            foreach (ITaskItem f in this.Files)
            {
                string entireFile;

                using (StreamReader streamReader = new StreamReader(f.ItemSpec))
                {
                    entireFile = streamReader.ReadToEnd();
                }

                // Load the regex to use
                this.parseRegex = new Regex(this.RegexPattern, RegexOptions.Compiled);

                // Match the regular expression pattern against a text string.
                Match m = this.parseRegex.Match(entireFile);
                if (m.Success)
                {
                    this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Included: {0}", f.ItemSpec));
                    this.includedFiles.Add(f);
                }
                else
                {
                    this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "Excluded: {0}", f.ItemSpec));
                    this.excludedFiles.Add(f);
                }
            }

            this.IncludedFilecount = this.includedFiles.Count;
            this.ExcludedFilecount = this.excludedFiles.Count;
        }

        private void SetAttributes()
        {
            if (this.Files == null)
            {
                Log.LogError("Files is required");
                return;
            }

            switch (this.TaskAction)
            {
                case SetAttributesTaskAction:
                    this.LogTaskMessage("Setting file attributes");
                    foreach (ITaskItem f in this.Files)
                    {
                        FileInfo afile = new FileInfo(f.ItemSpec) { Attributes = SetAttributes(f.GetMetadata("Attributes").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) };
                    }

                    break;
                case AddAttributesTaskAction:
                    this.LogTaskMessage("Adding file attributes");
                    foreach (ITaskItem f in this.Files)
                    {
                        FileInfo file = new FileInfo(f.ItemSpec);
                        file.Attributes = file.Attributes | SetAttributes(f.GetMetadata("Attributes").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    break;
                case RemoveAttributesTaskAction:
                    this.LogTaskMessage("Removing file attributes");
                    foreach (ITaskItem f in this.Files)
                    {
                        FileInfo file = new FileInfo(f.ItemSpec);
                        file.Attributes = file.Attributes & ~SetAttributes(f.GetMetadata("Attributes").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    break;
            }
        }

        private void GetChecksum()
        {
            if (!System.IO.File.Exists(this.Path.GetMetadata("FullPath")))
            {
                this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid File passed: {0}", this.Path));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Getting Checksum for file: {0}", this.Path));
            using (FileStream fs = System.IO.File.OpenRead(this.Path.GetMetadata("FullPath")))
            {
                MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider();
                byte[] hash = csp.ComputeHash(fs);
                this.Checksum = BitConverter.ToString(hash).Replace("-", string.Empty).ToUpperInvariant();
                fs.Close();
            }
        }

        private void Move()
        {
            if (!System.IO.File.Exists(this.Path.GetMetadata("FullPath")))
            {
                this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid File passed: {0}", this.Path));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Moving File: {0} to: {1}", this.Path, this.TargetPath));

            // If the TargetPath has multiple folders, then we need to create the parent
            DirectoryInfo f = new DirectoryInfo(this.TargetPath.GetMetadata("FullPath"));
            string parentPath = this.TargetPath.GetMetadata("FullPath").Replace(@"\" + f.Name, string.Empty);
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            else if (System.IO.File.Exists(this.TargetPath.GetMetadata("FullPath")))
            {
                System.IO.File.Delete(this.TargetPath.GetMetadata("FullPath"));
            }

            System.IO.File.Move(this.Path.GetMetadata("FullPath"), this.TargetPath.GetMetadata("FullPath"));
        }

        private void CountLines()
        {
            if (this.Files == null)
            {
                Log.LogError("Files is required");
                return;
            }

            this.LogTaskMessage("Counting Lines");
            DateTime start = DateTime.Now;
            this.excludedFiles = new List<ITaskItem>();
            this.includedFiles = new List<ITaskItem>();
            
            foreach (ITaskItem f in this.Files)
            {
                if (this.MaxSize > 0 || this.MinSize > 0)
                {
                    FileInfo thisFile = new FileInfo(f.ItemSpec);
                    if (this.MaxSize > 0 && thisFile.Length / 1024 > this.MaxSize)
                    {
                        this.excludedFiles.Add(f);
                        break;
                    }

                    if (this.MinSize > 0 && thisFile.Length / 1024 < this.MinSize)
                    {
                        this.excludedFiles.Add(f);
                        break;
                    }
                }
                
                this.IncludedFilecount++;
                this.includedFiles.Add(f);
                using (StreamReader re = System.IO.File.OpenText(f.ItemSpec))
                {
                    string input;
                    while ((input = re.ReadLine()) != null)
                    {
                        input = input.Trim();

                        if (string.IsNullOrEmpty(input))
                        {
                            this.EmptyLinecount++;
                        }
                        else if (this.commentIdentifiers != null)
                        {
                            foreach (string s in this.commentIdentifiers)
                            {
                                if (input.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                                {
                                    this.CommentLinecount++;
                                }
                            }
                        }

                        this.TotalLinecount++;
                    }
                }
            }

            if (this.ExcludedFiles != null)
            {
                this.ExcludedFilecount = this.excludedFiles.Count;
            }
            
            TimeSpan t = DateTime.Now - start;
            this.ElapsedTime = t.Seconds.ToString(CultureInfo.CurrentCulture);
            this.CodeLinecount = this.TotalLinecount - this.CommentLinecount - this.EmptyLinecount;
            this.TotalFilecount = this.IncludedFilecount + this.ExcludedFilecount;
        }

        private void Replace()
        {
            if (!string.IsNullOrEmpty(this.TextEncoding))
            {
                try
                {
                    this.fileEncoding = Encoding.GetEncoding(this.TextEncoding);
                }
                catch (ArgumentException)
                {
                    Log.LogError(string.Format(CultureInfo.CurrentCulture, "{0} is not a supported encoding name.", this.TextEncoding));
                    return;
                }
            }

            if (string.IsNullOrEmpty(this.RegexPattern))
            {
                Log.LogError("RegexPattern is required.");
                return;
            }

            // Load the regex to use
            this.parseRegex = new Regex(this.RegexPattern, RegexOptions.Compiled);

            // Check to see if we are processing a file collection or a path
            if (this.Path != null)
            {
                // we need to process a path
                this.ProcessPath();
            }
            else
            {
                // we need to process a collection
                this.ProcessCollection();
            }
        }

        private void ProcessPath()
        {
            bool recursive = false;
            if (this.Path.ItemSpec.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                this.Path.ItemSpec = this.Path.ItemSpec.Remove(this.Path.ItemSpec.Length - 1, 1);
                recursive = true;
            }

            // Validation
            if (Directory.Exists(this.Path.ItemSpec) == false)
            {
                this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Path not found: {0}", this.Path.ItemSpec));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Processing Path: {0} with RegEx: {1}, ReplacementText: {2}", this.Path, this.RegexPattern, this.Replacement));

            // Check if we need to do a recursive search
            if (recursive)
            {
                // We have to do a recursive search
                // Create a new DirectoryInfo object.
                DirectoryInfo dir = new DirectoryInfo(this.Path.ItemSpec);

                if (!dir.Exists)
                {
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "The directory does not exist: {0}", this.Path.ItemSpec));
                    return;
                }

                // Call the GetFileSystemInfos method.
                FileSystemInfo[] infos = dir.GetFileSystemInfos("*");
                this.ProcessFolder(infos);
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(this.Path.ItemSpec);

                if (!dir.Exists)
                {
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "The directory does not exist: {0}", this.Path.ItemSpec));
                    return;
                }

                FileInfo[] fileInfo = dir.GetFiles();

                foreach (FileInfo f in fileInfo)
                {
                    this.ParseAndReplaceFile(f.FullName, false);
                }
            }
        }

        private void ProcessFolder(IEnumerable<FileSystemInfo> fileSysInfo)
        {
            // Iterate through each item.
            foreach (FileSystemInfo i in fileSysInfo)
            {
                // Check to see if this is a DirectoryInfo object.
                if (i is DirectoryInfo)
                {
                    // Cast the object to a DirectoryInfo object.
                    DirectoryInfo dirInfo = new DirectoryInfo(i.FullName);

                    // Iterate through all sub-directories.
                    this.ProcessFolder(dirInfo.GetFileSystemInfos("*"));
                }
                else if (i is FileInfo)
                {
                    // Check to see if this is a FileInfo object.
                    this.ParseAndReplaceFile(i.FullName, false);
                }
            }
        }

        private void ProcessCollection()
        {
            if (this.Files == null)
            {
                this.Log.LogError("No file collection has been passed");
                return;
            }

            this.LogTaskMessage("Processing File Collection");

            foreach (ITaskItem file in this.Files)
            {
                this.ParseAndReplaceFile(file.ItemSpec, true);
            }
        }

        private void ParseAndReplaceFile(string parseFile, bool checkExists)
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Processing File: {0}", parseFile));
            if (checkExists && System.IO.File.Exists(parseFile) == false)
            {
                this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "The file does not exist: {0}", parseFile));
                return;
            }

            // Open the file and attempt to read the encoding from the BOM.
            string entireFile;

            using (StreamReader streamReader = new StreamReader(parseFile, this.fileEncoding, true))
            {
                if (this.fileEncoding == null)
                {
                    this.fileEncoding = streamReader.CurrentEncoding;
                }

                entireFile = streamReader.ReadToEnd();
            }

            // Parse the entire file.
            string newFile = this.parseRegex.Replace(entireFile, this.Replacement);

            if (newFile != entireFile)
            {
                // First make sure the file is writable.
                FileAttributes fileAttributes = System.IO.File.GetAttributes(parseFile);
                bool changedAttribute = false;

                // If readonly attribute is set, reset it.
                if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Making File Writeable: {0}", parseFile));
                    System.IO.File.SetAttributes(parseFile, fileAttributes ^ FileAttributes.ReadOnly);
                    changedAttribute = true;
                }

                // Set TextEncoding if it was specified.
                if (string.IsNullOrEmpty(this.TextEncoding) == false)
                {
                    try
                    {
                        this.fileEncoding = System.Text.Encoding.GetEncoding(this.TextEncoding);
                    }
                    catch (ArgumentException)
                    {
                        Log.LogError(string.Format(CultureInfo.CurrentCulture, "{0} is not a supported encoding name.", this.TextEncoding));
                        return;
                    }
                }

                // Write out the new file.
                using (StreamWriter streamWriter = new StreamWriter(parseFile, false, this.fileEncoding))
                {
                    streamWriter.Write(newFile);
                }

                if (changedAttribute)
                {
                    this.LogTaskMessage(MessageImportance.Low, "Making file readonly");
                    System.IO.File.SetAttributes(parseFile, FileAttributes.ReadOnly);
                }
            }
        }
    }
}
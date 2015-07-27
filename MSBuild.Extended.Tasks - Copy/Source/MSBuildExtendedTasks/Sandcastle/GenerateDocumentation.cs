/* Copyright (c) 2007, Ryan Conrad
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * - Redistributions of source code must retain the above copyright notice, this list of conditions 
 *   and the following disclaimer.
 *   
 * - Redistributions in binary form must reproduce the above copyright notice, this list of conditions 
 *   and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * - Neither the name of the Camalot Designs nor the names of its contributors may be used to endorse 
 *   or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED 
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR 
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
 * TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF 
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using System.Reflection;
using Microsoft.Build.Utilities;
using System.Xml;
using System.Text.RegularExpressions;

namespace MSBuild.Extended.Tasks.Sandcastle {
  /// <summary>
  /// Uses sandcastle to generate documentation from assemblies and the xml comments file.
  /// </summary>
  /// <example>
  /// <code language="xml" htmlDecode="true" title="MSBuild"><![CDATA[<GenerateDocumentation SandcastlePath="C:\Program Files\Sandcastle" HelpName="MSBuild.Extended.Tasks"
  /// OutputPath=$(MSBuildProjectDirectory)Help" Assemblies="$(MSBuildProjectDirectory)MSBuild.Extended.Tasks.dll"
  /// Dependencies="@(DependencyFiles)" XmlDocumentationFiles="@(CommentFiles)" BuildCHM="True"
  /// DocumentStyle="Standard" PresentationType="VS2005" FileNameStyle="Friendly" 
  /// CleanUpDocumentationFiles="True" DeleteHtml="False" UseCodeSyntaxHighlighterComponent="True"
  /// LocalReferenceLinkResolveType="Local" ExternalReferenceLinkResolveType="MSDN"
  /// ReflectionToHtmlTocTransform="$(MSBuildExtensionsPath)\MSBuildExtendedTasks\Sandcastle\Transforms\ReflectionToHtmlToc.xsl" />]]>
  /// </code>
  /// <para>Here is an example if creating a frame index page that will be used to show the documentation via a browser.</para>
  /// <code language="xml" htmlDecode="true" title="Create Frames Index page"><![CDATA[<AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="html" />
  ///  <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="head" XPath="/html" />
  ///  <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="title" XPath="/html/head" />
  ///  <SetXmlElementInnerText XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" XPath="/html/head/title"
  ///                          InnerText="MSBuild.Extended.Tasks" />
  ///  <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="frameset" XPath="/html" />
  ///  <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="cols" XPath="/html/frameset"
  ///                          Value="25&#37;,75&#37;" />
  ///  <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="frame" XPath="/html/frameset" />
  ///  <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="name" XPath="/html/frameset/frame[1]"
  ///                          Value="tocFrame" />
  ///  <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="src" XPath="/html/frameset/frame[1]"
  ///                          Value="toc.htm" />
  ///  <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="frame" XPath="/html/frameset" />
  ///  <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="name" XPath="/html/frameset/frame[2]"
  ///                          Value="contentFrame" />
  ///  <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="src" XPath="/html/frameset/frame[2]"
  ///                          Value="R_Project.htm" />]]></code>
  /// <note>
  /// <para><c>ReflectionToHtmlTocTransform</c> should be set in order to create the <c>toc.htm</c> file.</para>
  /// </note>
  /// <span style="white-space:pre-wrap;">It is possible to generate documentation for the main page that contains the namespace list generated by Sandcastle by creating<br />
  /// a ProjectDoc class outside of any namespace. Below is an example ProjectDoc class. Also in the example is a NamespaceDoc class. This<br /> 
  /// class works like the NamespaceDoc that was introduced by NDoc. A NamespaceDoc class is looked for in each namespace. If one is found<br />
  /// the comments for that NamespaceDoc class will be used for the Namespace documentation.</span>
/// <code language="C#" htmlDecode="true" title="ProjectDoc Example"><![CDATA[
/// using System;
///
/// /// <summary>
/// /// The aim of this project is to create a large collection of MSBuild tasks that can be used to perform 
/// /// actions to further automate the build process.
/// /// </summary>
/// /// <example>
/// [Browsable ( false ), EditorBrowsable ( EditorBrowsableState.Never )]
/// class ProjectDoc { }
/// 
/// namespace MSBuild.Extended.Tasks.Sandcastle {
///   /// <summary>
///   /// Contains Tasks that simplifiy documenting assemblies using Sandcastle.
///   /// </summary>
///   [ Browsable( false ), EditorBrowsable( EditorBrowsableState.Never ) ]
///   class NamespaceDoc { }
/// }
/// 
/// namespace MSBuild.Extended.Tasks.Net {
///   /// <summary>
///   /// Contains Tasks that pertain to things releated to internet and network activities.
///   /// </summary>
///   [ Browsable( false ), EditorBrowsable( EditorBrowsableState.Never ) ]
///   class NamespaceDoc { }
/// }
/// ]]></code>
  /// </example>
  public class GenerateDocumentation : Task {

    private string _sandcastlePath = string.Empty;
    private string _sandcastleBin = @"ProductionTools";
    private string _sandcastleTransforms = @"ProductionTransforms";
    private string _sandcastleOutputStructure = @"Presentation";
    private bool _useCodeColorizerComponent = false;
    private bool _supportNamespaceDocClass = true;
    private string _reflectionToHtmlTocXsl = string.Empty;

    private FileNameStyle _fileNameStyle = Sandcastle.FileNameStyle.Guid;
    private DocumentStyle _documentStyle = Sandcastle.DocumentStyle.Standard;
    private PresentationType _presentationStyle = Sandcastle.PresentationType.VS2005;
    private ReferenceResolveType _referencesResolveType = ReferenceResolveType.MSDN;
    private ReferenceResolveType _localResolveType = ReferenceResolveType.Local;

    private string _namespaceDocName = "NamespaceDoc";
    private string _projectDocName = "ProjectDoc";

    private string[] _dependencies = null;
    private string[] _assemblies = null;
    private string[] _documentationXmlFiles = null;

    private bool _buildCHM = true;
    private bool _buildHxS = false;

    private string _helpCompiler2BinPath = @"C:\Program Files\Microsoft Help 2.0 SDK";
    private string _htmlHelpCompilerBinPath = @"C:\Program Files\HTML Help Workshop";

    private string _helpDocName = string.Empty;
    private string _outputPath = string.Empty;

    private bool _deleteHtml = false;
    private bool _cleanUpFiles = true;

    /// <summary>
    /// Gets or sets the sandcastle path.
    /// </summary>
    /// <value>The sandcastle path.</value>
    [Required]
    public string SandcastlePath { get { return this._sandcastlePath; } set { this._sandcastlePath = value; } }

    /// <summary>
    /// Gets or sets the file name style.
    /// </summary>
    /// <value>The file name style.</value>
    public string FileNameStyle {
      get { return this._fileNameStyle.ToString (); }
      set { this._fileNameStyle = string.IsNullOrEmpty ( value ) ? Sandcastle.FileNameStyle.Guid : (FileNameStyle)StringToEnum ( value, typeof ( Sandcastle.FileNameStyle ) ); }
    }

    /// <summary>
    /// Gets or sets the document style.
    /// </summary>
    /// <value>The document style.</value>
    public string DocumentStyle {
      get { return this._documentStyle.ToString (); }
      set {
        this._documentStyle = string.IsNullOrEmpty ( value ) ? Sandcastle.DocumentStyle.VisualStudio :
          (DocumentStyle)StringToEnum ( value, typeof ( Sandcastle.DocumentStyle ) );
      }
    }

    /// <summary>
    /// Gets or sets the type of the presentation.
    /// </summary>
    /// <value>The type of the presentation.</value>
    public string PresentationType {
      get { return this._presentationStyle.ToString (); }
      set {
        this._presentationStyle = string.IsNullOrEmpty ( value ) ? Sandcastle.PresentationType.VS2005 :
          (PresentationType)StringToEnum ( value, typeof ( Sandcastle.PresentationType ) );
      }
    }

    /// <summary>
    /// Gets or sets the way to resolve the local reference links.
    /// </summary>
    /// <value>How to resolve references to local items.</value>
    public string LocalReferenceLinkResolveType {
      get { return this._localResolveType.ToString ().ToLower (); }
      set {
        this._localResolveType = string.IsNullOrEmpty ( value ) ? ReferenceResolveType.Local :
          (ReferenceResolveType)StringToEnum ( value, typeof ( ReferenceResolveType ) );
      }
    }

    /// <summary>
    /// Gets or sets the way to resolve the external reference links.
    /// </summary>
    /// <value>How to resolve references to external items.</value>
    public string ExternalReferenceLinkResolveType {
      get { return this._referencesResolveType.ToString ().ToLower (); }
      set {
        this._referencesResolveType = string.IsNullOrEmpty ( value ) ? ReferenceResolveType.Local :
          (ReferenceResolveType)StringToEnum ( value, typeof ( ReferenceResolveType ) );
      }
    }

    /// <summary>
    /// Gets or sets the name of the help.
    /// </summary>
    /// <value>The name of the help.</value>
    [Required]
    public string HelpName { get { return this._helpDocName; } set { this._helpDocName = value; } }

    /// <summary>
    /// Gets or sets the help output path.
    /// </summary>
    /// <value>The help output path.</value>
    [Required]
    public string OutputPath { get { return this._outputPath; } set { this._outputPath = value; } }

    /// <summary>
    /// Gets or sets the HTML help compiler bin path.
    /// </summary>
    /// <value>The HTML help compiler bin path.</value>
    public string HtmlHelpCompilerBinPath { get { return this._htmlHelpCompilerBinPath; } set { this._htmlHelpCompilerBinPath = value; } }

    /// <summary>
    /// Gets or sets the HX compiler bin path.
    /// </summary>
    /// <value>The HX compiler bin path.</value>
    public string HXCompilerBinPath { get { return this._helpCompiler2BinPath; } set { this._helpCompiler2BinPath = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether [build CHM].
    /// </summary>
    /// <value><c>true</c> if [build CHM]; otherwise, <c>false</c>.</value>
    public bool BuildCHM { get { return this._buildCHM; } set { this._buildCHM = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether [build hx S].
    /// </summary>
    /// <value><c>true</c> if [build hx S]; otherwise, <c>false</c>.</value>
    public bool BuildHxS { get { return this._buildHxS; } set { this._buildHxS = value; } }

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    /// <value>The dependencies.</value>
    public string[] Dependencies { get { return this._dependencies; } set { this._dependencies = value; } }

    /// <summary>
    /// Gets or sets the assemblies.
    /// </summary>
    /// <value>The assemblies.</value>
    [Required]
    public string[] Assemblies { get { return this._assemblies; } set { this._assemblies = value; } }

    /// <summary>
    /// Gets or sets the XML documentation files.
    /// </summary>
    /// <value>The XML documentation files.</value>
    [Required]
    public string[] XmlDocumentationFiles { get { return this._documentationXmlFiles; } set { this._documentationXmlFiles = value; } }

    /// <summary>
    /// Gets or sets the reflection to HTML toc transform.
    /// </summary>
    /// <value>The reflection to HTML toc transform.</value>
    public string ReflectionToHtmlTocTransform { get { return this._reflectionToHtmlTocXsl; } set { this._reflectionToHtmlTocXsl = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether [clean up documentation files].
    /// </summary>
    /// <value>
    /// 	if <c>true</c>, delete all sandcastle releated files.
    /// </value>
    public bool CleanUpDocumentationFiles { get { return this._cleanUpFiles; } set { this._cleanUpFiles = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether to use code syntax highlighter component.
    /// </summary>
    /// <value>
    /// 	<c>true</c>, use code syntax highlighter component.
    /// </value>
    public bool UseCodeSyntaxHighlighterComponent { get { return this._useCodeColorizerComponent; } set { this._useCodeColorizerComponent = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether to delete HTML files generated by sandcastle.
    /// </summary>
    /// <value><c>true</c> if [delete HTML]; otherwise, <c>false</c>.</value>
    public bool DeleteHtml { get { return this._deleteHtml; } set { this._deleteHtml = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether to support the NDoc style of defining info for a namespace.
    /// </summary>
    /// <value>
    /// 	if <c>true</c> then an action will be performed that will modify the XML document files where the NamespaceDoc class exists. This class
    /// value will be applied as the info for the namespace. Setting this to <c>true</c> will also enable support for the
    /// ProjectDoc class.
    /// </value>
    /// <seealso cref="P:MSBuild.Extended.Tasks.Sandcastle.GenerateDocumentation.NamespaceDocClassName"/>
    /// <seealso cref="P:MSBuild.Extended.Tasks.Sandcastle.GenerateDocumentation.ProjectDocClassName"/>
    public bool SupportNamespaceDocClass { get { return this._supportNamespaceDocClass; } set { this._supportNamespaceDocClass = value; } }

    /// <summary>
    /// Gets or sets the name of the class to use for the namespace documentation.
    /// </summary>
    /// <value>The name of the namespace doc class.</value>
    /// <remarks>Default: <c>NamespaceDoc</c></remarks>
    public string NamespaceDocClassName { get { return this._namespaceDocName; } set { this._namespaceDocName = value; } }
    /// <summary>
    /// Gets or sets the name of the class to use for the project documentation.
    /// </summary>
    /// <value>The name of the project doc class.</value>
    /// <remarks>Default: <c>ProjectDoc</c></remarks>
    public string ProjectDocClassName { get { return this._projectDocName; } set { this._projectDocName = value; } }
    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>
    /// true if the task executed successfully; otherwise, false.
    /// </returns>
    public override bool Execute () {
      SandcastleHelper.BuildEngine = this.BuildEngine;

      if ( !Directory.Exists ( OutputPath ) )
        Directory.CreateDirectory ( this.OutputPath );
      SandcastleHelper.EmptyOutputStructure ( this.OutputPath, Path.Combine ( this.SandcastlePath, this._sandcastleOutputStructure ), this._presentationStyle );

      string bin = Path.Combine ( this.SandcastlePath, _sandcastleBin );
      string transforms = Path.Combine ( this.SandcastlePath, _sandcastleTransforms );
      SandcastleHelper.BuildReflectionOutput ( bin, this._sandcastleTransforms,
            ArrayToString ( this.Assemblies, @"""{0}"" " ), ArrayToString ( this.Dependencies, @"/dep:""{0}"" " ),
            this._documentStyle, this._fileNameStyle, this.OutputPath );

      SandcastleHelper.CreateManifest ( bin, this._sandcastleTransforms, this.OutputPath );
      SandcastleHelper.CreateSandcastleConfig ( this.SandcastlePath, Path.Combine ( this.SandcastlePath, this._sandcastleOutputStructure ),
        this._presentationStyle, this.OutputPath, XmlDocumentationFiles, this._localResolveType, this._referencesResolveType );

      Assembly asm = typeof ( GenerateDocumentation ).Assembly;
      string path = Path.GetDirectoryName ( asm.Location );

      if ( SupportNamespaceDocClass ) {
        //SandcastleHelper.AddNamespaceDocComponentToSandcastleConfig ( Path.Combine ( path, "MSBuildExtendedTasks.CodeHighligherSandcastleComponent.dll" ), Path.Combine ( this.OutputPath, "sandcastle.config" ), XmlDocumentationFiles );
        List<FileInfo> files = new List<FileInfo> ();
        foreach ( string xdfile in XmlDocumentationFiles ) {
          DirectoryInfo dir = new DirectoryInfo ( Path.GetDirectoryName ( xdfile ) );
          string pattern = Path.GetFileName ( xdfile );
          foreach ( FileInfo file in dir.GetFiles ( pattern ) ) {
            files.Add ( file );
          }
        }

        bool change = false;
        bool projFound = false;
        foreach ( FileInfo file in files ) {
          XmlDocument doc = new XmlDocument ();
          doc.Load ( file.FullName );
          XmlNodeList memberNodes = doc.SelectNodes ( "//member[@name]" );
          foreach ( XmlElement memberElement in memberNodes ) {
            string pattern = string.Format ( @"^T:(.*?)\.{0}$", this.NamespaceDocClassName );
            Regex regex = new Regex ( pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace );
            Match match = regex.Match ( memberElement.GetAttribute ( "name" ) );
            if ( match.Success ) {
              change = true;
              memberElement.SetAttribute ( "name", match.Result ( "N:$1" ) );
            }
            if ( !projFound ) {
              string projPattern = string.Format ( @"^T:{0}$", this.ProjectDocClassName );
              Regex projRegex = new Regex ( projPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace );
              Match projMatch = projRegex.Match ( memberElement.GetAttribute ( "name" ) );
              if ( projMatch.Success ) {
                this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( "Found Project Doc", string.Empty, this.GetType ().FullName, MessageImportance.Normal ) );
                change = true;
                memberElement.SetAttribute ( "name", projMatch.Result ( "R:Project" ) );
                // since only 1 "R:Project" can exist we break;
              }
            }
          }
          if ( change )
            doc.Save ( file.FullName );
        }

      }
      if ( UseCodeSyntaxHighlighterComponent ) {
        SandcastleHelper.AddCodeHighlighterToSandcastleConfig ( Path.Combine ( path, "MSBuild.Extended.Tasks.Components.dll" ), Path.Combine ( this.OutputPath, "sandcastle.config" ) );
        SandcastleHelper.CopyFilesFromDirectory ( Path.Combine ( path, @"Sandcastle\ProductionTools\Images\OutliningIndicators" ), "*.gif", Path.Combine ( this.OutputPath, "icons" ) );
      }
      SandcastleHelper.BuildHtml ( bin, "sandcastle.config", this.OutputPath );

      if ( !string.IsNullOrEmpty ( this.ReflectionToHtmlTocTransform ) )
        SandcastleHelper.CustomTransform ( bin, this.ReflectionToHtmlTocTransform, this.OutputPath, "toc.htm" );

      if ( this.BuildCHM ) {
        SandcastleHelper.CreateTOC ( bin, transforms, _documentStyle, OutputPath );
        SandcastleHelper.GenerateHHTOC ( bin, transforms, this.OutputPath, this.HelpName );
        SandcastleHelper.CompileCHM ( this.HtmlHelpCompilerBinPath, this.OutputPath, this.HelpName );
      }

      if ( this.BuildHxS ) {
        SandcastleHelper.CreateHXFiles ( bin, _documentStyle, this.OutputPath, this.HelpName );
        SandcastleHelper.GenerateHCTOC ( bin, transforms, this.OutputPath, this.HelpName );
        SandcastleHelper.CompileHX ( this.HXCompilerBinPath, this.OutputPath, this.HelpName );
      }

      if ( this.CleanUpDocumentationFiles ) {
        foreach ( FileInfo file in new DirectoryInfo ( this.OutputPath ).GetFiles ( "*.*" ) ) {
          switch ( Path.GetFileName ( file.FullName ).ToLower () ) {
            case "manifest.xml":
            case "reflection.org":
            case "reflection.xml":
            case "sandcastle.config":
              file.Delete ();
              break;
            default:
              switch ( Path.GetExtension ( file.FullName ).ToLower () ) {
                case ".hhc":
                case ".hhk":
                case ".hhp":
                  file.Delete ();
                  break;
              }
              break;
          }
        }
      }

      if ( this.DeleteHtml ) {
        DirectoryInfo dir = new DirectoryInfo ( Path.Combine ( this.OutputPath, "html" ) );
        if ( dir.Exists )
          dir.Delete ( true );
        dir = new DirectoryInfo ( Path.Combine ( this.OutputPath, "icons" ) );
        if ( dir.Exists )
          dir.Delete ( true );
        dir = new DirectoryInfo ( Path.Combine ( this.OutputPath, "scripts" ) );
        if ( dir.Exists )
          dir.Delete ( true );
        dir = new DirectoryInfo ( Path.Combine ( this.OutputPath, "styles" ) );
        if ( dir.Exists )
          dir.Delete ( true );
      }

      return true;

    }

    /// <summary>
    /// Array to string.
    /// </summary>
    /// <param name="strArray">The string array.</param>
    /// <param name="format">The format.</param>
    /// <returns></returns>
    private string ArrayToString ( string[] strArray, string format ) {
      if ( strArray == null )
        return string.Empty;
      StringBuilder outString = new StringBuilder ();
      foreach ( string s in strArray ) {
        outString.AppendFormat ( format, s );
      }
      return outString.ToString ().Trim ();
    }

    /// <summary>
    /// converts a strings to an enum.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="enumType">Type of the enum.</param>
    /// <returns></returns>
    private object StringToEnum ( string name, Type enumType ) {
      return Enum.Parse ( enumType, name );
    }

  }
}

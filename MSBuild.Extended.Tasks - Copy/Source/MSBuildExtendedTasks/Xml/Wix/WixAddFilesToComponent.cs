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
using Microsoft.Build.Utilities;
using System.Xml;
using Microsoft.Build.Framework;
using System.IO;

namespace MSBuild.Extended.Tasks.Xml.Wix {
  /// <summary>
  /// Provides a way to dynamically generate file elements in a component defined in a Wix file.
  /// </summary>
  /// <example><code language="xml" htmlDecode="true" title="MSBuild"><![CDATA[<PropertyGroup>
  /// 	<WixFile>$(MSBuildProjectDirectory)\files.wxs</WixFile>
  /// 	<ComponentGuid>{04E016C1-6985-445C-A8E9-6B82B82C4B9B}</ComponentGuid>
  /// 	<BaseXPath>//wix:Component[@Guid='$(ComponentGuid)']</BaseXPath>
  /// 	<Namespace>http://schemas.microsoft.com/wix/2006/wi</Namespace>
  /// 	<NamespaceManager>wix:$(Namespace)</NamespaceManager>
  /// </PropertyGroup>
  /// 
  /// <Target Name="CreateWixFile">
  /// 	<CreateItem Include="$(OutputPath)**\*.*">
  /// 		<Output TaskParameter="Include" ItemName="WixFiles" />
  /// 	</CreateItem>
  /// 	
  /// 	<RemoveXmlNode XmlFile="$(WixFile)" ParentNodeXPath="$(BaseXPath)" 
  /// 		RemoveNodeXPath="wix:File" NamespaceURI="$(Namespace)" 
  /// 		Namespaces="$(NamespaceManager)" />
  /// 	<RemoveXmlNode XmlFile="$(WixFile)" ParentNodeXPath="$(BaseXPath)" 
  /// 		RemoveNodeXPath="wix:Directory" NamespaceURI="$(Namespace)" 
  /// 		Namespaces="$(NamespaceManager)" />
  ///     
  /// 	<WixAddFilesToComponent Files="@(WixFiles)" XPath="$(BaseXPath)" 
  /// 		NamespaceURI="$(Namespace)" Namespaces="$(NamespaceManager)" 
  /// 		XmlFile="$(WixFile)" Compressed="True" Vital="True" />
  /// </Target>
  /// ]]></code>
  /// 
  /// </example>
  public class WixAddFilesToComponent : AbstractXmlTask {
    private string[] _files = null;
    private string _baseDirectory = string.Empty;

    private bool? _compressed = null;
    private int _disk = 1;
    private bool? _keyPath = null;
    private bool? _vital = null;
    private int? _patchGroup = null;
    private bool? _system = null;
    private bool? _readonly = null;
    private bool? _hidden = null;
    //private string _installDirId = "INSTALLDIR";
    /// <summary>
    /// Initializes a new instance of the <see cref="WixAddFilesToComponent"/> class.
    /// </summary>
    public WixAddFilesToComponent () {
      this.NamespaceURI = "http://schemas.microsoft.com/wix/2006/wi";
      this.Namespaces = new string[] { string.Format ( "wix:{0}", this.NamespaceURI ) };
    }

    /// <summary>
    /// Gets or sets the files.
    /// </summary>
    /// <value>The files.</value>
    [Required]
    public string[] Files { get { return this._files; } set { this._files = value; } }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public override string Name { get { return base.Name; } set { base.Name = value; } }
    /// <summary>
    /// Gets or sets the XPath.
    /// </summary>
    /// <value>The XPath.</value>
    public override string XPath { get { return base.XPath; } set { base.XPath = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether files should be marked compressed.
    /// </summary>
    /// <value><c>true</c> if compressed; otherwise, <c>false</c>.</value>
    public bool? Compressed { get { return this._compressed; } set { this._compressed = value; } }

    /// <summary>
    /// Gets or sets the disk.
    /// </summary>
    /// <value>The disk.</value>
    public int Disk { get { return this._disk; } set { this._disk = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether the file should be markes as key path.
    /// </summary>
    /// <value><c>true</c> if key path; otherwise, <c>false</c>.</value>
    public bool? KeyPath { get { return this._keyPath; } set { this._keyPath = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether the file should be marked as vital.
    /// </summary>
    /// <value><c>true</c> if vital; otherwise, <c>false</c>.</value>
    public bool? Vital { get { return this._vital; } set { this._vital = value; } }

    /// <summary>
    /// Gets or sets if the files should be marked hidden.
    /// </summary>
    /// <value>if <c>true</c>, the files will be marked as hidden.</value>
    public bool? Hidden { get { return this._hidden; } set { this._hidden = value; } }

    /// <summary>
    /// Gets or sets if the file is a system file.
    /// </summary>
    /// <value>if <c>true</c>, the files will be marked as system files.</value>
    public bool? SystemFile { get { return this._system; } set { this._system = value; } }

    /// <summary>
    /// Gets or sets if the files should be marked readonly
    /// </summary>
    /// <value>if <c>true</c>, the files will be marked read only.</value>
    public bool? ReadOnly { get { return this._readonly; } set { this._readonly = value; } }

    /*/// <summary>
    /// Gets or sets the install directory id.
    /// </summary>
    /// <value>The install directory id.</value>
    [ Required ]
    public string InstallDirectoryId { get { return this._installDirId; } set { this._installDirId = value; } }*/
    /// <summary>
    /// When overridden in a derived class, executes the task.
    /// </summary>
    /// <returns>
    /// true if the task successfully executed; otherwise, false.
    /// </returns>
    public override bool Execute () {
      if ( !File.Exists ( this.XmlFile ) ) {
        this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( string.Format ( "File {0} was not found.", this.XmlFile ), string.Empty, this.GetType ().FullName, MessageImportance.High ) );
        throw new FileNotFoundException ( string.Format ( "File {0} was not found.", this.XmlFile ) );
      }

      _document = new XmlDocument ();
      _document.Load ( this.XmlFile );

      XmlNode componentNode = XPathQuery ( this.XPath );
      if ( componentNode == null ) {
        throw new XmlException ( "Component element was not found" );
      } else {
        foreach ( string filePattern in this.Files )
          AddFilePattern ( componentNode, filePattern );
      }
      _document.Save ( this.XmlFile );
      return true;
    }

    /// <summary>
    /// Adds the file pattern.
    /// </summary>
    /// <param name="parentElement">The parent element.</param>
    /// <param name="filePattern">The file pattern.</param>
    private void AddFilePattern ( XmlNode parentElement, string filePattern ) {
      string dir = string.Format ( "{0}\\", Path.GetDirectoryName ( filePattern ) );
      if ( string.IsNullOrEmpty ( this._baseDirectory ) )
        this._baseDirectory = string.Format ( "{0}{1}", dir, dir.EndsWith ( "\\" ) ? string.Empty : "\\" );
      string shortDir = dir.Replace ( _baseDirectory, string.Empty );
      string pattern = Path.GetFileName ( filePattern );
      this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( string.Format ( "shortDir = {0}.", shortDir ), string.Empty, this.GetType ().FullName, MessageImportance.High ) );
      //XmlElement installDirNode = XPath( string.Format("//wix:Directory[@Id=\"{0}\"]",this.InstallDirectoryId );
      //XmlElement trueParentNode = AddDirectories ( parentElement, shortDir ) as XmlElement;
      foreach ( FileInfo file in new DirectoryInfo ( dir ).GetFiles ( pattern ) ) {
        XmlElement fileNode = _document.CreateElement ( "File", this.NamespaceURI );
        fileNode.SetAttribute ( "Id", file.Name );
        fileNode.SetAttribute ( "Name", file.Name );
        fileNode.SetAttribute ( "DiskId", this.Disk.ToString () );
        if ( this.Compressed.HasValue )
          fileNode.SetAttribute ( "Compressed", this.Compressed.Value ? "yes" : "no" );
        if ( this.KeyPath.HasValue )
          fileNode.SetAttribute ( "KeyPath", this.KeyPath.Value ? "yes" : "no" );
        if ( this.Vital.HasValue )
          fileNode.SetAttribute ( "Vital", this.Vital.Value ? "yes" : "no" );
        fileNode.SetAttribute ( "Source", string.Format ( "{0}{1}{2}", _baseDirectory, shortDir, file.Name ) );
        parentElement.AppendChild ( fileNode );
      }
    }

    /*/// <summary>
    /// Adds the directories.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="dir">The dir.</param>
    /// <returns></returns>
    private XmlNode AddDirectories ( XmlNode parentNode, string dir ) {
      if ( string.IsNullOrEmpty ( dir ) )
        return parentNode;
      string[] ids = dir.Split ( new char[] { '\\' }, 2, StringSplitOptions.RemoveEmptyEntries );
      XmlElement dirNode = XPathQuery ( string.Format ( "wix:Directory[@Id=\"{0}\"]", ids[0] ), parentNode ) as XmlElement;
      if ( dirNode == null ) {
        dirNode = _document.CreateElement ( "Directory", this.NamespaceURI );
        dirNode.SetAttribute ( "Id", ids[0] );
        dirNode.SetAttribute ( "Name", ids[0] );
        parentNode.AppendChild ( dirNode );
      }

      if ( ids.Length > 1 )
        return AddDirectories ( dirNode, ids[1] );
      else
        return dirNode;
    }*/
  }
}

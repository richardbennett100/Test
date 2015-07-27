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
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Extended.Tasks.Xml {
  /// <summary>
  /// Removes an XmlNode from a parent node.
  /// </summary>
  /// <example>
  /// <code language="xml" htmlDecode="true"><![CDATA[<RemoveXmlNode XmlFile="$(XmlFilePath)" RemoveNodeXPath="Import2" ParentNodeXPath="/Project" />]]></code>
  /// </example>
  public class RemoveXmlNode : Task {
    private string xmlFile = string.Empty;
    private string pnXPath = string.Empty;
    private string rnXPath = string.Empty;
    private string[] _namespaces = null;
    private XmlDocument _document = null;
    private string xmlNamespace = string.Empty;
    /// <summary>
    /// The full path to the xml file.
    /// </summary>
    /// <value>The XML file.</value>
    [Required]
    public string XmlFile { get { return this.xmlFile; } set { this.xmlFile = value; } }

    /// <summary>
    /// Gets or sets the parent node XPath.
    /// </summary>
    /// <value>The parent node XPath.</value>
    [ Required ]
    public string ParentNodeXPath { get { return this.pnXPath; } set { this.pnXPath = value; } }

    /// <summary>
    /// Gets or sets the remove node XPath.
    /// </summary>
    /// <value>The remove node XPath.</value>
    [ Required ]
    public string RemoveNodeXPath { get { return this.rnXPath; } set { this.rnXPath = value; } }

    /// <summary>
    /// Gets or sets the default namespace URI.
    /// </summary>
    /// <value>The namespace URI.</value>
    public string NamespaceURI { get { return this.xmlNamespace; } set { this.xmlNamespace = value; } }

    /// <summary>
    /// Array of <c>Prefix=NamespaceURI</c> strings.
    /// </summary>
    /// <value>The namespaces.</value>
    /// <remarks>These will be added to the <see cref="System.Xml.XmlNamespaceManager">XmlNamespaceManager</see>.
    /// You should not add a default namespace here. Use <see cref="P:MSBuild.Extended.Tasks.Xml.AbstractXmlTask.NamespaceURI">NamespaceURI</see> and leave
    /// <see cref="P:MSBuild.Extended.Tasks.Xml.AbstractXmlTask.Prefix">Prefix</see> empty.</remarks>
    public string[] Namespaces { get { return this._namespaces; } set { this._namespaces = value; } }

    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>
    /// true if the task executed successfully; otherwise, false.
    /// </returns>
    /// <exception cref="System.IO.FileNotFoundException" />
    /// <exception cref="System.Xml.XmlException" />
    public override bool Execute () {
      if ( !File.Exists ( this.XmlFile ) ) {
        if ( this.BuildEngine != null )
          this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( string.Format ( "The file {0} was not found.",this.XmlFile), string.Empty, this.GetType ().FullName, MessageImportance.High ) );
        throw new FileNotFoundException ( string.Format ( "The file {0} was not found.", this.XmlFile ) );
      }
      try {
        _document = new XmlDocument ( );
        _document.Load ( this.XmlFile );
        XmlNodeList pNodes = XPathQuery ( this.ParentNodeXPath );
        if ( pNodes == null || pNodes.Count == 0 ) {
        if ( this.BuildEngine != null )
          this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( "XPath did not return a valid node.", string.Empty, this.GetType ().FullName, MessageImportance.High ) );
          throw new XmlException ( "XPath did not return a valid node." );
        }

        foreach ( XmlNode pNode in pNodes ) {
          XmlNodeList rNodes = XPathQuery ( this.RemoveNodeXPath, pNode );
          foreach ( XmlNode rNode in rNodes ) {
            if ( rNode == null ) {
              BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( "Node to remove was not found from the supplied XPath", string.Empty, this.GetType ().Name, MessageImportance.Normal ) );
              return false;
            }
            pNode.RemoveChild ( rNode );
          }
        }
        _document.Save ( this.XmlFile );
      } catch ( Exception ex ) {
        if ( this.BuildEngine != null )
          this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ex.ToString(  ), string.Empty, this.GetType ().FullName, MessageImportance.High ) );

        throw;
      }
      return true;
    }

    /// <summary>
    /// Performs an XPath query
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns></returns>
    protected virtual XmlNodeList XPathQuery ( string query )  {
      XmlNodeList nodes = XPathQuery ( query, _document );
      return nodes;
    }

    /// <summary>
    /// Performs an XPath query
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="baseNode">The base node.</param>
    /// <returns></returns>
    protected virtual XmlNodeList XPathQuery ( string query, XmlNode baseNode ) {
      XmlNodeList nodes = baseNode.SelectNodes ( query, CreateXmlNamespaceManager ( ) );
      return nodes;
    }

    /// <summary>
    /// Creates the XML namespace manager.
    /// </summary>
    /// <returns></returns>
    private XmlNamespaceManager CreateXmlNamespaceManager ( ) {
      XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager ( this._document.NameTable );
      if ( !string.IsNullOrEmpty ( this.NamespaceURI ) )
        xmlNamespaceManager.AddNamespace ( string.Empty, this.NamespaceURI );

      if ( this.Namespaces != null ) {
        foreach ( string pn in this.Namespaces ) {
          string[] tpn = pn.Split ( new char[] { ':' }, 2 );
          if ( !string.IsNullOrEmpty ( tpn[ 0 ] ) ) {
            if ( !xmlNamespaceManager.HasNamespace ( tpn[ 0 ] ) ) {
              this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( string.Format ( "Adding namespace {0}:{1} to namespace manager", tpn[ 0 ], tpn[ 1 ] ), string.Empty, this.GetType ( ).FullName, MessageImportance.Normal ) );
              xmlNamespaceManager.AddNamespace ( tpn[ 0 ], tpn[ 1 ] );
            }
          }
        }
      }

      return xmlNamespaceManager;
    }
  }
}

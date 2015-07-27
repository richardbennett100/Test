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
using Microsoft.Build.Utilities;
using System.Xml;

namespace MSBuild.Extended.Tasks.Xml {
  /// <summary>
  /// A base class for Xml Tasks
  /// </summary>
  public abstract class AbstractXmlTask : Task {
    /// <summary>
    /// The path to the xml file
    /// </summary>
    protected string _xmlFile = string.Empty;
    /// <summary>
    /// The xpath
    /// </summary>
    protected string _xpath = string.Empty;
    /// <summary>
    /// the namespace prefix
    /// </summary>
    protected string _prefix = string.Empty;
    /// <summary>
    /// the namespace uri.
    /// </summary>
    protected string _xmlNamespace = string.Empty;
    /// <summary>
    /// the node name
    /// </summary>
    protected string _name = string.Empty;
    /// <summary>
    /// The <see cref="System.Xml.XmlDocument">XmlDocument</see> loaded
    /// </summary>
    protected XmlDocument _document = null;

    /// <summary>
    /// Array of prefix:namespace values for the namespace manager
    /// </summary>
    protected string[] _namespaces = null;
    /// <summary>
    /// Gets or sets the XML file.
    /// </summary>
    /// <value>The XML file.</value>
    [Required]
    public string XmlFile { get { return this._xmlFile; } set { this._xmlFile = value; } }

    /// <summary>
    /// Gets or sets the XPath.
    /// </summary>
    /// <value>The XPath.</value>
    public virtual string XPath { get { return this._xpath; } set { this._xpath = value; } }

    /// <summary>
    /// Gets or sets the prefix.
    /// </summary>
    /// <value>The prefix.</value>
    public string Prefix { get { return this._prefix; } set { this._prefix = value; } }

    /// <summary>
    /// Gets or sets the namespace URI.
    /// </summary>
    /// <value>The namespace URI.</value>
    public string NamespaceURI { get { return this._xmlNamespace; } set { this._xmlNamespace = value; } }

    /// <summary>
    /// Array of <c>Prefix:NamespaceURI</c> strings.
    /// </summary>
    /// <value>The namespaces.</value>
    /// <remarks>These will be added to the <see cref="System.Xml.XmlNamespaceManager">XmlNamespaceManager</see>.
    /// You should not add a default namespace here. Use <see cref="P:MSBuild.Extended.Tasks.Xml.AbstractXmlTask.NamespaceURI">NamespaceURI</see> and leave
    /// <see cref="P:MSBuild.Extended.Tasks.Xml.AbstractXmlTask.Prefix">Prefix</see> empty.</remarks>
    public string[] Namespaces { get { return this._namespaces; } set { this._namespaces = value; } }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [Required]
    public virtual string Name { get { return this._name; } set { this._name = value; } }

    /// <summary>
    /// When overridden in a derived class, executes the task.
    /// </summary>
    /// <returns>
    /// true if the task successfully executed; otherwise, false.
    /// </returns>
    public abstract override bool Execute ( );

    /// <summary>
    /// Performs an XPath query
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="startNode">The start node.</param>
    /// <returns></returns>
    protected virtual XmlNode XPathQuery ( string query, XmlNode startNode ) {
      XmlNode baseNode = startNode;
      XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager ( this._document.NameTable );
      //this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs (string.Format("NamespaceURI = {0}", this.NamespaceURI), string.Empty, this.GetType ( ).FullName, MessageImportance.Normal ) );

      if ( !string.IsNullOrEmpty ( this.NamespaceURI ) ) {
        xmlNamespaceManager.AddNamespace ( string.IsNullOrEmpty ( this.Prefix ) ? string.Empty : this.Prefix, this.NamespaceURI );
      }

      if ( this.Namespaces != null ) {
        foreach ( string pn in this.Namespaces ) {
          string[] tpn = pn.Split ( new char[] { ':' }, 2 );
          if ( !string.IsNullOrEmpty ( tpn[ 0 ] ) ) {
            if ( !xmlNamespaceManager.HasNamespace ( tpn[ 0 ] ) ) {
              xmlNamespaceManager.AddNamespace ( tpn[ 0 ], tpn[ 1 ] );
            }
          }
        }
      }

      XmlNode node = baseNode.SelectSingleNode ( query, xmlNamespaceManager );
      return node;
    }

    /// <summary>
    /// Performs an XPath query
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns></returns>
    protected virtual XmlNode XPathQuery ( string query ) {
      XmlNode baseNode = _document;
      return XPathQuery ( query, baseNode );
    }
  }
}

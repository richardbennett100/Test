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
using System.Xml;
using System.IO;

namespace MSBuild.Extended.Tasks.Xml {
  /// <summary>
  /// Sets the inner text of an xml element.
  /// </summary>
  public class SetXmlElementInnerText : AbstractXmlTask {
    private string innerText = string.Empty;
    private bool useCData = false;
    /// <summary>
    /// Gets or sets the XPath.
    /// </summary>
    /// <value>The XPath.</value>
    [Required]
    public override string XPath { get { return base.XPath; } set { base.XPath = value; } }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public override string Name { get { return string.Empty; } set { return; } }

    /// <summary>
    /// Gets or sets a value indicating whether [use CData].
    /// </summary>
    /// <value><c>true</c> if [use CData]; otherwise, <c>false</c>.</value>
    public bool UseCData { get { return this.useCData; } set { this.useCData = value; } }
    /// <summary>
    /// Gets or sets the inner text.
    /// </summary>
    /// <value>The inner text.</value>
    [Required]
    public string InnerText { get { return this.innerText; } set { this.innerText = value; } }
    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>
    /// true if the task executed successfully; otherwise, false.
    /// </returns>
    public override bool Execute () {
      try {
        _document = new XmlDocument ();
        if ( File.Exists ( this.XmlFile ) )
          _document.Load ( this.XmlFile );
        //XmlNamespaceManager xnm = new XmlNamespaceManager ( doc.NameTable );
       // if ( !string.IsNullOrEmpty ( this.NamespaceURI ) )
       //   xnm.AddNamespace ( this.Prefix, this.NamespaceURI );
       // XmlElement xEle = doc.SelectSingleNode ( this.XPath, xnm ) as XmlElement;
        XmlElement xEle = XPathQuery( this.XPath,_document ) as XmlElement;
        if ( xEle == null ) {
          if ( this.BuildEngine != null )
            this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( "XPath did not return a valid XmlElement.", string.Empty, this.GetType ().FullName, MessageImportance.High ) );

          throw new XmlException ( "XPath did not return a valid XmlElement." );
        }
        if ( !UseCData )
          xEle.InnerText = this.InnerText;
        else
          xEle.AppendChild ( _document.CreateCDataSection ( this.InnerText ) );
        _document.Save ( this.XmlFile );
      } catch ( Exception ex ) {
        if ( this.BuildEngine != null )
          this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ex.ToString (), string.Empty, this.GetType ().FullName, MessageImportance.High ) );
        throw;
      }
      return true;
    }
  }
}

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
  /// Gets the value of an attribute in an xml element.
  /// </summary>
  public class GetXmlElementAttributeValue : AbstractXmlTask {
    private string attributeValue = string.Empty;
    /// <summary>
    /// Gets or sets the XPath to the element.
    /// </summary>
    /// <value>The XPath.</value>
    [ Required ]
    public override string XPath { get { return base.XPath; } set { base.XPath = value; } }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    [ Output ]
    public string Value { get { return this.attributeValue; } set { this.attributeValue = value; } }

    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>
    /// true if the task executed successfully; otherwise, false.
    /// </returns>
    public override bool Execute () {
      try {
        XmlDocument doc = new XmlDocument ();
        if ( File.Exists ( this.XmlFile ) )
          doc.Load ( this.XmlFile );
        XmlNamespaceManager xnm = new XmlNamespaceManager ( doc.NameTable );
        if ( !string.IsNullOrEmpty ( this.NamespaceURI ) )
          xnm.AddNamespace ( this.Prefix, this.NamespaceURI );
        XmlElement xEle = doc.SelectSingleNode ( this.XPath, xnm ) as XmlElement;
        if ( xEle == null ) {
          throw new XmlException ( "XPath did not return a valid XmlElement." );
        }
        this.Value = xEle.GetAttribute ( this.Name );
      } catch ( Exception ex ) {
        if ( this.BuildEngine != null )
          this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ex.ToString(  ), string.Empty, this.GetType ().FullName, MessageImportance.High ) );
        throw;
      }
      return true;
    }
  }
}

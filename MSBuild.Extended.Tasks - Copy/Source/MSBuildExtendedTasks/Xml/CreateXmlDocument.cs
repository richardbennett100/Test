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
using System.Xml;

namespace MSBuild.Extended.Tasks.Xml {
  /// <summary>
  /// Creates a new xml document.
  /// </summary>
  public class CreateXmlDocument : AbstractXmlTask {
    private string xmlVersion = "1.0";
    private string xmlEncoding = "UTF-8";
    private bool? xmlStandalone = null;

    private bool errorIfExists = false;

    /// <summary>
    /// Gets or sets the XML version.
    /// </summary>
    /// <value>The XML version.</value>
    public string XmlVersion { get { return this.xmlVersion; } set { this.xmlVersion = value; } }

    /// <summary>
    /// Gets or sets the XML encoding.
    /// </summary>
    /// <value>The XML encoding.</value>
    public string XmlEncoding { get { return this.xmlEncoding; } set { this.xmlEncoding = value; } }

    /// <summary>
    /// Gets or sets the if the document is standalone.
    /// </summary>
    /// <value>is standalone.</value>
    public bool? Standalone { get { return this.xmlStandalone; } set { this.xmlStandalone = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether [error IF file exists].
    /// </summary>
    /// <value><c>true</c> if [error If file exists]; otherwise, <c>false</c>.</value>
    public bool ErrorIfFileExists { get { return this.errorIfExists; } set { this.errorIfExists = value; } }
    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>
    /// true if the task executed successfully; otherwise, false.
    /// </returns>
    public override bool Execute () {
      if ( File.Exists ( XmlFile ) && this.ErrorIfFileExists ) {
        if ( this.BuildEngine != null )
          this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( string.Format ( "File {0} already exists.", this.XmlEncoding ), string.Empty, this.GetType ().FullName, MessageImportance.High ) );
        throw new XmlException ( string.Format("File {0} already exists.",this.XmlEncoding) );
      }

      XmlDocument doc = new XmlDocument ();
      doc.AppendChild ( doc.CreateXmlDeclaration ( this.XmlVersion, this.XmlEncoding, this.Standalone.HasValue ? this.Standalone.Value ? "yes" : "no" : string.Empty ) );
      doc.AppendChild ( doc.CreateElement ( this.Prefix, this.Name, this.NamespaceURI ) );
      doc.Save (this.XmlFile);
      return true;
    }
  }
}

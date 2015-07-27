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
using System.Xml.XPath;

namespace MSBuild.Extended.Tasks.Xml {
  /// <summary>
  /// Creates an <see cref="System.Xml.XmlElement">XmlElement</see> and adds it to the element that matches 
  /// the xpath, document if no element exists, or if xpath is empty and there is a document element, it will 
  /// add it to the document element.
  /// </summary>
  public class AddXmlElement : AbstractXmlTask {

#region Task Members

    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>
    /// true if the task executed successfully; otherwise, false.
    /// </returns>
    public override bool Execute () {
      try {
      _document = new XmlDocument(  );
      if ( File.Exists ( this.XmlFile ) )
        _document.Load ( this.XmlFile );
        //XPathNavigator navigator = doc.CreateNavigator ();
        XmlElement ele = _document.CreateElement ( this.Prefix, this.Name, this.NamespaceURI );
        XmlElement docElement = _document.DocumentElement;
        if ( docElement == null ) {
          _document.AppendChild ( ele );
        } else {
          // if xpath is empty, then append to the doc element
          if ( string.IsNullOrEmpty ( this.XPath ) ) {
            docElement.AppendChild ( ele );
          } else {
            XmlElement xEle = this.XPathQuery ( this.XPath ) as XmlElement;
            //XmlElement xEle = doc.SelectSingleNode ( this.XPath, xnm ) as XmlElement;
            //XPathNavigator xEle = navigator.SelectSingleNode ( this.xpath );
            if ( xEle == null ) {
              throw new XmlException ( "XPath did not return a valid XmlElement." );
            }

            xEle.AppendChild ( ele );
          }
        }
        _document.Save ( this.XmlFile );
      } catch ( Exception ex ) {
        if ( this.BuildEngine != null )
          this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ex.ToString(  ), string.Empty, this.GetType ().FullName, MessageImportance.High ) );
        return false;
      }
      return true;
    }

    #endregion
  }
}

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
using Microsoft.Ddue.Tools;
using System.Xml.XPath;
using System.Configuration;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace MSBuild.Extended.Tasks.Components {
  /// <summary>
  /// Adds references to required css and javascript files and modifies the original <c>code</c> blocks
  /// </summary>
  public class CodeBlockPostTransformComponent : BuildComponent {
    private string outputPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBlockPostTransformComponent"/> class.
    /// </summary>
    /// <param name="assembler">The assembler.</param>
    /// <param name="configuration">The configuration.</param>
    public CodeBlockPostTransformComponent ( BuildAssembler assembler, XPathNavigator configuration )
      : base ( assembler, configuration ) {
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo ( this.GetType ().Assembly.Location );
      base.WriteMessage ( MessageLevel.Info, String.Format (
                CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    CodeBlock PostTransform Component. " +
                "Copyright (c) 2007, Ryan Conrad, All rights reserved." +
                "\r\n    http://www.codeplex.com/msbuildextasks", fvi.ProductName,
                fvi.ProductVersion ) );
      XPathNavigator nav = configuration.SelectSingleNode ( "outputPath" );
      if ( nav != null )
        outputPath = nav.GetAttribute ( "value", String.Empty );

      if ( string.IsNullOrEmpty ( outputPath ) )
        throw new ConfigurationErrorsException ( "You must specify a 'value' attribute on the <outputPath> element." );

      if ( !outputPath.EndsWith ( @"\" ) )
        outputPath += @"\";


    }
    /// <summary>
    /// Applies the specified document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="key">The key.</param>
    public override void Apply ( System.Xml.XmlDocument document, string key ) {
      XmlElement headElement = document.SelectSingleNode ( "/html/head" ) as XmlElement;
      string presentationPath = Path.Combine ( Path.GetDirectoryName ( this.GetType ().Assembly.Location ), @"Sandcastle\Presentation" );

      if ( headElement != null ) {
        DirectoryInfo scriptsDir = new DirectoryInfo ( Path.Combine ( presentationPath, "scripts" ) );
        foreach ( FileInfo file in scriptsDir.GetFiles ( "*.js" ) ) {
          string pathFormat = "../scripts/{0}";
          XmlElement scriptElement = document.CreateElement ( "script" );
          scriptElement.SetAttribute ( "type", "text/javascript" );
          scriptElement.SetAttribute ( "src", string.Format ( pathFormat, file.Name ) );
          scriptElement.InnerText = string.Empty;
          if ( headElement.SelectSingleNode ( string.Format ( "script[@src='{0}']", string.Format ( pathFormat, file.Name ) ) ) == null )
            headElement.AppendChild ( scriptElement );
        }
        DirectoryInfo stylesDir = new DirectoryInfo ( Path.Combine ( presentationPath, "styles" ) );
        foreach ( FileInfo file in stylesDir.GetFiles ( "*.css" ) ) {
          string pathFormat = "../styles/{0}";
          XmlElement linkElement = document.CreateElement ( "link" );
          linkElement.SetAttribute ( "type", "text/css" );
          linkElement.SetAttribute ( "rel", "stylesheet" );
          linkElement.SetAttribute ( "media", "screen" );
          linkElement.SetAttribute ( "href", string.Format ( pathFormat, file.Name ) );
          if ( headElement.SelectSingleNode ( string.Format ( "link[@href='{0}']", string.Format ( pathFormat, file.Name ) ) ) == null )
            headElement.AppendChild ( linkElement );
        }

        // this fix is for the built-in "code" class...
        // it removes the built-in code class and the pre
        // <div class="sectionContent"><div class="code"><pre><span id="span$code$84135b2d27ef451cbca99f387793508f">
        //string xpath = "//div[@class='sectionContent']/div[@class='code']/pre/span[@class='codeExHostSpan']";
        // //div[@id='exampleSection']/div[@class='code']/pre/span[@class='codeExHostSpan']{0}
        string xpath = "//div[@class='sectionContent']/div[@class='code']/pre/span[@class='codeExHostSpan'] | //div[@id='exampleSection']/div[@class='code']/pre/span[@class='codeExHostSpan']";
        XmlNodeList blocks = document.SelectNodes ( xpath );
        if ( blocks != null ) {
          foreach ( XmlElement block in blocks ) {
            string sourceBlock = block.OuterXml;
            XmlElement codeOld = block.ParentNode.ParentNode as XmlElement;
            if ( codeOld == null )
              base.WriteMessage ( MessageLevel.Error, "UNABLE TO FIND NODE" );
            else {
              base.WriteMessage ( MessageLevel.Info, codeOld.OuterXml );
              XmlElement preNode = codeOld.SelectSingleNode ( "pre" ) as XmlElement;
              if ( preNode != null )
                codeOld.RemoveChild ( preNode );
              codeOld.InnerXml = sourceBlock;
              codeOld.SetAttribute ( "class", string.Empty );
            }
          }
        }

        // check VS2005 presentation type
        // <div id="exampleSection" class="section" name="collapseableSection" style=""><div class="code"><pre>
      }
    }


  }
}

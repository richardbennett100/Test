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
using System.Xml.XPath;
using Microsoft.Ddue.Tools;
using System.Xml;
using System.IO;
using System.Web;

namespace MSBuild.Extended.Tasks.Components {
  /// <summary>
  /// A sandcastle component that colorizes the code blocks.
  /// </summary>
  public class CodeBlockComponent : Microsoft.Ddue.Tools.BuildComponent {
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBlockComponent"/> class.
    /// </summary>
    /// <param name="assembler">The assembler.</param>
    /// <param name="configuration">The configuration.</param>
    public CodeBlockComponent ( BuildAssembler assembler, XPathNavigator configuration )
      : base ( assembler, configuration ) {
    }
    /// <summary>
    /// Applies the specified document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="key">The key.</param>
    public override void Apply ( System.Xml.XmlDocument document, string key ) {
      string componentPath = Path.GetDirectoryName ( typeof ( CodeBlockComponent ).Assembly.Location );
      if ( componentPath.EndsWith("\\") )
        componentPath = componentPath.Substring( 0,componentPath.Length - 1 );
      document.PreserveWhitespace = true;
      string lang = "C#";
      string title = lang;
      string codeBlock;

      //base.WriteMessage ( MessageLevel.Info, document.OuterXml );

      //Console.WriteLine ( document.OuterXml );
      // Select all code nodes
      XmlNodeList codeList = document.SelectNodes ( "//code" );

      foreach ( XmlElement code in codeList ) {
      
        //Console.WriteLine ( "found code..." );
        if ( !string.IsNullOrEmpty ( code.GetAttribute ( "src" ) ) )
          codeBlock = this.ReadFile ( code.GetAttribute ( "src" ) );
        else
          codeBlock = code.InnerXml;

        bool decodeText = string.Compare ( bool.TrueString, code.GetAttribute ( "htmlDecode" ), true ) == 0;
        if ( decodeText )
          codeBlock = System.Web.HttpUtility.HtmlDecode ( codeBlock );

        if ( !string.IsNullOrEmpty ( code.GetAttribute ( "language" ) ) )
          lang = code.GetAttribute ( "language" );

        //Console.WriteLine ( "Code Language: {0}", lang );
        //Console.WriteLine ( "Code: \n{0}\n", codeBlock );

        if ( !string.IsNullOrEmpty ( code.GetAttribute ( "title" ) ) )
          title = code.GetAttribute ( "title" );
        else
          title = lang;

        ActiproSoftware.CodeHighlighter.CodeHighlighterConfigurationSectionHandler shcsh = new ActiproSoftware.CodeHighlighter.CodeHighlighterConfigurationSectionHandler ();
        XmlDocument doc = new XmlDocument ();
        string xml = Properties.Resources.CodeHighlighter.Replace ( "$(ComponentPath)", componentPath );
        //base.WriteMessage ( MessageLevel.Info, xml );
        doc.LoadXml ( xml );
        ActiproSoftware.CodeHighlighter.CodeHighlighterConfiguration config = (ActiproSoftware.CodeHighlighter.CodeHighlighterConfiguration)shcsh.Create ( null, null, doc.DocumentElement );
        
        ActiproSoftware.CodeHighlighter.CodeHighlighterEngine engine = new ActiproSoftware.CodeHighlighter.CodeHighlighterEngine ();
        engine.KeywordLinkingEnabled = config.KeywordLinkingEnabled;
        engine.KeywordLinkingTarget = config.KeywordLinkingTarget;
        //engine.Keywords.AddRange(config.KeywordCollections.Values);
        engine.LineNumberMarginForeColor = config.LineNumberMarginForeColor;
        engine.LineNumberMarginPaddingCharacter = config.LineNumberMarginPaddingCharacter;
        engine.LineNumberMarginVisible = config.LineNumberMarginVisible;
        engine.OutliningEnabled = config.OutliningEnabled;
        engine.OutliningImagesPath = config.OutliningImagesPath;
        engine.SpacesInTabs = config.SpacesInTabs;

        ActiproSoftware.SyntaxEditor.SyntaxLanguage syntaxLanguage = ActiproSoftware.SyntaxEditor.Addons.Dynamic.DynamicSyntaxLanguage.PlainText;
        if ( config.LanguageConfigs.ContainsKey ( lang.ToUpper () ) ) {
          ActiproSoftware.CodeHighlighter.SyntaxLanguageConfiguration slConfig = config.LanguageConfigs[lang.ToUpper ()] as ActiproSoftware.CodeHighlighter.SyntaxLanguageConfiguration;
          syntaxLanguage = ActiproSoftware.SyntaxEditor.Addons.Dynamic.DynamicSyntaxLanguage.LoadFromXml ( slConfig.DefinitionPath,0 );
        } else
          base.WriteMessage ( MessageLevel.Error, string.Format ( "COULD NOT FIND LANGUAGE {0}" ) );

        string id = string.Format ( "code${0}", Guid.NewGuid ().ToString ( "N" ) );
        string div = this.ReadFile ( Path.Combine ( componentPath, @"Sandcastle\Transforms\CodeContainerFormat.template" ) );
        string codeContent = engine.GenerateHtmlInline ( id, codeBlock, syntaxLanguage );
        code.InnerXml = string.Format ( div, id, title, codeContent, HttpUtility.HtmlEncode ( codeBlock ), decodeText.ToString ().ToLower () );
        //base.WriteMessage ( MessageLevel.Info, code.InnerXml );
      }
    }

    /// <summary>
    /// Loads the code block from a file.
    /// </summary>
    /// <param name="codeFile">The code file.</param>
    /// <returns>the code contained in the file</returns>
    private string ReadFile ( string codeFile ) {
      try {
        FileInfo file = new FileInfo ( codeFile );
        if ( !file.Exists )
          return string.Empty;
        FileStream fs = new FileStream ( file.FullName, FileMode.Open, FileAccess.Read );
        StreamReader reader = new StreamReader ( fs );
        StringBuilder sb = new StringBuilder ();
        using ( fs ) {
          using ( reader ) {
            while ( !reader.EndOfStream )
              sb.AppendLine ( reader.ReadLine () );
          }
        }
        return sb.ToString ();
      } catch {
        throw;
      }
    }
  }
}

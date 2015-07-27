
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeHighligherSandcastleComponent.Parsers {
  using ActiproSoftware.SyntaxEditor;
  using ActiproSoftware.SyntaxEditor.Addons.Dynamic;
  ///  <summary>
  ///  Provides an implementation of a <c>C#</c> syntax language that can perform automatic outlining.
  ///  </summary>
  public class CSharpDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage {

    ///  <summary>
    ///  This constructor is for designer use only and should never be called by your code.
    ///  </summary>
    public CSharpDynamicSyntaxLanguage () {
    }

    ///  <summary>
    ///  Initializes a new instance of the <c>CSharpDynamicSyntaxLanguage</c> class. 
    ///  </summary>
    ///  <param name="key">The key of the language.</param>
    ///  <param name="secure">Whether the language is secure.</param>
    public CSharpDynamicSyntaxLanguage ( string key, bool secure )
      :
            base ( key, secure ) {
    }

    ///  <summary>
    ///  Returns token parsing information for automatic outlining that determines if the current <see cref="IToken"/>
    ///  in the <see cref="TokenStream"/> starts or ends an outlining node.
    ///  </summary>
    ///  <param name="tokenStream">A <see cref="TokenStream"/> that is positioned at the <see cref="IToken"/> requiring outlining data.</param>
    ///  <param name="outliningKey">Returns the outlining node key to assign.  A <see langword="null"/> should be returned if the token doesn't start or end a node.</param>
    ///  <param name="tokenAction">Returns the <see cref="OutliningNodeAction"/> to take for the token.</param>
    public override void GetTokenOutliningAction ( TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction ) {
      //  Get the token
      IToken token = tokenStream.Peek ();
      //  See if the token starts or ends an outlining node
      switch ( token.Key ) {
        case "OpenCurlyBraceToken":
          outliningKey = "CodeBlock";
          tokenAction = OutliningNodeAction.Start;
          break;
        case "CloseCurlyBraceToken":
          outliningKey = "CodeBlock";
          tokenAction = OutliningNodeAction.End;
          break;
        case "MultiLineCommentStartToken":
          outliningKey = "MultiLineComment";
          tokenAction = OutliningNodeAction.Start;
          break;
        case "MultiLineCommentEndToken":
          outliningKey = "MultiLineComment";
          tokenAction = OutliningNodeAction.End;
          break;
        case "XMLCommentStartToken":
          outliningKey = "XMLComment";
          tokenAction = OutliningNodeAction.Start;
          break;
        case "XMLCommentEndToken":
          outliningKey = "XMLComment";
          tokenAction = OutliningNodeAction.End;
          break;
        case "RegionPreProcessorDirectiveStartToken":
          outliningKey = "RegionPreProcessorDirective";
          tokenAction = OutliningNodeAction.Start;
          break;
        case "EndRegionPreProcessorDirectiveStartToken":
          outliningKey = "RegionPreProcessorDirective";
          tokenAction = OutliningNodeAction.End;
          break;
      }
    }

    // GetTokenOutliningAction
    //  <summary>
    //  Occurs after automatic outlining is performed on a <see cref="Document"/> that uses this language.
    //  </summary>
    //  <param name="document">The <see cref="Document"/> that is being modified.</param>
    //  <param name="e">A <c>DocumentModificationEventArgs</c> that contains the event data.</param>
    //  <remarks>
    //  A <see cref="DocumentModification"/> may or may not be passed in the event arguments, depending on if the outlining
    //  is performed in the main thread.
    //  </remarks>
    protected override void OnDocumentAutomaticOutliningComplete ( Document document, DocumentModificationEventArgs e ) {
      //  Collapse all outlining region nodes
      document.Outlining.RootNode.CollapseDescendants ( "RegionPreProcessorDirective" );
    }

    ///  <summary>
    ///  Allows for setting the collapsed text for the specified <see cref="OutliningNode"/>.
    ///  </summary>
    ///  <param name="node">The <see cref="OutliningNode"/> that is requesting collapsed text.</param>
    public override void SetOutliningNodeCollapsedText ( OutliningNode node ) {
      TokenCollection tokens = node.Document.Tokens;
      int tokenIndex = tokens.IndexOf ( node.StartOffset );
      switch ( tokens[tokenIndex].Key ) {
        case "MultiLineCommentStartToken":
        case "XMLCommentStartToken":
          node.CollapsedText = "/**/";
          break;
        case "RegionPreProcessorDirectiveStartToken":
          string collapsedText = String.Empty;
          tokenIndex = ( tokenIndex + 1 );
          while ( ( tokenIndex < tokens.Count ) ) {
            if ( ( tokens[tokenIndex].Key == "PreProcessorDirectiveEndToken" ) ) {
              break;
            }
            collapsedText = ( collapsedText + tokens.Document.GetTokenText ( tokens[tokenIndex] ) );
            tokenIndex = ( tokenIndex + 1 );
          }
          node.CollapsedText = collapsedText.Trim ();
          break;
      }
    }
  }
}

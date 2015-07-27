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
using System.Text.RegularExpressions;
using System.ComponentModel;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Extended.Tasks.String {
  /// <summary>
  /// Using Regex, this task will replace a string with a replacement string.
  /// </summary>
  public class StringReplace : Task {
    private string pattern = string.Empty;
    private string inputString = string.Empty;
    private string replace = string.Empty;

    private string resultString = string.Empty;

    private bool cultureInvariant = false;
    private bool ecmaScript = false;
    private bool explicitCapture = false;
    private bool ignoreCase = false;
    private bool ignorePatternWhitespace = false;
    private bool multiLine = true;
    private bool rightToLeft = false;
    private bool singleLine = false;

    #region Properties
    /// <summary>
    /// Gets or sets the pattern.
    /// </summary>
    /// <value>The pattern.</value>
    [Description ( "Gets or sets the pattern used to match the string." ), Required]
    public string Pattern { get { return this.pattern; } set { this.pattern = value; } }

    /// <summary>
    /// Gets or sets the input string.
    /// </summary>
    /// <value>The input string.</value>
    [Description ( "Gets or sets the input string." ), Required]
    public string InputString { get { return this.inputString; } set { this.inputString = value; } }

    /// <summary>
    /// Gets or sets the replacement string.
    /// </summary>
    /// <value>The replace.</value>
    [Description ( "Gets or sets the replacement string." )]
    public string Replace { get { return this.replace; } set { this.replace = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether regex uses culture invariant.
    /// </summary>
    /// <value><c>true</c> if [culture invariant]; otherwise, <c>false</c>.</value>
    [ Description( "Gets or sets a value indicating whether regex uses culture invariant." ) ]
    public bool CultureInvariant { get { return this.cultureInvariant; } set { this.cultureInvariant = value; } }


    /// <summary>
    /// Gets or sets a value indicating whether regex uses ECMA script.
    /// </summary>
    /// <value><c>true</c> if [ECMA script]; otherwise, <c>false</c>.</value>
    [ Description( "Gets or sets a value indicating whether regex uses ECMA script" ) ]
    public bool ECMAScript { get { return this.ecmaScript; } set { this.ecmaScript = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether regex will ignore case.
    /// </summary>
    /// <value><c>true</c> if [ignore case]; otherwise, <c>false</c>.</value>
    [ Description( "Gets or sets a value indicating whether regex will ignore case." ) ]
    public bool IgnoreCase { get { return this.ignoreCase; } set { this.ignoreCase = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether regex will ignore pattern whitespace.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if [ignore pattern whitespace]; otherwise, <c>false</c>.
    /// </value>
    [ Description( "Gets or sets a value indicating whether regex will ignore pattern whitespace." ) ]
    public bool IgnorePatternWhitespace { get { return this.ignorePatternWhitespace; } set { this.ignorePatternWhitespace = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether regex will evaluate using multi line options. 
    /// </summary>
    /// <value><c>true</c> if [multi line]; otherwise, <c>false</c>.</value>
    [Description ( "Gets or sets a value indicating whether regex will evaluate using multi line options. " )]
    public bool MultiLine { get { return this.multiLine; } set { this.multiLine = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether regex will evaluate right to left.
    /// </summary>
    /// <value><c>true</c> if [right to left]; otherwise, <c>false</c>.</value>
    [ Description( "Gets or sets a value indicating whether regex will evaluate right to left." ) ]
    public bool RightToLeft { get { return this.rightToLeft; } set { this.rightToLeft = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether regex will evaluate using single line options.
    /// </summary>
    /// <value><c>true</c> if [single line]; otherwise, <c>false</c>.</value>
    [ Description( "Gets or sets a value indicating whether regex will evaluate using single line options." ) ]
    public bool SingleLine { get { return this.singleLine; } set { this.singleLine = value; } }
    /// <summary>
    /// Gets or sets the result.
    /// </summary>
    /// <value>The result.</value>
    [Description ( "Gets or sets the result." ), Output]
    public string Result { get { return this.resultString; } set { this.resultString = value; } }
    #endregion

    #region AbstractTask Members
    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>
    /// true if the task executed successfully; otherwise, false.
    /// </returns>
    public override bool Execute () {
      if ( string.IsNullOrEmpty ( pattern ) )
        return false;
      try {
        Regex regex = new Regex ( this.Pattern, GetRegexOptions () );
        this.Result = regex.Replace ( this.InputString, this.Replace );
      } catch (  Exception ex ) {
        if ( BuildEngine != null )
          BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ex.ToString(), string.Empty, this.GetType().FullName, MessageImportance.High ) );
        return false;
      }
      return true;
    }

    #endregion

    /// <summary>
    /// Gets the regex options.
    /// </summary>
    /// <returns></returns>
    private RegexOptions GetRegexOptions () {
      RegexOptions options = RegexOptions.Compiled;
      if ( cultureInvariant )
        options |= RegexOptions.CultureInvariant;
      if ( ecmaScript )
        options |= RegexOptions.ECMAScript;
      if ( explicitCapture )
        options |= RegexOptions.ExplicitCapture;
      if ( ignoreCase )
        options |= RegexOptions.IgnoreCase;
      if ( ignorePatternWhitespace )
        options |= RegexOptions.IgnorePatternWhitespace;
      if ( multiLine )
        options |= RegexOptions.Multiline;
      if ( rightToLeft )
        options |= RegexOptions.RightToLeft;
      if ( singleLine )
        options |= RegexOptions.Singleline;
      return options;
    }
  }
}

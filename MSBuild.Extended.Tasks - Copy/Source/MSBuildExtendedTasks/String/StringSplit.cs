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
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Extended.Tasks.String {
  /// <summary>
  /// Splits a string in to an array of strings
  /// </summary>
  public class StringSplit : Task {
    private string _inputString = string.Empty;
    private string[] _splitStrings = null;
    private int? _maxSplit = null;
    private string[] _result = null;
    private bool _removeEmptyEntries = false;
    /// <summary>
    /// Gets or sets the input string.
    /// </summary>
    /// <value>The input string.</value>
    [Required]
    public string InputString { get { return this._inputString; } set { this._inputString = value; } }
    /// <summary>
    /// An array of strings to use to split the input string
    /// </summary>
    /// <value>The split string.</value>
    [Required]
    public string[] SplitString { get { return this._splitStrings; } set { this._splitStrings = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether to remove the empty strings from the array.
    /// </summary>
    /// <value>if <c>true</c>, any empty string found after splitting the input string will be removed.</value>
    public bool RemoveEmptyEntries { get { return this._removeEmptyEntries; } set { this._removeEmptyEntries = value; } }
    /// <summary>
    /// Gets or sets the max results.
    /// </summary>
    /// <value>The max results.</value>
    public int? MaxResults { get { return this._maxSplit; } set { this._maxSplit = value; } }
    /// <summary>
    /// Gets the resulting strings from the split.
    /// </summary>
    /// <value>The result.</value>
    [Output]
    public string[] Result { get { return this._result; } set { this._result = value; } }
    /// <summary>
    /// When overridden in a derived class, executes the task.
    /// </summary>
    /// <returns>
    /// true if the task successfully executed; otherwise, false.
    /// </returns>
    public override bool Execute ( ) {
      int max = this.MaxResults.HasValue ? this.MaxResults.Value : int.MaxValue;
      this.Result = this.InputString.Split ( this.SplitString, max, this.RemoveEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None );
      return true;
    }
  }
}

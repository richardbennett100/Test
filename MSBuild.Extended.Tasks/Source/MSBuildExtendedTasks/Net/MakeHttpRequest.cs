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
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using Microsoft.Build.Utilities;
namespace MSBuild.Extended.Tasks.Net {
  /// <summary>
  /// Performs a Http Web Request.
  /// </summary>
  /// <example>
  /// <code language="xml" htmlDecode="true" title="MSBuild"><![CDATA[<MakeHttpRequest 
///    UseProxy="True" ProxyUsername="myUsername" ProxyPassword="p@ssw0rd1"
///    ProxyServer="proxy.domain.com" ProxyPort="8080" Method="Post" 
///    UserAgent="MSBuild Extended Tasks"
///    Url="http://www.codeplex.com/msbuildextasks/Release/ProjectReleases.aspx">
///  <Output TaskParameter="ResponseContentType" PropertyName="ResponseContentType" />
///  <Output TaskParameter="ResponseContentLength" PropertyName="ResponseContentLength" />
///  <Output TaskParameter="ResponseMethod" PropertyName="ResponseMethod" />
///  <Output TaskParameter="ResponseStatusDescription" PropertyName="ResponseStatusDescription" />
///  <Output TaskParameter="ResponseStatusCodeName" PropertyName="ResponseStatusCodeName" />
///  <Output TaskParameter="ResponseHeaders" PropertyName="ResponseHeaders" />
///  <Output TaskParameter="ResponseText" PropertyName="ResponseText" />
///</MakeHttpRequest>]]></code>
  /// </example>
  public class MakeHttpRequest : Task {
    #region private members
    private Uri _url = null;
    private string _accept = "*/*";
    private int? _rangeFrom = null;
    private int? _rangeTo = null;
    private bool _allowAutoRedirect = true;
    private string _userAgent = string.Empty;
    private bool _useAuthentication = false;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _domain = string.Empty;
    private bool _acceptSSLCertificate = true;
    private bool _keepAlive = false;
    private bool _useProxy = false;
    private string _proxyAddress = string.Empty;
    private int _proxyPort = 8080;
    private string _proxyUser = string.Empty;
    private string _proxyPassword = string.Empty;
    private string _mediaType = string.Empty;
    private string _referer = string.Empty;
    private string _method = "get";
    private int _timeout = 30;
    private string _transferEncoding = null;
    private string[] _headers = null;
    private string _expect = string.Empty;

    private string[] _responseHeaders = null;
    private string _responseCodeName = string.Empty;
    private int _responseCode = 0;
    private string _responseCodeText = string.Empty;
    private string _responseText = string.Empty;
    private string _responseContentType = string.Empty;
    private long _responseContentLength = 0;
    private string _responseMethod = string.Empty;
    #endregion
    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="MakeHttpRequest"/> class.
    /// </summary>
    public MakeHttpRequest ( ) {

    }
    #endregion
    #region Public Input Properties
    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    /// <value>The URL.</value>
    [Required]
    public string Url { get { return this._url.ToString ( ); } set { this._url = new Uri(value); } }

    /// <summary>
    /// Gets or sets the range from.
    /// </summary>
    /// <value>The range from.</value>
    public int? RangeFrom { get { return this._rangeFrom; } set { this._rangeFrom = value; } }

    /// <summary>
    /// Gets or sets the range to.
    /// </summary>
    /// <value>The range to.</value>
    public int? RangeTo { get { return this._rangeTo; } set { this._rangeTo = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether [allow auto redirect].
    /// </summary>
    /// <value>If the request should allow an auto redirect then <c>true</c>, otherwise, <c>false</c></value>
    public bool AllowAutoRedirect { get { return this._allowAutoRedirect; } set { this._allowAutoRedirect = value; } }
    
    /// <summary>
    /// Gets or sets the accept.
    /// </summary>
    /// <value>The accept.</value>
    public string Accept { get { return this._accept; } set { this._accept = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether or not to accept SSL certificate from server.
    /// </summary>
    /// <value>
    /// 	if <c>true</c>accept the SSL certificate; otherwise, <c>false</c>.
    /// </value>
    public bool AcceptSSLCertificate { get { return this._acceptSSLCertificate; } set { this._acceptSSLCertificate = value; } }
    /// <summary>
    /// Gets or sets a value indicating whether to use authentication while making the request.
    /// </summary>
    /// <value>if <c>true</c> then the request should pass username and password, otherwise, <c>false</c>.</value>
    public bool UseAuthentication { get { return this._useAuthentication; } set { this._useAuthentication = value; } }
    /// <summary>
    /// Gets or sets the authentication username.
    /// </summary>
    /// <value>The username.</value>
    public string AuthenticationUsername { get { return this._userName; } set { this._userName = value; } }

    /// <summary>
    /// Gets or sets the authentication password.
    /// </summary>
    /// <value>The password.</value>
    public string AuthenticationPassword { get { return this._password; } set { this._password = value; } }

    /// <summary>
    /// Gets or sets the authentication domain.
    /// </summary>
    /// <value>The authentication domain.</value>
    public string AuthenticationDomain { get { return this._domain; } set { this._domain = value; } }

    /// <summary>
    /// Gets or sets the expect HTTP Header.
    /// </summary>
    /// <value>The expect HTTP header value.</value>
    public string Expect { get { return this._expect; } set { this._expect = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether use proxy.
    /// </summary>
    /// <value>if <c>true</c> use proxy; otherwise, <c>false</c>.</value>
    public bool UseProxy { get { return this._useProxy; } set { this._useProxy = value; } }

    /// <summary>
    /// Gets or sets the proxy username.
    /// </summary>
    /// <value>The proxy username.</value>
    public string ProxyUsername { get { return this._proxyUser; } set { this._proxyUser = value; } }

    /// <summary>
    /// Gets or sets the proxy password.
    /// </summary>
    /// <value>The proxy password.</value>
    public string ProxyPassword { get { return this._proxyPassword; } set { this._proxyPassword = value; } }

    /// <summary>
    /// Gets or sets the proxy server.
    /// </summary>
    /// <value>The proxy server.</value>
    public string ProxyServer { get { return this._proxyAddress; } set { this._proxyAddress = value; } }

    /// <summary>
    /// Gets or sets the proxy port.
    /// </summary>
    /// <value>The proxy port.</value>
    public int ProxyPort { get { return this._proxyPort; } set { this._proxyPort = value; } }

    /// <summary>
    /// Gets or sets the headers. These should be in a <c>key:value</c> format.
    /// </summary>
    /// <value>The headers. Each item in the array should be in <c>key:value</c> format.</value>
    /// <remarks></remarks>
    public string[] Headers { get { return this._headers; } set { this._headers = value; } }

    /// <summary>
    /// Gets or sets a value indicating whether [keep alive].
    /// </summary>
    /// <value><c>true</c> if [keep alive]; otherwise, <c>false</c>.</value>
    public bool KeepAlive { get { return this._keepAlive; } set { this._keepAlive = value; } }

    /// <summary>
    /// Gets or sets the media type.
    /// </summary>
    /// <value>The type of media.</value>
    public string MediaType { get { return this._mediaType; } set { this._mediaType = value; } }

    /// <summary>
    /// Gets or sets the request method.
    /// </summary>
    /// <value>The request method.</value>
    public string Method { get { return this._method; } set { this._method = value; } }

    /// <summary>
    /// Gets or sets the referer.
    /// </summary>
    /// <value>The referer.</value>
    public string Referer { get { return this._referer; } set { this._referer = value; } }

    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    /// <value>The timeout in seconds.</value>
    public int Timeout { get { return this._timeout; } set { this._timeout = value; } }

    /// <summary>
    /// Gets or sets the request transfer encoding.
    /// </summary>
    /// <value>The transfer encoding.</value>
    public string TransferEncoding { get { return this._transferEncoding; } set { this._transferEncoding = value; } }
    /// <summary>
    /// Gets or sets the user agent string.
    /// </summary>
    /// <value>The user agent.</value>
    [Required]
    public string UserAgent { get { return this._userAgent; } set { this._userAgent = value; } }
    #endregion
    #region Public Output Properties
    /// <summary>
    /// Gets or sets the result text.
    /// </summary>
    /// <value>The result text.</value>
    [Output]
    public string ResponseText { get { return this._responseText; } set { this._responseText = value; } }
    /// <summary>
    /// Gets or sets the response headers.
    /// </summary>
    /// <value>The response headers.</value>
    [Output]
    public string[] ResponseHeaders { get { return this._responseHeaders; } set { this._responseHeaders = value; } }
    /// <summary>
    /// Gets or sets the name of the response code.
    /// </summary>
    /// <value>The name of the response code.</value>
    [Output]
    public string ResponseStatusCodeName { get { return this._responseCodeName; } set { this._responseCodeName = value; } }
    /// <summary>
    /// Gets or sets the response code.
    /// </summary>
    /// <value>The response code.</value>
    [Output]
    public int ResponseStatusCode { get { return this._responseCode; } set { this._responseCode = value; } }
    /// <summary>
    /// Gets or sets the response code text.
    /// </summary>
    /// <value>The response code text.</value>
    [Output]
    public string ResponseStatusDescription { get { return this._responseCodeText; } set { this._responseCodeText = value; } }

    /// <summary>
    /// Gets or sets the response method.
    /// </summary>
    /// <value>The response method.</value>
    [Output]
    public string ResponseMethod { get { return this._responseMethod; } set { this._responseMethod = value; } }

    /// <summary>
    /// Gets or sets the length of the response content.
    /// </summary>
    /// <value>The length of the response content.</value>
    [Output]
    public long ResponseContentLength { get { return this._responseContentLength; } set { this._responseContentLength = value; } }

    /// <summary>
    /// Gets or sets the type of the response content.
    /// </summary>
    /// <value>The type of the response content.</value>
    [Output]
    public string ResponseContentType { get { return this._responseContentType; } set { this._responseContentType = value; } }
    #endregion
    /// <summary>
    /// When overridden in a derived class, executes the task.
    /// </summary>
    /// <returns>
    /// true if the task successfully executed; otherwise, false.
    /// </returns>
    public override bool Execute ( ) {
      //try {
        HttpWebRequest request = HttpWebRequest.Create ( this.Url ) as HttpWebRequest;
        request.Accept = this.Accept;

        if ( this.RangeFrom.HasValue && this.RangeTo.HasValue )
          request.AddRange ( this.RangeFrom.Value, this.RangeTo.Value );

        request.AllowAutoRedirect = this.AllowAutoRedirect;
        if ( UseAuthentication ) {
          NetworkCredential nc = new NetworkCredential ( this.AuthenticationUsername, this.AuthenticationPassword, this.AuthenticationDomain );
          request.Credentials = nc;
        }

        if ( this.UseProxy ) {
          WebProxy proxy = new WebProxy ( this.ProxyServer, this.ProxyPort );
          if ( !string.IsNullOrEmpty ( this.ProxyUsername ) && !string.IsNullOrEmpty ( this.ProxyPassword ) )
            proxy.Credentials = new NetworkCredential ( this.ProxyUsername, this.ProxyPassword );
          request.Proxy = proxy;
        }

        request.Expect = this.Expect;
        ServicePointManager.ServerCertificateValidationCallback = delegate ( object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors ) {
          return this.AcceptSSLCertificate;
        };

        if ( this.Headers != null ) {
          foreach ( string keyHeader in this.Headers )
            request.Headers.Add ( keyHeader );
        }

        request.KeepAlive = this.KeepAlive;
        request.MediaType = this.MediaType;
        request.Method = this.Method;
        request.Referer = this.Referer;
        request.Timeout = this.Timeout * 1000;
        request.TransferEncoding = this.TransferEncoding;
        request.UserAgent = this.UserAgent;

        HttpWebResponse resp = request.GetResponse ( ) as HttpWebResponse;
        string[] outHeaders = new string[ resp.Headers.Count ];
        for ( int x = 0; x < resp.Headers.Count; x++ )
          outHeaders[ x ] = string.Format ( "{0}:{1}", resp.Headers.Keys[ x ], resp.Headers[ x ] );
        this.ResponseStatusCode = (int)resp.StatusCode;
        this.ResponseStatusCodeName = resp.StatusCode.ToString ( );
        this.ResponseStatusDescription = resp.StatusDescription;
        this.ResponseContentType = resp.ContentType;
        this.ResponseMethod = resp.Method;
        this.ResponseContentLength = resp.ContentLength;

        //this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ResponseStatusCodeName, string.Empty, this.GetType ().FullName, MessageImportance.Normal ) );
        //this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ResponseContentType, string.Empty, this.GetType ().FullName, MessageImportance.Normal ) );

        StreamReader reader = new StreamReader ( resp.GetResponseStream ( ) );
        StringBuilder sbData = new StringBuilder ( );
        while ( !reader.EndOfStream )
          sbData.AppendLine ( reader.ReadLine ( ) );

        this.ResponseText = sbData.ToString ( );
        //this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ResponseText, string.Empty, this.GetType ().FullName, MessageImportance.Normal ) );
      //} catch ( Exception ex ) {
        //this.BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ex.ToString(), string.Empty, this.GetType ( ).FullName, MessageImportance.High ) );
        //this.BuildEngine.LogErrorEvent(new BuildErrorEventArgs(string.Empty,ex.Source,string.Empty,ex.InnerException.TargetSite.
      //  throw ex;
      //}
      return true;
   }
 }
}

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
using System.ComponentModel;

/// <summary>
/// The aim of this project is to create a large collection of MSBuild tasks that can be used to perform 
/// actions to further automate the build process.
/// </summary>
/// <example>
/// <code language="xml" title="MSBuild Sample" htmlDecode="true"><![CDATA[<Project DefaultTargets="All"  xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
///   <!-- Required Import to use MSBuild Extended Tasks -->
///   <Import Project="$(MSBuildExtensionsPath)\MSBuildExtendedTasks\MSBuild.Extended.Tasks.Targets"/>
///   <PropertyGroup>
///     <Configuration Condition="'$(Configuration)' == ''">Debug</Configuration>
///     <OutputPath>bin\$(Configuration)\</OutputPath>
///     <XmlFilePath>$(MSBuildProjectDirectory)\Test.xml</XmlFilePath>
///     <InputString>0.3.92.642</InputString>
///   </PropertyGroup>
///   
///   <Target Name="All" DependsOnTargets="BuildDocumentation;StringReplace;WebRequest;CreateXmlDocument;StringFormat" />
/// 
///   <Target Name="StringReplace">
///     <StringReplace Pattern="\." InputString="$(InputString)" Replace="_">
///       <Output PropertyName="ResultString" TaskParameter="Result" />
///     </StringReplace>
///     <Message Text="Result = $(ResultString)" />
///   </Target>
/// 
///   <Target Name="StringFormat">
///     <StringFormat Format="Old Version: {0}; New Version: {1}" Parameters="$(InputString);$(ResultString)">
///       <Output PropertyName="ResultFormat" TaskParameter="Result" />
///     </StringFormat>
///     <Message Text="ResultFormat = $(ResultFormat)" />
///   </Target>
/// 
///   <Target Name="CreateXmlDocument">
///     <Delete Files="$(XmlFilePath)" Condition="Exists('$(XmlFilePath)')" />
///     <CreateXmlDocument XmlFile="$(XmlFilePath)" Name="Project" />
///     <SetXmlElementAttribute Name="DefaultTargets" Value="All" XmlFile="$(XmlFilePath)" XPath="/Project" />
///     <AddXmlElement XmlFile="$(XmlFilePath)" Name="Import" />
///     <SetXmlElementAttribute XmlFile="$(XmlFilePath)" Name="Project" Value="$(MSBuildExtensionsPath)\MSBuildExtendedTasks\MSBuild.Extended.Tasks.Targets"
///                    XPath="/Project/Import[1]" />
///     <AddXmlElement XmlFile="$(XmlFilePath)" Name="Import2" />
///     <AddXmlElement XmlFile="$(XmlFilePath)" Name="Import2" />
///     <AddXmlElement XmlFile="$(XmlFilePath)" Name="Import2" />
///     <AddXmlElement XmlFile="$(XmlFilePath)" Name="Import2" />
///     <RemoveXmlNode XmlFile="$(XmlFilePath)" RemoveNodeXPath="Import2" ParentNodeXPath="/Project" />
/// 
///     <GetXmlElementAttributeValue XmlFile="$(XmlFilePath)" XPath="/Project/Import[1]" Name="Project">
///       <Output PropertyName="ImportValue" TaskParameter="Value" />
///     </GetXmlElementAttributeValue>
///     <Message Text="ImportValue = $(ImportValue)" />
/// 
///     <AddXmlElement XmlFile="$(XmlFilePath)" Name="PropertyGroup" />
///     <AddXmlElement XmlFile="$(XmlFilePath)" Name="MyProperty" XPath="/Project/PropertyGroup" />
///     <SetXmlElementInnerText XmlFile="$(XmlFilePath)" XPath="/Project/PropertyGroup/MyProperty" InnerText="$(OutputPath)" />
///   </Target>
/// 
///   <Target Name="BuildDocumentation">
///     <CreateItem Include="$(MSBuildProjectDirectory)\MSBuild.Extended.Tasks.xml">
///       <Output ItemName="CommentFiles" TaskParameter="Include"/>
///     </CreateItem>
///     <CreateItem Include="$(MSBuildProjectDirectory)\MSBuild.Extended.Tasks.dll">
///       <Output ItemName="AssemblyFiles" TaskParameter="Include"/>
///     </CreateItem>
/// 
///     <GenerateDocumentation SandcastlePath="C:\Program Files\Sandcastle" HelpName="MSBuild.Extended.Tasks"
///       OutputPath="$(MSBuildProjectDirectory)\Help" Assemblies="@(AssemblyFiles)"
///       Dependencies="$(SYSTEMROOT)\Microsoft.NET\Framework\v2.0.50727\Microsoft.Build.Framework.dll;$(SYSTEMROOT)\Microsoft.NET\Framework\v2.0.50727\Microsoft.Build.Utilities.dll;"
///       XmlDocumentationFiles="@(CommentFiles)" BuildCHM="True" DocumentStyle="Standard"
///       PresentationType="VS2005" FileNameStyle="Friendly" UseCodeSyntaxHighlighterComponent="True" 
///       CleanUpDocumentationFiles="False" SupportNamespaceDocClass="True" LocalReferenceLinkResolveType="Local"
///       ExternalReferenceLinkResolveType="MSDN"
///       ReflectionToHtmlTocTransform="$(MSBuildExtensionsPath)\MSBuildExtendedTasks\Sandcastle\Transforms\ReflectionToHtmlToc.xsl"
///     />
/// 
///     <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="html" />
///     <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="head" XPath="/html" />
///     <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="title" XPath="/html/head" />
///     <SetXmlElementInnerText XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" XPath="/html/head/title"
///                             InnerText="MSBuild.Extended.Tasks" />
///     <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="frameset" XPath="/html" />
///     <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="cols" XPath="/html/frameset"
///                             Value="25&#37;,75&#37;" />
///     <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="frame" XPath="/html/frameset" />
///     <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="name" XPath="/html/frameset/frame[1]"
///                             Value="tocFrame" />
///     <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="src" XPath="/html/frameset/frame[1]"
///                             Value="toc.htm" />
///     <AddXmlElement XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="frame" XPath="/html/frameset" />
///     <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="name" XPath="/html/frameset/frame[2]"
///                           Value="contentFrame" />
///     <SetXmlElementAttribute XmlFile="$(MSBuildProjectDirectory)\Help\Html\index.htm" Name="src" XPath="/html/frameset/frame[2]"
///                             Value="R_Project.htm" />
///   </Target>
///   
///   <Target Name="WebRequest">
///     <MakeHttpRequest Method="Get" Timeout="20" 
///            UserAgent="MSBuild Extended Tasks"
///            Url="http://www.codeplex.com/msbuildextasks/WorkItem/List2.aspx">
///       <Output PropertyName="ContentType" TaskParameter="ResponseContentType" />
///       <Output PropertyName="ContentLength" TaskParameter="ResponseContentLength" />
///       <Output PropertyName="Method" TaskParameter="ResponseMethod" />
///       <Output PropertyName="eStatusDescription" TaskParameter="ResponseStatusDescription" />
///       <Output PropertyName="StatusCodeName" TaskParameter="ResponseStatusCodeName" />
///       <Output PropertyName="Headers" TaskParameter="ResponseHeaders" />
///       <Output PropertyName="Text" TaskParameter="ResponseText" />
///     </MakeHttpRequest>
/// 
///     <Message Text="ResponseContentType = $(ContentType)" />
///     <Message Text="ResponseContentLength = $(ContentLength)" />
///     <Message Text="ResponseMethod = $(Method)" />
///     <Message Text="ResponseStatusDescription = $(StatusDescription)" />
///     <Message Text="ResponseStatusCodeName = $(StatusCodeName)" />
///     <Message Text="ResponseHeaders = @(Headers)" />
///     <Message Text="ResponseText = $(Text)" />
///   </Target>
/// </Project>]]></code>
/// </example>
/// <seealso cref="http://www.codeplex.com/msbuildextasks"/>
[Browsable ( false ), EditorBrowsable ( EditorBrowsableState.Never )]
class ProjectDoc {

}

namespace MSBuild.Extended.Tasks.Sandcastle {
  /// <summary>
  /// Contains Tasks that simplifiy documenting assemblies using Sandcastle.
  /// </summary>
  class NamespaceDoc {
  }
}

namespace MSBuild.Extended.Tasks.Net {
  /// <summary>
  /// Contains Tasks that pertain to things releated to internet and network activities.
  /// </summary>
  [ Browsable( false ), EditorBrowsable( EditorBrowsableState.Never ) ]
  class NamespaceDoc {
  }
}

namespace MSBuild.Extended.Tasks.String {
  /// <summary>
  /// Contains Tasks that allow different types of <see cref="System.String">String</see> manipulation.
  /// </summary>
  /// <example>
  /// <code language="xml" title="StringReplace Example" htmlDecode="true"><![CDATA[<Target Name="StringReplace">
  ///  <StringReplace Pattern="\." InputString="$(InputString)" Replace="_">
  ///    <Output PropertyName="ResultString" TaskParameter="Result" />
  ///  </StringReplace>
  ///  <Message Text="Result = $(ResultString)" />
  ///</Target>]]></code>
  /// 
  /// </example>
  [Browsable ( false ), EditorBrowsable ( EditorBrowsableState.Never )]
  class NamespaceDoc {
  }
}

namespace MSBuild.Extended.Tasks.Xml {
  /// <summary>
  /// Contains Tasks that perform different Xml related tasks
  /// </summary>
  [Browsable ( false ), EditorBrowsable ( EditorBrowsableState.Never )]
  class NamespaceDoc {
  }
}

namespace MSBuild.Extended.Tasks.Xml.Wix {
  /// <summary>
  /// Contains Tasks that perform Wix related tasks
  /// </summary>
  [Browsable ( false ), EditorBrowsable ( EditorBrowsableState.Never )]
  class NamespaceDoc {
  }
}

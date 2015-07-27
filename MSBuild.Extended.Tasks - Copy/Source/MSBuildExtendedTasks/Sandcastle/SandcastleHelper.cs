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
using System.Diagnostics;
using System.IO;
using System.Xml;
using MSBuild.Extended.Tasks.Xml;
using Microsoft.Build.Framework;

namespace MSBuild.Extended.Tasks.Sandcastle {
  /// <summary>
  /// The type of file names to use for the genereated files.
  /// </summary>
  public enum FileNameStyle {
    /// <summary>
    /// Fiendly file names
    /// </summary>
    Friendly,
    /// <summary>
    /// Use GUID's for file names
    /// </summary>
    Guid,
  }

  /// <summary>
  /// The presentation style to use.
  /// </summary>
  public enum PresentationType {
    /// <summary>
    /// 
    /// </summary>
    Prototype,
    /// <summary>
    /// 
    /// </summary>
    VS2005
  }

  /// <summary>
  /// The style of the documentation
  /// </summary>
  public enum DocumentStyle {
    /// <summary>
    /// Use Standard style
    /// </summary>
    Standard,
    /// <summary>
    /// Use the Visual Studio style.
    /// </summary>
    VisualStudio
  }

  /// <summary>
  /// Link resolving types
  /// </summary>
  public enum ReferenceResolveType {
    /// <summary>
    /// Results in text with no active link
    /// </summary>
    None,
    /// <summary>
    /// Results in links within project using <a href> tag
    /// </summary>
    Local,
    /// <summary>
    /// ms-help style links for HxS
    /// </summary>
    Index,
    /// <summary>
    /// Links to Framework topics in MSDN
    /// </summary>
    MSDN
  }


  internal static class SandcastleHelper {
    private static IBuildEngine buildEngine = null;
    /// <summary>
    /// Gets or sets the build engine.
    /// </summary>
    /// <value>The build engine.</value>
    public static IBuildEngine BuildEngine { get { return buildEngine; } set { buildEngine = value; } }
    /// <summary>
    /// Builds the reflection output.
    /// </summary>
    /// <param name="bin">The sandcastle bin path.</param>
    /// <param name="transforms">The transforms path.</param>
    /// <param name="assemblies">The assemblies.</param>
    /// <param name="dependencies">The dependencies.</param>
    /// <param name="docStyle">The doc style.</param>
    /// <param name="fileNameStyle">The file name style.</param>
    /// <param name="outputPath">The output path.</param>
    public static void BuildReflectionOutput ( string bin, string transforms, string assemblies,
       string dependencies, DocumentStyle docStyle, FileNameStyle fileNameStyle, string outputPath ) {
      RunProcess ( Path.Combine ( bin, "MRefBuilder.exe" ),
        string.Format ( "{0} {1} /out:reflection.org", assemblies, dependencies ),
        outputPath );
      string dxRoot = new DirectoryInfo ( bin ).Parent.FullName;
      // XslTransform /xsl:"C:\Program Files\Sandcastle\ProductionTransforms\AddOverloads.xsl" 
      //    reflection.org /xsl:"C:\Program Files\Sandcastle\ProductionTransforms\AddGuidFilenames.xsl" 
      //    /out:reflection.xml
      //string docStyleFile = docStyle == DocumentStyle.Standard ? "AddOverloads.xsl" : "ApplyVSDocModel.xsl";
      string docStyleFile = docStyle == DocumentStyle.Standard ? "ApplyPrototypeDocModel.xsl" : "ApplyVSDocModel.xsl";
      RunProcess ( Path.Combine ( bin, "XslTransform.exe" ),
        string.Format ( "/xsl:\"{0}\\{1}\" reflection.org /xsl:\"{0}\\Add{2}Filenames.xsl\" /out:reflection.xml", Path.Combine ( dxRoot, transforms ),
          docStyleFile, fileNameStyle.ToString ( ) ),
          outputPath );
    }

    /// <summary>
    /// Creates the manifest.
    /// </summary>
    /// <param name="bin">The bin.</param>
    /// <param name="transform">The transform.</param>
    /// <param name="outputPath">The output path.</param>
    public static void CreateManifest ( string bin, string transform, string outputPath ) {
      string dxRoot = new DirectoryInfo ( bin ).Parent.FullName;
      RunProcess ( Path.Combine ( bin, "XslTransform.exe" ),
        string.Format ( "/xsl:\"{0}\\ReflectionToManifest.xsl\" reflection.xml /out:manifest.xml", Path.Combine ( dxRoot, transform ) ),
        outputPath );
    }

    public static void CreateTOC ( string bin,string transforms, DocumentStyle docStyle, string outputPath ) {
      string dxRoot = new DirectoryInfo ( bin ).Parent.FullName;
      string file = docStyle == DocumentStyle.Standard ? "createPrototypetoc.xsl" : "createvstoc.xsl";
      RunProcess (  Path.Combine( bin,"XslTransform.exe" ),
        string.Format ( "/xsl:\"{0}\\{1}\" reflection.xml /out:toc.xml", transforms, file ), outputPath );
    }

    /// <summary>
    /// Builds the HTML.
    /// </summary>
    /// <param name="bin">The bin.</param>
    /// <param name="sandcastleConfig">The sandcastle config.</param>
    /// <param name="outputPath">The output path.</param>
    public static void BuildHtml ( string bin, string sandcastleConfig, string outputPath ) {
      RunProcess ( Path.Combine ( bin, "BuildAssembler.exe" ),
        string.Format ( "/config:\"{0}\" manifest.xml", sandcastleConfig ),
        outputPath );
    }

    /// <summary>
    /// Custom transform.
    /// </summary>
    /// <param name="bin">The bin.</param>
    /// <param name="transform">The transform.</param>
    /// <param name="helpOutputPath">The help output path.</param>
    /// <param name="taskName">Name of the task.</param>
    public static void CustomTransform ( string bin, string transform, string helpOutputPath, string taskName ) {
      //arg:html=""{1}\html""
      RunProcess ( Path.Combine ( bin, "XslTransform.exe" ),
        string.Format ( @"/xsl:""{0}"" reflection.xml /out:""{1}\html\{2}""", transform, helpOutputPath, taskName ),
        helpOutputPath );
    }

    /// <summary>
    /// Generates the HHTOC.
    /// </summary>
    /// <param name="bin">The bin.</param>
    /// <param name="transformsPath">The transforms path.</param>
    /// <param name="helpOutputPath">The help output path.</param>
    /// <param name="helpName">Name of the help.</param>
    public static void GenerateHHTOC ( string bin, string transformsPath, string helpOutputPath, string helpName ) {
      string dxRoot = new DirectoryInfo ( bin ).Parent.FullName;
      RunProcess ( Path.Combine ( bin, "XslTransform.exe" ),
      string.Format ( @"/xsl:""{0}\TocToChmContents.xsl"" toc.xml /arg:html=.\html\ /out:""{1}\{2}.hhc""", transformsPath, helpOutputPath, helpName ),
        helpOutputPath );
      RunProcess ( Path.Combine ( bin, "XslTransform.exe" ),
        string.Format ( @"/xsl:""{0}\ReflectionToChmIndex.xsl"" reflection.xml /arg:html=.\html\ /out:""{1}\{2}.hhk""", transformsPath, helpOutputPath, helpName ),
        helpOutputPath );

      // 	<xsl:param name="project" select="string('test')" />
      string xslFile = string.Format ( @"{0}\ReflectionToChmProject.xsl", transformsPath );
      SetXmlElementAttribute sxea = new SetXmlElementAttribute ( );

      sxea.XmlFile = xslFile;
      sxea.Prefix = "xsl";
      sxea.NamespaceURI = "http://www.w3.org/1999/XSL/Transform";
      sxea.XPath = "/xsl:stylesheet/xsl:param[@name='project']";
      sxea.Name = "select";
      sxea.Value = string.Format ( "string('{0}')", helpName );
      sxea.Execute ( );

      RunProcess ( Path.Combine ( bin, "XslTransform.exe" ),
        string.Format ( @"/xsl:""{0}\ReflectionToChmProject.xsl"" reflection.xml /arg:html=.\html\ /out:""{1}\{2}.hhp""", transformsPath, helpOutputPath, helpName ),
        helpOutputPath );
      sxea.Value = "string('test')";
      sxea.Execute ( );

    }

    /// <summary>
    /// Compiles the CHM.
    /// </summary>
    /// <param name="hhcBin">The HHC bin.</param>
    /// <param name="outputPath">The output path.</param>
    /// <param name="helpName">Name of the help.</param>
    public static void CompileCHM ( string hhcBin, string outputPath, string helpName ) {
      RunProcess ( Path.Combine ( hhcBin, "hhc.exe" ), string.Format ( @"""{0}.hhp""", helpName ), outputPath );
    }

    /// <summary>
    /// Generates the HCTOC.
    /// </summary>
    /// <param name="bin">The bin.</param>
    /// <param name="transformsPath">The transforms path.</param>
    /// <param name="helpOutputPath">The help output path.</param>
    /// <param name="helpName">Name of the help.</param>
    public static void GenerateHCTOC ( string bin, string transformsPath, string helpOutputPath, string helpName ) {
      string dxRoot = new DirectoryInfo ( bin ).Parent.FullName;

      RunProcess ( Path.Combine ( bin, "XslTransform.exe" ),
  string.Format ( @"/xsl:""{0}\TocToHxSContents.xsl"" toc.xml /arg:html=.\html\ /out:""{1}\{2}.HxT""", transformsPath, helpOutputPath, helpName ),
  helpOutputPath );

      // Presentation
      // XslTransform /xsl:..\..\ProductionTransforms\TocToHxSContents.xsl toc.xml /out:Output\test.HxT
    }

    /// <summary>
    /// Creates the HX files.
    /// </summary>
    /// <param name="bin">The bin.</param>
    /// <param name="docStyle">The doc style.</param>
    /// <param name="helpOutputPath">The help output path.</param>
    /// <param name="helpName">Name of the help.</param>
    public static void CreateHXFiles ( string bin, DocumentStyle docStyle, string helpOutputPath, string helpName ) {
      string dxRoot = new DirectoryInfo ( bin ).Parent.FullName;
      string docFile = docStyle == DocumentStyle.VisualStudio ? "vs2005" : "prototype";
      DirectoryInfo dir = new DirectoryInfo ( Path.Combine ( dxRoot, string.Format ( "Presentation\\{0}\\Hxs", docFile ) ) );
      foreach ( FileInfo file in dir.GetFiles () ) {
        string newFileName = Path.GetFileName( file.FullName ).Replace( "test", helpName );
        FileInfo newFile = file.CopyTo ( Path.Combine ( helpOutputPath, newFileName ), true );
        if ( string.Compare ( Path.GetExtension ( newFile.FullName ), ".hxc", true ) == 0 ) {
          XmlDocument doc = new XmlDocument ();
          doc.Load ( newFile.OpenRead () );
          doc.DocumentElement.SetAttribute ( "Title", helpName );
          XmlElement ele = doc.DocumentElement.SelectSingleNode ( "CompilerOptions" ) as XmlElement;
          if ( ele != null ) {
            ele.SetAttribute ( "OutputFile", string.Format ( "{0}.HxS", helpName ) );
            XmlElement t = ele.SelectSingleNode ( "IncludeFile" ) as XmlElement;
            if( t!= null )
              t.SetAttribute ( "File", string.Format ( "{0}.HxF", helpName ) );
          }

          ele = doc.DocumentElement.SelectSingleNode ( "TOCDef" ) as XmlElement;
          if ( ele != null )
            ele.SetAttribute ( "File", string.Format ( "{0}.HxT", helpName ) );

          XmlNodeList nodes = doc.DocumentElement.SelectNodes ( "KeywordIndexDef" );
          foreach ( XmlElement tele in nodes ) {
            string tVal = tele.GetAttribute ( "File" ).Replace ( "test", helpName );
            tele.SetAttribute ( "File", tVal );
          }
          doc.Save ( newFile.FullName );
        }
      }
    }

    /// <summary>
    /// Compiles the HX.
    /// </summary>
    /// <param name="hxcBin">The HXC bin.</param>
    /// <param name="helpOutputPath">The help output path.</param>
    /// <param name="helpName">Name of the help.</param>
    public static void CompileHX ( string hxcBin, string helpOutputPath, string helpName ) {
      //throw new NotImplementedException ( "This method is not yet implemented." );
    }

    /// <summary>
    /// Empties the output structure.
    /// </summary>
    /// <param name="helpOutputPath">The help output path.</param>
    /// <param name="presentationPath">The presentation path.</param>
    /// <param name="presentationType">Type of the presentation.</param>
    public static void EmptyOutputStructure ( string helpOutputPath, string presentationPath, PresentationType presentationType ) {
      //Console.WriteLine ( "EmptyOutputStructure" );
      DirectoryInfo dir = new DirectoryInfo ( helpOutputPath );

      foreach ( DirectoryInfo sub in dir.GetDirectories ( ) )
        sub.Delete ( true );

      DirectoryInfo iconsDir = dir.CreateSubdirectory ( "icons" );
      DirectoryInfo scriptsDir = dir.CreateSubdirectory ( "scripts" );
      DirectoryInfo htmlDir = dir.CreateSubdirectory ( "html" );
      DirectoryInfo stylesDir = dir.CreateSubdirectory ( "styles" );
      string presentationDirectory = Path.Combine ( presentationPath, presentationType.ToString ( ) );
      CopyFilesFromDirectory ( Path.Combine ( presentationDirectory, "icons" ), "*.*", iconsDir.FullName );
      CopyFilesFromDirectory ( Path.Combine ( presentationDirectory, "scripts" ), "*.*", scriptsDir.FullName );
      CopyFilesFromDirectory ( Path.Combine ( presentationDirectory, "styles" ), "*.*", stylesDir.FullName );

      string assemblyPresentationPath = Path.Combine ( Path.GetDirectoryName ( typeof ( SandcastleHelper ).Assembly.Location ), @"Sandcastle\Presentation" );
      CopyFilesFromDirectory ( Path.Combine ( assemblyPresentationPath, "icons" ), "*.gif", iconsDir.FullName );
      CopyFilesFromDirectory ( Path.Combine ( assemblyPresentationPath, "styles" ), "*.css", stylesDir.FullName );
      CopyFilesFromDirectory ( Path.Combine ( assemblyPresentationPath, "scripts" ), "*.js", scriptsDir.FullName );
    }

    /// <summary>
    /// Copies the files from directory.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="matchPattern">The match pattern.</param>
    /// <param name="destPath">The dest path.</param>
    public static void CopyFilesFromDirectory ( string sourcePath, string matchPattern, string destPath ) {
      DirectoryInfo sd = new DirectoryInfo ( sourcePath );
      if ( sd.Exists ) {
        foreach ( FileInfo file in sd.GetFiles ( string.IsNullOrEmpty ( matchPattern ) ? "*.*" : matchPattern ) )
          file.CopyTo ( Path.Combine ( destPath, file.Name ), true );
      }
    }

    /// <summary>
    /// Creates the sandcastle config.
    /// </summary>
    /// <Exec Command="WScript.exe 
    /// &quot;$(MSBuildExtensionsPath)\Sandcastle\SandcastleConfigurator.vbs&quot; 
    /// /in:&quot;$(SandcastleOutputStructure)\Configuration\sandcastle.config&quot; 
    /// /out:sandcastle.config 
    /// /ref:&quot;$(ReferenceContentFilePath)&quot; 
    /// /shared:&quot;$(SharedContentFilePath)&quot; 
    /// /output:&quot;$(HelpOutputPath)&quot; 
    /// /path:&quot;$(SandcastlePath)&quot; 
    /// &quot;@(DocFiles, '&quot; &quot;')&quot;" />
    public static void CreateSandcastleConfig ( string sandcastlePath, string presentationPath,
        PresentationType presentationType, string outputPath, string[] docFiles, ReferenceResolveType localResolve, ReferenceResolveType externalResolve ) {
      try {
        //Console.WriteLine ( "CreateSandcastleConfig" );
        string nodeType = "comments";
        string fullPresentationPath = Path.Combine ( presentationPath, presentationType.ToString ( ) );
        string sharedContentPath = Path.Combine ( fullPresentationPath, @"content\shared_content.xml" );
        string refContentPath = Path.Combine ( fullPresentationPath, @"content\reference_content.xml" );
        string oConfigFile = Path.Combine ( fullPresentationPath, @"Configuration\sandcastle.config" );

        XmlDocument orrigConfig = new XmlDocument ( );
        orrigConfig.Load ( oConfigFile );
        orrigConfig.PreserveWhitespace = true;
        XmlNodeList xmlNodes = orrigConfig.SelectNodes ( "//component[@type='Microsoft.Ddue.Tools.SharedContentComponent']/content" );
        foreach ( XmlElement ele in xmlNodes )
          ele.ParentNode.RemoveChild ( ele );

        XmlElement sourceNode = orrigConfig.SelectSingleNode ( "//component[@type='Microsoft.Ddue.Tools.SharedContentComponent']" ) as XmlElement;
        XmlElement contentNode = orrigConfig.CreateElement ( "content" );
        contentNode.SetAttribute ( "file", sharedContentPath );
        sourceNode.AppendChild ( contentNode );
        contentNode = orrigConfig.CreateElement ( "content" );
        contentNode.SetAttribute ( "file", refContentPath );
        sourceNode.AppendChild ( contentNode );

        xmlNodes = orrigConfig.SelectNodes ( "//component[@type='Microsoft.Ddue.Tools.SaveComponent']/save" );
        foreach ( XmlElement ele in xmlNodes )
          ele.ParentNode.RemoveChild ( ele );

        sourceNode = orrigConfig.SelectSingleNode ( "//component[@type='Microsoft.Ddue.Tools.SaveComponent']" ) as XmlElement;
        contentNode = orrigConfig.CreateElement ( "save" );
        contentNode.SetAttribute ( "base", Path.Combine ( outputPath, @"html" ) );
        contentNode.SetAttribute ( "path", "concat(/html/head/meta[@name='guid']/@content,'.htm')" );
        contentNode.SetAttribute ( "indent", "false" );
        contentNode.SetAttribute ( "omit-xml-declaration", "true" );
        sourceNode.AppendChild ( contentNode );
        // <index name="comments" value="/doc/members/member" key="@name" cache="100">
        // <data files="comments.xml" />
        sourceNode = orrigConfig.SelectSingleNode ( string.Format ( "//index[@name='{0}']", nodeType ) ) as XmlElement;
        sourceNode.RemoveChild ( sourceNode.SelectSingleNode ( "data[@files='comments.xml']" ) );
        foreach ( string dataFile in docFiles ) {
          XmlElement dataNode = orrigConfig.CreateElement ( "data" );
          dataNode.SetAttribute ( "files", dataFile );
          sourceNode.AppendChild ( dataNode );
        }

        // change the reference resolve types 
        // //component[@type='Microsoft.Ddue.Tools.ResolveReferenceLinksComponent']/targets[@files='reflection.xml']
        // //component[@type='Microsoft.Ddue.Tools.ResolveReferenceLinksComponent']/targets[@files='%DXROOT%\Data\cpref_reflection\*.xml']

        XmlElement localReference = orrigConfig.SelectSingleNode ( @"//component[@type='Microsoft.Ddue.Tools.ResolveReferenceLinksComponent']/targets[@files='reflection.xml']" ) as XmlElement;
        XmlElement externalReference = orrigConfig.SelectSingleNode ( @"//component[@type='Microsoft.Ddue.Tools.ResolveReferenceLinksComponent']/targets[@files='%DXROOT%\Data\cpref_reflection\*.xml']" ) as XmlElement;
        if ( localReference != null )
          localReference.SetAttribute ( "type", localResolve.ToString ( ).ToLower ( ) );
        if ( externalReference != null )
          externalReference.SetAttribute ( "type", externalResolve.ToString ( ).ToLower ( ) );


        orrigConfig.Save ( Path.Combine ( outputPath, "sandcastle.config" ) );
      } catch ( Exception ex ) {
        if ( BuildEngine != null )
          BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( ex.ToString ( ), string.Empty, "CreateSandcastleConfig", MessageImportance.High ) );
      }
    }

    /// <summary>
    /// Adds the code highlighter to sandcastle config.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="config">The config.</param>
    public static void AddCodeHighlighterToSandcastleConfig ( string assembly/*, string type*/, string config ) {
      XmlDocument doc = new XmlDocument ( );
      doc.Load ( config );
      string typeFormat = "MSBuild.Extended.Tasks.Components.{0}";
      XmlElement sourceNode = doc.SelectSingleNode ( "//component[@type='Microsoft.Ddue.Tools.TransformComponent']" ) as XmlElement;
      XmlElement componentsNode = doc.SelectSingleNode ( "/configuration/dduetools/builder/components" ) as XmlElement;
      if ( sourceNode != null && componentsNode != null ) {
        XmlElement codeNode = doc.CreateElement ( "component" );
        codeNode.SetAttribute ( "type", string.Format ( typeFormat, "CodeBlockComponent" ) );
        codeNode.SetAttribute ( "assembly", assembly );

        componentsNode.InsertBefore ( codeNode, sourceNode );

        XmlElement saveComponent = doc.SelectSingleNode ( "//component[@type='Microsoft.Ddue.Tools.SaveComponent']/save" ) as XmlElement;
        string outputPath = saveComponent.GetAttribute ( "base" );

        XmlElement comNode = doc.CreateElement ( "component" );
        comNode.SetAttribute ( "type", string.Format ( typeFormat, "CodeBlockPostTransformComponent" ) );
        comNode.SetAttribute ( "assembly", assembly );
        XmlElement outElement = doc.CreateElement ( "outputPath" );
        outElement.SetAttribute ( "value", outputPath );
        comNode.AppendChild ( outElement );
        componentsNode.InsertAfter ( comNode, sourceNode );

      }
      doc.Save ( config );
    }

    /// <summary>
    /// Runs the process.
    /// </summary>
    /// <param name="exe">The exe.</param>
    /// <param name="args">The args.</param>
    /// <param name="workingDirectory">The working directory.</param>
    /// <returns></returns>
    private static int RunProcess ( string exe, string args, string workingDirectory ) {
      string dxRoot = new DirectoryInfo ( Path.GetDirectoryName ( exe ) ).Parent.FullName;
      Process process = new Process ( );
      using ( process ) {
        ProcessStartInfo psi = new ProcessStartInfo ( );
        if ( !psi.EnvironmentVariables.ContainsKey ( "DXROOT" ) )
          psi.EnvironmentVariables.Add ( "DXROOT", dxRoot );
        psi.CreateNoWindow = true;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.FileName = exe;
        psi.WorkingDirectory = workingDirectory;
        psi.Arguments = args;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        process.StartInfo = psi;
        //Console.WriteLine ( "{0} {1}", psi.FileName, psi.Arguments );
        if ( BuildEngine != null )
          BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( string.Format ( "{0} {1}", psi.FileName, psi.Arguments ), string.Empty, "CompileCHM:hhc", MessageImportance.Normal ) );
        process.Start ( );
        while ( !process.StandardOutput.EndOfStream ) {
          string line = process.StandardOutput.ReadLine ( );
          //Console.WriteLine ( line );
          if ( BuildEngine != null )
            BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( line, string.Empty, string.Format ( "RunProcess:{0}", Path.GetFileNameWithoutExtension ( exe ) ), MessageImportance.Normal ) );
        }
        while ( !process.StandardError.EndOfStream ) {
          string line = process.StandardError.ReadLine ( );
          //Console.WriteLine ( line );
          if ( BuildEngine != null )
            BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( line, string.Empty, string.Format ( "RunProcess:{0}", Path.GetFileNameWithoutExtension ( exe ) ), MessageImportance.Normal ) );
        }
        process.WaitForExit ( );
        if ( BuildEngine != null )
          BuildEngine.LogMessageEvent ( new BuildMessageEventArgs ( string.Format ( "RunProcess:{1} exited with code {0}", process.ExitCode, Path.GetFileNameWithoutExtension ( exe ) ),
            string.Empty, "CompileCHM", MessageImportance.Normal ) );
        return process.ExitCode;
      }
    }
  }
}

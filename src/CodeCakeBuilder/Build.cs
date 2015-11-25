using Cake.Common;
using Cake.Common.Solution;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Common.Diagnostics;
using SimpleGitVersion;
using Code.Cake;
using Cake.Common.Build.AppVeyor;
using Cake.Common.Tools.NuGet.Pack;
using System;
using System.Linq;
using Cake.Common.Tools.SignTool;
using Cake.Core.Diagnostics;
using Cake.Common.Text;
using Cake.Common.Tools.NuGet.Push;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;

namespace CodeCake
{
    /// <summary>
    /// Sample build "script".
    /// It can be decorated with AddPath attributes that inject paths into the PATH environment variable. 
    /// </summary>
    [AddPath( "CodeCakeBuilder/Tools" )]
    public class Build : CodeCakeHost
    {
        public Build()
        {
            DNXSolution dnxSolution = null;
            IEnumerable<DNXProjectFile> projectsToPublish = null;
            SimpleRepositoryInfo gitInfo = null;
            string configuration = null;

            Setup( () =>
            {
                dnxSolution = Cake.GetDNXSolution( p => p.ProjectName != "CodeCakeBuilder" );
                if( !dnxSolution.IsValid ) throw new Exception( "Unable to initialize solution." );
                projectsToPublish = dnxSolution.Projects.Where( p => !p.ProjectName.EndsWith( "Tests" ) );
            } );

            Teardown( () =>
            {
                dnxSolution.RestoreProjectFiles();
            } );

            Task( "Verbosity" )
                .Does( () =>
                {
                    Console.WriteLine( "Identified Projects in solution : " );
                    foreach( DNXProjectFile project in dnxSolution.Projects)
                    {
                        Console.WriteLine( project.ProjectName );
                    }
                } );
            Task( "Check-Repository" )
              .Does( () =>
              {
                  gitInfo = dnxSolution.RepositoryInfo;
                  if( !gitInfo.IsValid )
                  {
                      if( Cake.IsInteractiveMode()
                          && Cake.ReadInteractiveOption( "Repository is not ready to be published. Proceed anyway?", 'Y', 'N' ) == 'Y' )
                      {
                          Cake.Warning( "GitInfo is not valid, but you choose to continue..." );
                      }
                      else throw new Exception( "Repository is not ready to be published." );
                  }
                  configuration = gitInfo.IsValidRelease && gitInfo.PreReleaseName.Length == 0 ? "Release" : "Debug";
                  Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}",
                      projectsToPublish.Count(),
                      gitInfo.SemVer,
                      configuration,
                      String.Join( ", ", projectsToPublish.Select( p => p.ProjectName ) ) );
              } );

            Task( "Set-ProjectVersion" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    if( dnxSolution.UpdateProjectFiles() > 0 )
                    {
                        // dnu restore
                        Cake.DNURestore(restore =>
                        {
                            restore.Quiet = true;
                            restore.ProjectPaths.UnionWith( dnxSolution.Projects.Select( p => p.ProjectFilePath ) );
                            Console.WriteLine( "dnu restore" );
                        } );
                    }
                } );

            Task( "Clean" )
                .Does( () =>
                {
                    Code.Cake.CommandRunner.RunCmd( Cake, "echo %cd%" );
                    Cake.CleanDirectories( "../*/*/*/*/*/*/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "../*/*/*/*/*/*/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                } );

            Task( "Unit-Testing" )
               .IsDependentOn( "Check-Repository" )
               .Does( () =>
               {
                   var testProjects = dnxSolution.Projects.Where( p => p.ProjectName.EndsWith( ".Tests" ) );
                   foreach( var p in testProjects )
                   {
                       foreach( var framework in p.Frameworks )
                       {
                           Cake.DNXRun( c => {
                               c.Arguments = "test";
                               c.Configuration = configuration;
                               c.Framework = null;
                               c.Project = p.ProjectFilePath;
                           } );
                       }
                   }
               } );
            Task( "Build-And-Pack" )
               .IsDependentOn( "Clean" )
               .IsDependentOn( "Unit-Testing" )
               .IsDependentOn( "Set-ProjectVersion" )
               .Does( () =>
               {
                   Cake.DNUBuild( c =>
                   {
                       c.GeneratePackage = true;
                       c.Configurations.Add( configuration );
                       c.ProjectPaths.UnionWith( dnxSolution.Projects.Select( p => p.ProjectDir ) );
                       c.Quiet = true;
                   } );
               } );

         
            Task( "Default" ).IsDependentOn( "Verbosity" ).IsDependentOn( "Build-And-Pack" );


    }
    }

}
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
            Console.WriteLine( "ICI" );
            var nugetOutputDir = Cake.Directory( "CodeCakeBuilder/Release" );
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


            Task( "Clean" )
              .Does( () =>
              {
                  Console.WriteLine( "Coucou la famille" );
                  Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                  Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                  Cake.DeleteFiles( "Tests/**/TestResult.xml" );
              } );


            Task( "Build" )
                .IsDependentOn( "Clean" )
                .Does( () =>
                {
                    Console.WriteLine( "Build" );
                } );

         
            Task( "Default" ).IsDependentOn( "Clean" );

        }
    }

}
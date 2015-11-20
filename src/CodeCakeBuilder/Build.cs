using Cake.Common;
using Cake.Common.Solution;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Code.Cake;
using Cake.Common.Diagnostics;
using Cake.Common.Tools.NuGet.Pack;
using System.Linq;
using Cake.Core.Diagnostics;
using Cake.Common.Tools.NuGet.Restore;
using System;
using Cake.Common.Tools.NuGet.Push;
using CodeCake;

namespace CodeCakeBuilder
{
   
    /// <summary>
    /// Sample build "script".
    /// Build scripts can be decorated with AddPath attributes that inject existing paths into the PATH environment variable. 
    /// </summary>
    [AddPath( "CodeCakeBuilder/Tools" )]
    [AddPath( "packages/**/tools*" )]
    public class Build : CodeCakeHost
    {
        public Build()
        {
            // The configuration ("Debug", etc.) defaults to "Release".
            var configuration = Cake.Argument( "configuration", "Release" );

            // Git .ignore file should ignore this folder.
            // Here, we name it "Releases" (default , it could be "Artefacts", "Publish" or anything else, 
            // but "Releases" is by default ignored in https://github.com/github/gitignore/blob/master/VisualStudio.gitignore.
            var releasesDir = Cake.Directory( "CodeCakeBuilder/Releases" );

            Task( "Clean" )
                .Does( () =>
                {
                    // Avoids cleaning CodeCakeBuilder itself!
                    Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( releasesDir );
                } );

           
            Task( "Build" )
                .IsDependentOn( "Clean" )
                .Does( () =>
                {
                    Cake.Information( "Building all existing .sln files at the root level with '{0}' configuration (excluding this builder application).", configuration );
                    foreach( var sln in Cake.GetFiles( "*.sln" ) )
                    {
                        using( var tempSln = Cake.CreateTemporarySolutionFile( sln ) )
                        {
                            // Excludes "CodeCakeBuilder" itself from compilation!
                            tempSln.ExcludeProjectsFromBuild( "CodeCakeBuilder" );
                            Cake.MSBuild( tempSln.FullPath, new MSBuildSettings()
                                    .SetConfiguration( configuration )
                                    .SetVerbosity( Verbosity.Minimal )
                                    .SetMaxCpuCount( 1 ) );
                        }
                    }
                } );

         
            Task( "Default" ).IsDependentOn( "Clean" );

        }
    }

}
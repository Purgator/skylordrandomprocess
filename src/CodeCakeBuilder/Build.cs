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
using Renci.SshNet;

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
                projectsToPublish = dnxSolution.Projects.Where( p => !p.ProjectName.EndsWith( ".Tests" ) );
            } );

            Teardown( () =>
            {
                dnxSolution.RestoreProjectFiles();
                //Loic est pd
            } );

            Task( "Verbosity" )
                .Does( () =>
                {
                    Console.WriteLine( "Identified Projects in solution : " );
                    foreach( DNXProjectFile project in dnxSolution.Projects )
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
                        Cake.DNURestore( restore =>
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
                  
                        Cake.CleanDirectories( "*/bin" + configuration,
                            ( d ) =>
                            {
                                return (d.Path.Segments.Contains( "ITI.SkyLord.TestAvecEntity" ) || d.Path.Segments.Contains( "ITI.SkyLord.TestAvecEntity.Tests" ));
                            } );

                    Cake.CleanDirectories( "*/obj" + configuration,
                           ( d ) =>
                           {
                               return (d.Path.Segments.Contains( "ITI.SkyLord.TestAvecEntity" ) || d.Path.Segments.Contains( "ITI.SkyLord.TestAvecEntity.Tests" ));
                           } );
                } );

            Task( "Unit-Testing" )
               .IsDependentOn( "Set-ProjectVersion" )
               .Does( () =>
               {
                   var testProjects = dnxSolution.Projects.Where( p => p.ProjectName.EndsWith( ".Tests" ) );
                   foreach( var p in testProjects )
                   {
                       foreach( var framework in p.Frameworks )
                       {
                           Cake.DNXRun( c =>
                           {
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
               .IsDependentOn( "Set-ProjectVersion" )
               .IsDependentOn( "Unit-Testing" )
               .Does( () =>
               {
                   Cake.DNUBuild( c =>
                   {
                       c.GeneratePackage = true;
                     //  configuration = "Release";
                       c.Configurations.Add( configuration );
                       c.ProjectPaths.UnionWith( projectsToPublish.Select( p => p.ProjectDir ) );
                       c.Quiet = true;
                   } );
               } );

            Task( "Deploy" )
           .IsDependentOn( "Build-And-Pack")
           .Does( () =>
           {

               var login = Environment.GetEnvironmentVariable("login");
               var password= Environment.GetEnvironmentVariable("password");

                    /* 
                      Code référence
                      http://stackoverflow.com/questions/11169396/c-sharp-send-a-simple-ssh-command
                    */
                    // Se connecte en SSH à notre serveur de prod
                    using( SshClient mySSH = new SshClient( "10.8.99.163", 22, login, password ) )
               {
                   mySSH.Connect();
                   string stopServer = "killall -SIGSTOP coreclr";
                   string sendPackages = "";
                   string updateDatabase = "";
                   string runServer = "dnx web -p \"pathOfTheProject\"";

                        // Arrête le serveur qui tourne
                        mySSH.RunCommand( stopServer );

                        // Envoi le package sur le serveur de prod en SFTP
                        /* 
                            POUR LA DOC SFTP 
                            https://sshnet.codeplex.com/wikipage?title=Draft%20for%20Documentation%20page
                        */
                   mySSH.RunCommand( sendPackages );

                        // dnu install ??


                        // dnx ef database update sur le serveur de prod
                        mySSH.RunCommand( updateDatabase );

                        // dnx web pour lancer le serveur OTD
                        mySSH.RunCommand( runServer );

                        // Fin du déploiement
                        mySSH.Disconnect();
               }
           } );

            Task( "Default" ).IsDependentOn( "Verbosity" ).IsDependentOn( "Deploy" );


        }
    }

}
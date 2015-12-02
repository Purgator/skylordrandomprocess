﻿using Cake.Common;
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
using Cake.Core.IO;
using System.Security.Cryptography;
using System.Text;

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

            Task( "Clean" )
              .Does( () =>
              {
                  Cake.CleanDirectories( "*/bin" + configuration,
                          ( d ) =>
                          {
                              foreach( DNXProjectFile projet in dnxSolution.Projects )
                              {
                                  if( d.Path.Segments.Contains( projet.ProjectName ) )
                                      return true;
                              }
                              return false;
                          } );

                  Cake.CleanDirectories( "*/obj" + configuration,
                         ( d ) =>
                         {
                             foreach( DNXProjectFile projet in dnxSolution.Projects )
                             {
                                 if( d.Path.Segments.Contains( projet.ProjectName ) )
                                     return true;
                             }
                             return false;
                         } );
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
                               c.Framework = framework;
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
          //.IsDependentOn( "Build-And-Pack" )
           .Does( () =>
           {

               string login = Environment.GetEnvironmentVariable("login");
               string password = Environment.GetEnvironmentVariable("password");
               login = "ubuntu";
               password = "proutogobelin";
               string ip = "labo.itinet.fr";
               int port = 5322;

              PrivateKeyFile privateKeyFile = new PrivateKeyFile("skylordPrivateKeyOtDOpenSsh.ppk", password);

               AuthenticationMethod authentification = new PrivateKeyAuthenticationMethod(login, privateKeyFile);

               ConnectionInfo connection = new ConnectionInfo(ip, port, login, authentification);

               DNXSupportOTD.RunSuccessfullCmd( Cake, "dnu publish" );

               using( ScpClient scp = new ScpClient( connection ) )
               {
                   scp.Connect();
                   scp.Upload( new DirectoryInfo( "ITI.SkyLord.TestAvecEntity\\bin\\output\\approot" ), "." );
                   scp.Disconnect();
               }

              // Se connecte en SSH à notre serveur de prod
               using( SshClient mySSH = new SshClient( connection ) )
               {
                   mySSH.Connect();
                   string stopServer = "killall -SIGSTOP coreclr";
                   string sendPackages = "";
                   string updateDatabase = "";
                   string runServer = "dnx web -p \"pathOfTheProject\"";

                   //  ScpClient scp = new ScpClient();
                   // Arrête le serveur qui tourne
                   //mySSH.RunCommand( stopServer );

                   // Envoi le package sur le serveur de prod en SFTP

                   //  mySSH.RunCommand( sendPackages );

                   // dnu install ??



                 //  mySSH.RunCommand( "rm testOTD" );

                   // dnx ef database update sur le serveur de prod
                   // mySSH.RunCommand( updateDatabase );

                   // dnx web pour lancer le serveur OTD
                   // mySSH.RunCommand( runServer );

                   // Fin du déploiement
                   mySSH.Disconnect();
               }
           } );

            Task( "Default" ).IsDependentOn( "Verbosity" ).IsDependentOn( "Deploy" );


        }
    }


    public class DecryptHelper : IDisposable
    {
        StreamReader StreamReader { get; }
        CryptoStream crStream { get; }

        public DecryptHelper(string path) : this(new FileStream(path, FileMode.Open, FileAccess.Read)) { }

        public DecryptHelper(FileStream fileStream)
        {
            DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

            cryptic.Key = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");
            cryptic.IV = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");

            CryptoStream crStream = new CryptoStream(fileStream, cryptic.CreateDecryptor(), CryptoStreamMode.Read);

            StreamReader = new StreamReader(crStream);
        }



        public void Dispose()
        {
            crStream.Close();
            StreamReader.Close();
        }

        public void Decrypt()
        {
            

            string data = reader.ReadToEnd();

            
        }
    }
    

}
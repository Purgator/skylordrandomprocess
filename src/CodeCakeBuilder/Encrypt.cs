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
using Cake.Core.IO;
using CodeCake;
using System.Text;

namespace CodeCakeBuilder
{
    public class Encrypt : CodeCakeHost
    {
        public Encrypt()
        {
            Task( "Default" )
                .Does( () =>
                {
                    string targetPath = Cake.Arguments.GetArgument("targetPath");
                    string outPath = Cake.Arguments.GetArgument("outPath");
                    string passphrase = Cake.Arguments.GetArgument("passphrase");

                    if( String.IsNullOrEmpty( targetPath ) || String.IsNullOrEmpty( passphrase ) )
                    {
                        throw new ArgumentException( "targetPath or passphrase expected. Use command : CodeCakeBuilder Encrypt -targetPath=\"Path\" -passphrase=\"passphrase\"" );
                    }
                    if( String.IsNullOrEmpty( outPath ) )
                    {
                        outPath = Environment.CurrentDirectory + "\\" + targetPath.Split( '\\' ).Last() + ".enc";
                    }
                    Console.WriteLine( "Encryting " + targetPath + " to " + outPath );
                    Cake.SecureFileCrypt( targetPath, outPath, passphrase );

                } );

        }
    }
}


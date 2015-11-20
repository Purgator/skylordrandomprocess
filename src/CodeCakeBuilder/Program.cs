using System;
using System.IO;
using System.Linq;
using Code.Cake;
using CodeCake;
using Microsoft.Extensions.PlatformAbstractions;

namespace CodeCakeBuilder
{
    public class Program
    {
        string _solutionDir;

        public Program( IApplicationEnvironment appEnv )
        {
            _solutionDir = Path.GetDirectoryName( appEnv.ApplicationBasePath );
        }
        public int Main( string[] args )
        {
            Console.ReadKey();
#if DNX50
            var app = new CodeCakeApplication( _solutionDir, typeof(Program).Assembly );
#else
            var app = new CodeCakeApplication();
#endif
            bool interactive = !args.Contains( '-' + InteractiveAliases.NoInteractionArgument, StringComparer.OrdinalIgnoreCase );
            int result = app.Run( args );
            Console.WriteLine();
            if( interactive )
            {
                Console.WriteLine( "Hit any key to exit. (Use -{0} parameter to exit immediately)", InteractiveAliases.NoInteractionArgument );
                Console.ReadKey();
            }
            return result;
        }
    }
}
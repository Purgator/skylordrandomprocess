using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using NUnit;

namespace Tests
{
    public class Program
    {
        public int Main(string[] args)
        {
            return new AutoRun().Execute(args);
        }
    }
}

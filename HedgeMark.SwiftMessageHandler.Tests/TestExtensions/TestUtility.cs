using System;
using System.IO;

namespace HedgeMark.SwiftMessageHandler.Tests.TestExtensions
{
    public class TestUtility
    {
        static TestUtility()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\HedgeMark.Operations.Secure.Middleware.Tests\\");
            path = path.Replace("TestResults", "UnitTests");
            DirectoryInfo info = new DirectoryInfo(path);
            AssemblyDirectory = info.FullName.Replace("\\HedgeMark.Operations.Secure.Middleware.Tests\\HedgeMark.Operations.Secure.Middleware.Tests\\", "\\HedgeMark.Operations.Secure.Middleware.Tests\\");

            /*Load Prowide dll into the assembly*/
            //ikvm.runtime.Startup.addBootClassPathAssembly(Assembly.Load("pw-swift-core-SRU2018-7.10.3.ikvm.a"));
        }
     
        public static string AssemblyDirectory { get; private set; }
    }
}

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
        }
     
        public static string AssemblyDirectory { get; private set; }
    }
}

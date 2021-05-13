using System.IO;
using NUnit.Framework;

namespace HedgeMark.SwiftMessageHandler.Tests.TestExtensions
{
    public class TestUtility
    {
        static TestUtility()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "..\\..\\..\\HedgeMark.SwiftMessageHandler.Tests\\");
            path = path.Replace("TestResults", "UnitTests");
            DirectoryInfo info = new DirectoryInfo(path);
            AssemblyDirectory = info.FullName.Replace("\\HedgeMark.SwiftMessageHandler.Tests\\HedgeMark.SwiftMessageHandler.Tests\\", "\\HedgeMark.SwiftMessageHandler.Tests\\");
        }

        public static string AssemblyDirectory { get; private set; }
    }
}

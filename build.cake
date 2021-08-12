#addin "nuget:?package=Cake.Sonar&version=1.1.25"

#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.8.0"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.11.1"
#tool "nuget:?package=NUnit.Extension.NUnitV2Driver&version=3.8.0"
#tool "nuget:?package=NUnit.Extension.VSProjectLoader&version=3.8.0"
#tool "nuget:?package=OpenCover&version=4.7.922"
#tool "nuget:?package=ReportGenerator&version=4.5.6"

var target = Argument("target", "package");
var configuration = EnvironmentVariable<string>("configuration", "Debug");
var solution = File(Argument("sln", GetFiles("./*.sln").FirstOrDefault().ToString()));
var testPattern = Argument("test", "*.Tests");
var sonarUrl = Argument("sonarurl", "https://sonarqube.bnymellon.net");
var sonarExclude = Argument("sonarexclude", "HedgeMark.Operations.Secure*/Scripts/**/*");
var namespacePattern = Argument("namespace", "HedgeMark.Operations.Secure" + "*");
var testAssemblies = "./**/bin/" + configuration + "/" + testPattern + ".dll";
var groupId = Argument("groupid", OrFromPom("//*[local-name()='project' and namespace-uri() != '']/*[local-name()='groupId']"));
var artifactId = Argument("artifactid", OrFromPom("//*[local-name()='project' and namespace-uri() != '']/*[local-name()='artifactId']"));
var version = Argument("version", OrFromPom("//*[local-name()='project' and namespace-uri() != '']/*[local-name()='version']"));
var publishDir = "/publish";
var testResultsDir = "TestResults";
var coverageFilePath = $"./{testResultsDir}/OpenCover.xml";

private static string OrFromPom(string expression)
{
  if(!System.IO.File.Exists(@".\pom.xml"))
    return "";
  var nav = new System.Xml.XPath.XPathDocument(@".\pom.xml").CreateNavigator();
  var value = nav.SelectSingleNode(expression);
  return value == null ? "null" : value.ToString();
}

Task("info")
  .Does(() =>
  {
    Information("target: " + target);
    Information("configuration: " + configuration);
    Information("solution: " + solution);
    Information("namespacePattern: " + namespacePattern);
    Information("testAssemblies: " + testAssemblies);  
    Information("sonarUrl: " + sonarUrl);
    Information("sonarExclude: " + sonarExclude);
    Information("groupId: " + groupId);
    Information("artifactId: " + artifactId);
    Information("version: " + version);
  });

Task("clean-outputs")
  .IsDependentOn("info")
  .Does(() =>
  {
    foreach (var item in new [] { "." + publishDir, $"./{testResultsDir}"})
    {
        if(DirectoryExists(item))
        {
            DeleteDirectory(item, new DeleteDirectorySettings {
                Recursive = true,
                Force = true
            });
        }
    }
  });

Task("clean")
  .IsDependentOn("clean-outputs")
  .Does(() =>
  {
    MSBuild(solution, settings => settings
            .SetConfiguration(configuration)
            .UseToolVersion(MSBuildToolVersion.VS2019)
            .WithTarget("Clean")
            .SetVerbosity(Verbosity.Minimal));
  });
 
Task("restore")
  .IsDependentOn("clean")
  .Does(() =>
  {
    NuGetRestore(solution);
  });

Task("sonarqube-start")    
    .IsDependentOn("restore")
    .Does(() =>
    {
      if(BuildSystem.IsRunningOnGitLabCI)
      {
        if(groupId == "" || artifactId == "" || version == "")
          throw new Exception("groupId,artifactID & version must be set for sonarqube, these can be passed as arguments (groupid,artifactid,version) or derived from a pom.xml file");

        var exclusion = "";
        if(sonarExclude != "")
        {
          exclusion = "/d:sonar.exclusions=\"" + sonarExclude + "\"";
        }

        
        var id = groupId + "." + artifactId;
        SonarBegin(new SonarBeginSettings
        {
            Name = id,
            Key = id,
            Url = sonarUrl,
            Version = version,
            ArgumentCustomization = args => args
                .Append($"/d:sonar.cs.opencover.reportsPaths=\"**\\{testResultsDir}\\OpenCover.xml\"")
                .Append($"/d:sonar.cs.nunit.reportsPaths=\"**\\{testResultsDir}\\NUnitResults.xml\"")
                .Append(exclusion)
                
        });
      }
    });

Task("build")
  .IsDependentOn("sonarqube-start")
  .Does(() =>
  { 
        MSBuild(solution, settings => settings.SetConfiguration(configuration)
            .UseToolVersion(MSBuildToolVersion.VS2019)
        );
  }); 

Task("test")
  .IsDependentOn("build")
  .Does((context) =>
  {
    CreateDirectory($"./{testResultsDir}");
            
    var toolPath = context.Tools.Resolve("nunit3-console.exe");

    Information("Found NUnit.ConsoleRunner here: " + toolPath);

    var testSettings = new NUnit3Settings ()
    {
        Configuration = "Debug",
        DisposeRunners = true,
        NoResults = false,
        SkipNonTestAssemblies = true,
        Agents = 1,
        OutputFile = new FilePath($"./{testResultsDir}/Output.txt"),
        Process = NUnit3ProcessOption.InProcess,
        ToolTimeout = new TimeSpan(1, 0, 0),
        TraceLevel = NUnitInternalTraceLevel.Verbose,
        ToolPath = toolPath,
        Workers = 1,
        Results = new List<NUnit3Result>
        {
            new NUnit3Result{ FileName = $"./{testResultsDir}/NUnitResults.xml" }
        }
    };
    
    OpenCover(tool => { tool.NUnit3(testAssemblies, testSettings);},
        new FilePath(coverageFilePath),
        new OpenCoverSettings 
        { 
          Register = BuildSystem.IsRunningOnGitLabCI ? "normal" : "user", 
          ReturnTargetCodeOffset = 0,
          ArgumentCustomization = args => args.Append("-coverbytest:*")
        }
            .WithFilter("+[" + namespacePattern + "]*")
            .WithFilter("-[" + testPattern + "]*"));


    ReportGenerator(
          coverageFilePath, 
          testResultsDir,
          new ReportGeneratorSettings 
          {
              ReportTypes = new List<ReportGeneratorReportType>
              {
                BuildSystem.IsRunningOnGitLabCI ? ReportGeneratorReportType.Cobertura : ReportGeneratorReportType.Html
              }
          });

  });

Task("sonarqube-end")
    .IsDependentOn("test")
    .Does(() =>
    {
      if(BuildSystem.IsRunningOnGitLabCI)
      {
        SonarEnd(new SonarEndSettings());
      }
    });

Task("package")
    .IsDependentOn("sonarqube-end")
    .Does(() =>
    { 
                  MSBuild("./HM.Operations.Secure.Web/HM.Operations.Secure.Web.csproj", settings => 
                      settings.SetConfiguration(configuration)
            .UseToolVersion(MSBuildToolVersion.VS2019) 
            .WithTarget("WebPublish")
            .WithProperty("WebPublishMethod", "FileSystem")
            .WithProperty("PublishUrl", "../artifacts")
            .WithProperty("DebugType", "pdbonly")
            .WithProperty("DebugSymbols", "true")
        );
          CreateDirectory("./publish"); Zip("./artifacts", "./publish/publish.zip"); 
    }); 


RunTarget(target);
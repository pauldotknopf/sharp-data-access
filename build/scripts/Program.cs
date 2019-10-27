using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Build.Buildary;
using Newtonsoft.Json;
using static Bullseye.Targets;
using static Build.Buildary.Directory;
using static Build.Buildary.Path;
using static Build.Buildary.Shell;
using static Build.Buildary.Runner;
using static Build.Buildary.Runtime;
using static Build.Buildary.Log;
using static Build.Buildary.File;
using static Build.Buildary.GitVersion;

namespace Build
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = ParseOptions<RunnerOptions>(args);
            
            var gitVersion = GetGitVersion(ExpandPath("./"));
            var commandBuildArgs = $"--configuration {options.Config} ";
            var commandBuildArgsWithVersion = commandBuildArgs;
            if (!string.IsNullOrEmpty(gitVersion.PreReleaseTag))
            {
                commandBuildArgsWithVersion += $" --version-suffix \"{gitVersion.PreReleaseTag}\"";
            }
            
            Info($"Version: {gitVersion.FullVersion}");
            
            Target("clean", () =>
            {
                CleanDirectory(ExpandPath("./output"));
            });
            
            Target("update-version", () =>
            {
                if (FileExists("./build/version.props"))
                {
                    DeleteFile("./build/version.props");
                }
                
                WriteFile("./build/version.props",
                    $@"<Project>
    <PropertyGroup>
        <VersionPrefix>{gitVersion.Version}</VersionPrefix>
    </PropertyGroup>
</Project>");
            });
            
            Target("build", () =>
            {
                RunShell($"dotnet clean {commandBuildArgs} ./SharpDataAccess.sln");
            });
            
            Target("publish", () =>
            {
                RunShell($"dotnet pack {commandBuildArgsWithVersion} --output {ExpandPath("./output")} {ExpandPath("./SharpDataAccess.sln")}");
            });
            
            Target("default", DependsOn("clean", "update-version", "build", "publish"));

            Execute(options);
        }
    }
}

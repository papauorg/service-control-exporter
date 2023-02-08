#tool "dotnet:?package=GitVersion.Tool&version=5.12.0"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

const string SOLUTION_FILE = "./ServiceControlExporter.sln";

GitVersion versionInfo;

Task("Version")
    .Description("Retrieves the current version from the git repository")
    .Does(() => {
        versionInfo = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = false
        });

        Information($"Version: {versionInfo.SemVer}");

        var envFile = EnvironmentVariable("GITHUB_OUTPUT");
        Information($"GITHUB_OUTPUT file: {envFile}");
        if (!string.IsNullOrEmpty(envFile))
        {
            Information("Writing version to output.");
            System.IO.File.AppendAllText(envFile, $"version={versionInfo.SemVer}\r\n");
        }

        //Information($"::set-output name=version::{versionInfo.SemVer}");
    });

Task("Clean")
    .Description("Removes the artifact directory")
    .Does(() => {
        EnsureDirectoryDoesNotExist("./artifacts");
    });

Task("Package")
    .IsDependentOn("Version")
    .IsDependentOn("Clean")
    .Description("Creates packages for the exporter.")
    .Does(() => {
        var msBuildSettings = new DotNetMSBuildSettings()
        {
            Version = versionInfo.AssemblySemVer,
            InformationalVersion = versionInfo.InformationalVersion
        };

        var nugetPackageBuildSettings = new DotNetPublishSettings {
            Configuration = configuration,
            SelfContained = true,
            Runtime = "win10-x64",
            OutputDirectory = "./artifacts/win/",
            MSBuildSettings = msBuildSettings
        };
        DotNetPublish(SOLUTION_FILE, nugetPackageBuildSettings);

        Zip("./artifacts/win/", $"./artifacts/service-control-exporter.{versionInfo.SemVer}.zip");
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);
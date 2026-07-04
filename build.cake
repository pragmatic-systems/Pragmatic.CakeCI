///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////

// NOTE: We are dog-fooding our own CI tools here and using the local built package to run the CI pipe.
#addin nuget:?package=Pragsys.CakeCI&version=0.1.0-local

#addin nuget:?package=Cake.Docker&version=1.3.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var cakeMixFile = CiArgument("cakemix", "build.cakemix");
var target = CiArgument("target", "Default");
var configuration = CiArgument("configuration", "Release");
var versionNumber = CiArgument("VersionOverride");

BuildManifest buildManifest;

var nugetArgs = new NugetArgs
{
    Source = CiArgument("Source"),
    ApiKey = CiArgument("ApiKey"),
};
var containerArgs = new ContainerArgs
{
    Registry = CiArgument("ContainerRegistry"),
    Token = CiArgument("ContainerRegistryToken"),
    UserName = CiArgument("ContainerRegistryUserName")
};
var sonarArgs = new SonarArgs
{
    Org = CiArgument("SonarOrg"),
    Token = CiArgument("SonarToken"),
    ProjectKey = CiArgument("SonarProjectKey"),
    ProjectName = CiArgument("SonarProjectName"),
    HostUrl = CiArgument("SonarHostUrl", "http://localhost:9000")
};

// Artifact Folders
var artifactsFolder = "./artifacts";
var packagesFolder = System.IO.Path.Combine(artifactsFolder, "packages");
var swaggerFolder = System.IO.Path.Combine(artifactsFolder, "swagger");
var postmanFolder = System.IO.Path.Combine(artifactsFolder, "postman");

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Sonar Scanner Helper
///////////////////////////////////////////////////////////////////////////////
string GetSonarScannerPath()
{
	var toolsDir = MakeAbsolute(Directory("tools"));
	var sonarToolDir = System.IO.Path.Combine(toolsDir.FullPath, "dotnet-sonarscanner", "7.1.1", "tools");

	// Assumes the .cake script resides at the repository root.
    var scriptDirectory = Context.Environment.WorkingDirectory;
	
	Information("Tools Dir: " + toolsDir);
	Information("Sonar Tool Dir: " + sonarToolDir);
	Information("Script Dir: " + scriptDirectory);

	if (!System.IO.Directory.Exists(sonarToolDir))
	{
		Information("Installing dotnet-sonarscanner tool...");
		var installSettings = new ProcessSettings
		{
			Arguments = new ProcessArgumentBuilder()
				.Append("tool")
				.Append("install")
				.Append("dotnet-sonarscanner")
				.Append("--version")
				.Append("7.1.1")
				.Append("--tool-path")
				.Append(toolsDir.FullPath)
				.Append("--add-source")
				.Append("https://api.nuget.org/v3/index.json")
				.Append("-v")
				.Append("quiet")
			};
		StartProcess("dotnet", installSettings);
	}

	var scannerExe = System.IO.Path.Combine(sonarToolDir, "dotnet-sonarscanner.exe");
	if (!System.IO.File.Exists(scannerExe))
	{
		scannerExe = System.IO.Path.Combine(sonarToolDir, "dotnet-sonarscanner");
	}
	return scannerExe;
}

Task("__InstallSonarScanner")
	.Does(() =>
	{
		// Triggers installation of dotnet-sonarscanner if not already present
		_ = GetSonarScannerPath();
	});

// Setup / Teardown
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
	buildManifest = LoadBuildManifest(cakeMixFile);

	// Clean artifacts
	if (System.IO.Directory.Exists(artifactsFolder))
		System.IO.Directory.Delete(artifactsFolder, true);
});

Teardown(context =>
{

});

///////////////////////////////////////////////////////////////////////////////
// Tasks
///////////////////////////////////////////////////////////////////////////////
Task("__NugetArgsCheck")
	.Does(() => {
		nugetArgs.Validate();
	});

Task("__ContainerArgsCheck")
	.Does(() => {
		containerArgs.Validate();
	});

Task("__SonarArgsCheck")
	.Does(() => {
		sonarArgs.Validate();
	});

Task("__Test")
	.Does(() => {
		CiTest();
	});

Task("__Benchmark")
	.Does(() => {
		CiBenchmark();
	});

Task("__LintCheck")
    .Does(() =>
    {
		CiLint();
    });

Task("__BeginSonarScan")
		.IsDependentOn("__InstallSonarScanner")
		.Does(() =>
		{
			var reportFiles = System.IO.Directory.GetFiles(artifactsFolder, "*.xml", SearchOption.AllDirectories)
					.Select(p => p.Replace("\\", "/")).ToArray();
			var reportPaths = reportFiles.Length > 0 ? string.Join(",", reportFiles) : string.Empty;

			var scannerPath = GetSonarScannerPath();
			Information($"Using Sonar scanner: {scannerPath}");

			var beginSettings = new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.Append("begin")
					.AppendSwitchQuoted("/k", sonarArgs.ProjectKey)
					.AppendSwitchQuoted("/n", sonarArgs.ProjectName)
					.AppendSwitchQuoted("/o", sonarArgs.Org)
					.Append("/d:sonar.login=" + sonarArgs.Token)
					.Append("/d:sonar.host.url=" + sonarArgs.HostUrl)
					.Append("/d:sonar.cs.opencover.reportsPaths=" + reportPaths)
			};

			StartProcess(scannerPath, beginSettings);

			DotNetBuild("*.sln");
		});

Task("__EndSonarScan")
		.IsDependentOn("__InstallSonarScanner")
		.Does(() =>
		{
			var scannerPath = GetSonarScannerPath();

			var endSettings = new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.Append("end")
					.Append("/d:sonar.login=" + sonarArgs.Token)
			};

			StartProcess(scannerPath, endSettings);
			Information("Sonar analysis completed successfully.");
		});

Task("__VersionInfo")
	.Does(() => {
		versionNumber = CiVersion(versionNumber);
	});

Task("__NugetPack")
	.Does(() => {

		foreach(var package in buildManifest.NugetPackages)
		{
			Information($"Packing {package}...");
			var settings = new DotNetMSBuildSettings
			{
				PackageVersion = versionNumber
			};

			var packSettings = new DotNetPackSettings
			{
				Configuration = "Release",
				OutputDirectory = packagesFolder,
				MSBuildSettings = settings
			};
			DotNetPack(package, packSettings);
		}
	});

Task("__NugetPush")
	.Does(() => {

		if (!System.IO.Directory.Exists(packagesFolder))
		{
			Information("No packages to push in the packages folder");
			return;
		}

		var packedArtifacts = System.IO.Directory.EnumerateFiles(packagesFolder);
		foreach(var package in packedArtifacts)
		{
			Information($"Pushing {package}...");
			var pushSettings = new DotNetNuGetPushSettings
			{
				Source = nugetArgs.Source,
				ApiKey = nugetArgs.ApiKey
			};

			DotNetNuGetPush(package, pushSettings);
		}
	});

Task("__DockerLogin")
	.Does(() => {

		Information($"Logging into registry: {containerArgs.Registry}...");

		var loginSettings = new DockerRegistryLoginSettings
		{ 
			Password = containerArgs.Token, 
			Username = containerArgs.UserName
		};

		DockerLogin(loginSettings, containerArgs.Registry);
	});

Task("__DockerPack")
	.IsDependentOn("__VersionInfo")
	.Does(() => {

		foreach(var package in buildManifest.DockerPackages)
		{
			Information($"Packing Docker: {package}...");
			var directoryName = System.IO.Path.GetDirectoryName(package);
			Information($"Directory Name: {directoryName}");
			var parts = directoryName.Split(System.IO.Path.DirectorySeparatorChar);
			Information($"Parts: {parts.Length}");
			Information($"Last Part: {parts.Last()}");
			var packageName = parts.Last().ToLower();
			packageName = $"{containerArgs.Registry}/{packageName}".ToLower();

			Information($"Packing: {packageName}...");
			var settings = new DockerImageBuildSettings
				{
					Tag = new[] { $"{packageName}:{versionNumber}" },
					File = package
				};

			DockerBuild(settings, ".");
		}
	});

Task("__DockerPush")
	.Does(() => {

		foreach(var package in buildManifest.DockerPackages)
		{
			Information($"Pushing Docker: {package}...");
			var directoryName = System.IO.Path.GetDirectoryName(package);
			Information($"Directory Name: {directoryName}");
			var parts = directoryName.Split(System.IO.Path.DirectorySeparatorChar);
			Information($"Parts: {parts.Length}");
			Information($"Last Part: {parts.Last()}");
			var packageName = parts.Last().ToLower();
			packageName = $"{containerArgs.Registry}/{packageName}".ToLower();

			var settings = new DockerImagePushSettings
			{
				AllTags = true
			};

			Information($"Pushing: {packageName}...");

			DockerPush(settings, $"{packageName}");
		}
	});

Task("BuildAndTest")
	.IsDependentOn("__Test");

Task("BuildAndBenchmark")
	.IsDependentOn("__Benchmark");

Task("BuildAndSonarScan")
	.IsDependentOn("__SonarArgsCheck")
	.IsDependentOn("__Test")
	.IsDependentOn("__Benchmark")
	.IsDependentOn("__BeginSonarScan")
	.IsDependentOn("__EndSonarScan");

Task("NugetPackAndPush")
	.IsDependentOn("__NugetArgsCheck")
	.IsDependentOn("__VersionInfo")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__Test")
	.IsDependentOn("__Benchmark")
	.IsDependentOn("__NugetPack")
	.IsDependentOn("__NugetPush");

Task("DockerPackAndPush")
	.IsDependentOn("__ContainerArgsCheck")
	.IsDependentOn("__VersionInfo")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__Test")
	.IsDependentOn("__Benchmark")
	.IsDependentOn("__DockerLogin")
	.IsDependentOn("__DockerPack")
	.IsDependentOn("__DockerPush");

Task("FullPackAndPush")
	.IsDependentOn("__NugetArgsCheck")
	.IsDependentOn("__ContainerArgsCheck")
	.IsDependentOn("__VersionInfo")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__Test")
	.IsDependentOn("__Benchmark")
	.IsDependentOn("__NugetPack")
	.IsDependentOn("__DockerLogin")
	.IsDependentOn("__DockerPack")
	.IsDependentOn("__NugetPush")
	.IsDependentOn("__DockerPush");

Task("Default")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__Test")
	.IsDependentOn("__Benchmark");

RunTarget(target);

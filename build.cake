﻿///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////

// NOTE: We are dog-fooding our own CI tools here and using the local built package to run the CI pipe.
#addin nuget:?package=Pragsys.CakeCI&version=0.16.0-local

#addin nuget:?package=Cake.Json&version=7.0.1
#addin nuget:?package=Cake.Docker&version=1.3.0
#addin nuget:?package=Cake.Sonar&version=5.0.0

///////////////////////////////////////////////////////////////////////////////
// TOOLS
///////////////////////////////////////////////////////////////////////////////
#tool dotnet:?package=GitVersion.Tool&version=5.12.0
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.8.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var cakeMixFile = Argument("cakemix", "build.cakemix");
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var versionNumber = CiArgument("VersionOverride");	

// Nuget Params
var nugetPackageSource = CiArgument("Source");
var nugetApiKey = CiArgument("ApiKey");			

// Container Params
var containerRegistry = CiArgument("ContainerRegistry");
var containerRegistryToken = CiArgument("ContainerRegistryToken");
var containerRegistryUserName = CiArgument("ContainerRegistryUserName");

// Sonar Params
var sonarOrg = CiArgument("SonarOrg");
var sonarToken = CiArgument("SonarToken");
var sonarProjectKey = CiArgument("SonarProjectKey");
var sonarProjectName = CiArgument("SonarProjectName");
var sonarHostUrl = CiArgument("SonarHostUrl", "http://localhost:9000");

// Artifact Folders
var artifactsFolder = "./artifacts";
var packagesFolder = System.IO.Path.Combine(artifactsFolder, "packages");
var swaggerFolder = System.IO.Path.Combine(artifactsFolder, "swagger");
var postmanFolder = System.IO.Path.Combine(artifactsFolder, "postman");

BuildManifest buildManifest;

///////////////////////////////////////////////////////////////////////////////
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
		if (string.IsNullOrEmpty(nugetPackageSource))
			throw new ArgumentException("NugetPackageSource is required");

		if (string.IsNullOrEmpty(nugetApiKey))
			throw new ArgumentException("NugetApiKey is required");
	});

Task("__ContainerArgsCheck")
	.Does(() => {
		if (string.IsNullOrEmpty(containerRegistryToken))
			throw new ArgumentException("ContainerRegistryToken is required");
			
		if (string.IsNullOrEmpty(containerRegistryUserName))
			throw new ArgumentException("ContainerRegistryUserName is required");
			
		if (string.IsNullOrEmpty(containerRegistry))
			throw new ArgumentException("ContainerRegistry is required");
	});

Task("__SonarArgsCheck")
	.Does(() => {
		if (string.IsNullOrEmpty(sonarOrg))
			throw new ArgumentException("SonarOrg is required");
		
		if (string.IsNullOrEmpty(sonarToken))
			throw new ArgumentException("SonarToken is required");

		if (string.IsNullOrEmpty(sonarProjectKey))
			throw new ArgumentException("SonarProjectKey is required");
			
		if (string.IsNullOrEmpty(sonarProjectName))
			throw new ArgumentException("SonarProjectName is required");
	});

Task("__Test")
	.Does(() => {
		CiTest();
	});

Task("__Benchmark")
	.Does(() => {

		foreach(var benchmark in buildManifest.Benchmarks)
		{
			Information($"Benchmarking {benchmark}...");
			var benchName = System.IO.Path.GetFileNameWithoutExtension(benchmark);

			var settings = new DotNetRunSettings
			{
				Configuration = "Release", 
				ArgumentCustomization = args => {
					return args
						.Append("--artifacts")
						.AppendQuoted(System.IO.Path.Combine(artifactsFolder, benchName));
				}
			};

			DotNetRun(benchmark, settings);
		}
	});

Task("__LintCheck")
    .Does(() =>
    {
		CiLint();
    });

Task("__BeginSonarScan")
		.Does(() =>
		{
			var reportPaths = System.IO.Directory.GetFiles(artifactsFolder, "*.xml", SearchOption.AllDirectories)
					.Select(p => p.Replace('\\', '/'))
					.Aggregate((a, b) => a + "," + b);

			SonarBegin(new SonarBeginSettings
			{
				Key = sonarProjectKey,
				Name = sonarProjectName,
				Login = sonarToken,
				Organization = sonarOrg,
				Url = sonarHostUrl,
				VsCoverageReportsPath = reportPaths,
			});

			DotNetBuild("*.sln");
		});

Task("__EndSonarScan")
		.Does(() =>
		{
			SonarEnd(new SonarEndSettings
			{
				Login = sonarToken,
			});
			Information("Sonar analysis completed successfully.");
		});

Task("__VersionInfo")
	.Does(() => {

		if (string.IsNullOrEmpty(versionNumber))
		{
			var version = GitVersion();
			Information("GitVersion Info: " + SerializeJsonPretty(version));
			versionNumber = version.SemVer;
		}

		Information("Version Number: " + versionNumber);
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
				Source = nugetPackageSource,
				ApiKey = nugetApiKey
			};
			DotNetNuGetPush(package, pushSettings);
		}
	});

Task("__DockerLogin")
	.Does(() => {
		
		Information($"Logging into registry: {containerRegistry}...");

		var loginSettings = new DockerRegistryLoginSettings
		{ 
			Password = containerRegistryToken, 
			Username = containerRegistryUserName
		};

		DockerLogin(loginSettings, containerRegistry);  
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
			packageName = $"{containerRegistry}/{packageName}".ToLower();	
			
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
			packageName = $"{containerRegistry}/{packageName}".ToLower();

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

Task("SonarScan")
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
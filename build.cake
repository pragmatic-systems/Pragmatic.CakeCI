///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////

// NOTE: We are dog-fooding our own CI tools here and using the local built package to run the CI pipe.
#addin nuget:?package=Pragmatic.CakeCI&version=0.1.0-dogfood

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
    Source = CiArgument("NugetSource"),
    ApiKey = CiArgument("NugetApiKey"),
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
	Branch = CiArgument("SonarBranch"),
    HostUrl = CiArgument("SonarHostUrl", "http://localhost:9000")
};

// Artifact Folders
var artifactsFolder = "./artifacts";
var packagesFolder = System.IO.Path.Combine(artifactsFolder, "packages");
var swaggerFolder = System.IO.Path.Combine(artifactsFolder, "swagger");
var postmanFolder = System.IO.Path.Combine(artifactsFolder, "postman");

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
Task("__Version")
	.Does(() => versionNumber = CiVersion(versionNumber));

Task("__LintCheck")
	.Does(() => CiLint());

Task("__ValidateSonarArgs")
	.Does(() => sonarArgs.Validate());

Task("__ValidateNugetArgs")
	.Does(() => nugetArgs.Validate());

Task("__ValidateDockerArgs")
	.Does(() => containerArgs.Validate());

Task("__BeginSonarScan")
	.IsDependentOn("__ValidateSonarArgs")
	.Does(() => CiSonarScannerBegin(sonarArgs, artifactsFolder));

///////////////////////////////////////////////////////////////////////////////
// Public Tasks
///////////////////////////////////////////////////////////////////////////////
Task("BuildAndTest")
	.Does(() => CiTest());

Task("BuildAndBenchmark")
	.Does(() => CiBenchmark());

Task("BuildAndSonarScan")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__ValidateSonarArgs")
	.IsDependentOn("__BeginSonarScan")
	.Does(() =>
	{
		CiTest();
		CiBenchmark();
		CiSonarScannerEnd(sonarArgs);
	});

Task("NugetPackAndPush")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__ValidateNugetArgs")
	.IsDependentOn("__Version")
	.Does(() =>
	{
		CiTest();
		CiBenchmark();
		CiNugetPack(buildManifest, packagesFolder, versionNumber);
		CiNugetPush(nugetArgs, packagesFolder);
	});

Task("DockerPackAndPush")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__ValidateDockerArgs")
	.IsDependentOn("__Version")
	.Does(() =>
	{
		CiTest();
		CiBenchmark();
		CiDockerLogin(containerArgs);
		CiDockerBuild(buildManifest, containerArgs, versionNumber);
		CiDockerPush(buildManifest, containerArgs, versionNumber);
	});

Task("FullPackAndPush")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__ValidateNugetArgs")
	.IsDependentOn("__ValidateDockerArgs")
	.IsDependentOn("__Version")
	.IsDependentOn("__BeginSonarScan")
	.Does(() =>
	{
		CiTest();
		CiBenchmark();
		CiNugetPack(buildManifest, packagesFolder, versionNumber);
		CiDockerLogin(containerArgs);
		CiDockerBuild(buildManifest, containerArgs, versionNumber);
		CiSonarScannerEnd(sonarArgs);
		CiNugetPush(nugetArgs, packagesFolder);
		CiDockerPush(buildManifest, containerArgs, versionNumber);
	});

Task("Default")
	.Does(() =>
	{
		CiLint();
		CiTest();
		CiBenchmark();
	});

RunTarget(target);

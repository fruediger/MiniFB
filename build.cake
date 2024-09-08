#addin nuget:?package=Cake.DoInDirectory&version=6.0.0
#addin nuget:?package=Cake.Prompt&version=1.2.1

using System.Collections.Immutable;
using System.Linq;

///////////////
// Arguments //
///////////////

// Argument: target

var target = Argument("target", "Build");

// Argument: configuration

var configuration = Argument("configuration", "Release");

// Argument: silent

var silent = Argument("silent", false);

///////////////////////////
// Known sample projects //
///////////////////////////

ImmutableSortedSet<string> samples = [ "Noise", "Timer", "InputEvents", "MultipleWindows", "HiDpi", "Fullscreen", "FpsInTitle" ];

///////////
// Tasks //
///////////

// Task: Update-Native-Libraries

var updateNativeLibraries = Task("Update-Native-Libraries")
	.Description("Updates the bundled native libraries of \"MiniFB\" by downloading their latest release")
	.Does(() =>
	{
		CakeExecuteScript("update-native-libraries.cake", new CakeSettings {});
	});

// Task: Clean

var clean = Task("Clean")
	.Description("Cleans the build artifacts from all of the projects")
	.Does(() =>
	{
		List<string> projectPaths = [ "MiniFB.SourceGeneration", "MiniFB" ];

		Information("""
			
			Cleaning "MiniFB.SourceGeneration" using "dotnet"...

			""");

		DotNetClean("MiniFB.SourceGeneration");

		Information("""
			
			Cleaning "MiniFB" using "dotnet"...

			""");

		DotNetClean("MiniFB");

		foreach (var sample in samples)
		{
			var samplePath = $"Samples/{sample}";

			projectPaths.Add(samplePath);

			if (DirectoryExists(samplePath))
			{
				Information($$"""
			
					Cleaning "{{samplePath}}" using "dotnet"...

					""");

				DotNetClean(samplePath);
			}
		}

		Information($"Cleaning build artifacts (\"bin/\" and \"obj/\") from [ {string.Join(", ", projectPaths.Select(projectPath => $"\"{projectPath}\""))} ]...");

		CleanDirectories(projectPaths.SelectMany(projectPath => new[] { $"{projectPath}/bin", $"{projectPath}/obj" }), new CleanDirectorySettings
		{
			Force = true
		});
	});

// Task: Build

var build = Task("Build")
	.Description("Builds the \"MiniFB\" library")
	.WithCriteria(() =>
	{
		if (!DirectoryExists("MiniFB/runtimes") && !silent)
		{
			Information("""

				A "runtimes" directory in the project "MiniFB" does not seem to exists.
				This directory contains the native libraries to bundle with "MiniFB".
					
				You might want to run the target "Update-Native-Libraries" first, to download
				the latest released version of those native libraries.
				Alternatively, you can just execute the cake script "update-native-libraries.cake".
				On some systems, you can just run the script from a commandline:

					update-native-libraries

				(adjust to appropriate invocation syntax for your cli,
				like prefixing with, for example, a "./", or suffixing
				with the corresponding file extension, for example ".sh")

				""");

			while (true)
			{
				switch (Prompt("Do you want to [C]ancel, [P]roceed any way, or [R]un the \"Update-Native-Libraries\" now?", "R")
					.Trim().ToLowerInvariant())
				{
					case "c" or "cancel": return false;

					case "p" or "proceed": return true;

					case "r" or "run":
						RunTarget(updateNativeLibraries.Task.Name);
						return true;
				}
			}
		}

		return true;
	},
		"Build cancelled due to user decision"
	)
	.Does(() =>
	{
		DoInDirectory("MiniFB", () =>
		{			
			Information($$"""
			
				Building "MiniFB" using "dotnet" (Configuration: {{configuration}})...

				""");

			DotNetBuild(".", new DotNetBuildSettings
			{
				Configuration = configuration
			});
		});
	});

// Task: Clean-Build

var cleanBuild = Task("Clean-Build")
	.Description("Cleans and then rebuilds the \"MiniFB\" library")
	.IsDependentOn(clean)
	.IsDependentOn(build);

// Task: Publish

var publish = Task("Publish")
	.Description("Publishes the \"MiniFB\" library")
	.IsDependentOn(build)
	.Does(() =>
	{
		DoInDirectory("MiniFB", () =>
		{
			Information($$"""
			
				Publishing "MiniFB" using "dotnet" (Configuration: {{configuration}})...

				""");

			DotNetPublish(".", new DotNetPublishSettings
			{
				Configuration = configuration,
				NoBuild = true
			});
		});
	});

// Task: Clean-Publish

var cleanPublish = Task("Clean-Publish")
	.Description("Cleans and then republishes the \"MiniFB\" library")
	.IsDependentOn(clean)
	.IsDependentOn(publish);

// Task: Pack

var pack = Task("Pack")
	.Description("Packs the \"MiniFB\" library")
	.IsDependentOn(publish)
	.Does(() =>
	{
		DoInDirectory("MiniFB", () =>
		{
			Information($$"""
			
				Packing "MiniFB" using "dotnet" (Configuration: {{configuration}})...

				""");

			DotNetPack(".", new DotNetPackSettings
			{
				Configuration = configuration,
				NoBuild = true
			});
		});
	});

// Task: Clean-Pack

var cleanPack = Task("Clean-Pack")
	.Description("Cleans and then repacks the \"MiniFB\" library")
	.IsDependentOn(clean)
	.IsDependentOn(pack);

// Task: Build-Samples

var buildSamples = Task("Build-Samples")
	.Description("Builds all of the samples at once");

var cleanBuildSamples = Task("Clean-Build-Samples")
	.Description("Cleans and then rebuilds all of the samples at once")
	.IsDependentOn(clean)
	.IsDependentOn(buildSamples);

foreach (var sample in samples)
{
	if (DirectoryExists($"Samples/{sample}"))
	{

// Tasks: Build-Sample-*

		var buildSample = Task($"Build-Sample-{sample}")
			.Description($"Builds the \"{sample}\" sample application")
			.IsDependentOn(build)
			.Does(() =>
			{
				DoInDirectory($"Samples/{sample}", () =>
				{
					Information($$"""
			
						Building "Samples/{{sample}}" using "dotnet" (Configuration: {{configuration}})...

						""");

					DotNetBuild(".", new DotNetBuildSettings
					{
						Configuration = configuration
					});
				});
			});

		buildSamples.IsDependentOn(buildSample);

// Tasks: Clean-Build-Sample-*

		var cleanBuildSample = Task($"Clean-Build-Sample-{sample}")
			.Description($"Cleans and then rebuilds the \"{sample}\" sample application")
			.IsDependentOn(clean)
			.IsDependentOn(buildSample);

// Tasks: Run-Sample-*

		var runSample = Task($"Run-Sample-{sample}")
			.Description($"Builds and runs the \"{sample}\" sample application")
			.IsDependentOn(buildSample)
			.Does(() => 
			{
				DoInDirectory($"Samples/{sample}", () =>
				{
					DotNetRun(".", new DotNetRunSettings
					{
						Configuration = configuration,
						NoBuild = true
					});
				});
			});

// Tasks: Clean-Run-Sample-*

		var cleanRunSample = Task($"Clean-Run-Sample-{sample}")
			.Description("Cleans and then rebuilds and runs the \"{sample}\" sample application")
			.IsDependentOn(clean)
			.IsDependentOn(buildSample);
	}
}

////////////
// Runner //
////////////

RunTarget(target);
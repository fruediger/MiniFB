#tool nuget:?package=7-Zip.CommandLine&version=18.1.0
#addin nuget:?package=Cake.7zip&version=4.1.0
#addin nuget:?package=Cake.DoInDirectory&version=6.0.0

DoInDirectory("MiniFB", () =>
{
	if (DirectoryExists("runtimes"))
	{
		Information("Archiving existing \"runtimes\" directory...");

		EnsureDirectoryExists(".old");

		var archiveFile = File($".old/runtimes-{System.DateTime.Now:yyyy-MM-dd-HH-mm-ss}.7z");

		SevenZip(action => action
			.InAddMode()
			.WithDirectories("runtimes")
			.WithArchive(archiveFile)
		);

		Information("Archived existing \"runtimes\" directory into \"{archiveFile}\".");
	}

	EnsureDirectoryDoesNotExist("runtimes", new EnsureDirectoryDoesNotExistSettings
	{
		Force = true,
		Recursive = true
	});

	var latestArchiveUrl = "https://github.com/fruediger/minifb-native/releases/latest/download/runtimes.7z";

	Information($"Downloading latest archived \"runtimes\" from \"{latestArchiveUrl}\"...");

	var latestArchive = DownloadFile(latestArchiveUrl);

	Information($"Extracting lasted archived \"runtimes\" ({latestArchive})...");

	SevenZip(action => action
		.InExtractMode()
		.WithArchive(latestArchive)
		.WithOutputDirectory(".")
	);

	if (DirectoryExists("runtimes"))
	{
		Information("The latest native libraries are now install in \"runtimes\".");
	}
});
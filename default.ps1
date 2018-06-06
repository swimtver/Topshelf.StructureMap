Framework "4.6"
properties {
	$BaseDir = Resolve-Path ".\src"
	$SolutionFile = "$BaseDir\Topshelf.StructureMap.sln"
	$OutputDir1 = "$BaseDir\Topshelf.StructureMap\bin"	
	$OutputDir2 = "$BaseDir\Topshelf.Quartz.Structuremap\bin"
	$Configuration = "Release"
}

task default -depends Build

task Init {
    cls	
}

task Clean -depends Init {
    if (Test-Path $OutputDir1) {
        ri $OutputDir1 -Recurse
    }
	if (Test-Path $OutputDir2) {
        ri $OutputDir2 -Recurse
    }
}

task RestorePackages {
	exec { dotnet restore $solutionFile }
}

task Build -depends Init,Clean,RestorePackages {
    exec { dotnet build $SolutionFile --configuration $Configuration --no-restore --no-incremental }
}

task Publish -depends Build {
    exec {
        dotnet pack "$BaseDir\Topshelf.StructureMap\Topshelf.StructureMap.csproj" -o $OutputDir1 --no-build --include-symbols -c $Configuration
    }
	 exec {
        dotnet pack "$BaseDir\Topshelf.Quartz.StructureMap\Topshelf.Quartz.StructureMap.csproj" -o $OutputDir2 --no-build --include-symbols -c $Configuration
    }     
}
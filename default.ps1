properties {
	$BaseDir = Resolve-Path ".\"
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
	nuget restore $solutionName
}

task Build -depends Init,Clean,RestorePackages {
    exec { msbuild $SolutionFile /t:Rebuild /p:Configuration=$Configuration /m }
}

task Publish -depends Build {
    exec {
        nuget pack Topshelf.StructureMap\Topshelf.StructureMap.csproj -OutputDirectory $OutputDir1 -Symbols -Prop Configuration=$Configuration        
    }
	 exec {
        nuget pack Topshelf.Quartz.Structuremap\Topshelf.Quartz.StructureMap.csproj -OutputDirectory $OutputDir2 -Symbols -Prop Configuration=$Configuration        
    }     
}
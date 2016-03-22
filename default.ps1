Framework "4.5.2"

properties {
    $baseDir  = resolve-path .
    $buildDir = "$baseDir\build"
    $artifactsDir = "$baseDir\artifacts"
    $toolsDir = "$baseDir\tools"
    $slnFiles = "$baseDir\src\Horton.sln"
}

include $PSScriptRoot\tools\psake\buildutils.ps1

task default -depends Build

task Clean {
    if (Test-Path $buildDir) {
        Delete-Directory $buildDir
    }
    if (Test-Path $artifactsDir) {
        Delete-Directory $artifactsDir
    }
    foreach ($slnFile in $slnFiles) {
        exec { msbuild $slnFile /v:m /nologo /p:Configuration=Debug /m /target:Clean }
        exec { msbuild $slnFile /v:m /nologo /p:Configuration=Release /m /target:Clean }
    }
}

task Init -depends Clean {
    echo "Creating build directory at the follwing path $buildDir"
    Create-Directory($buildDir);
    echo "Creating artifacts directory at the follwing path $artifactsDir"
    Create-Directory($artifactsDir);

    $currentDirectory = Resolve-Path .

    echo "Current Directory: $currentDirectory"
}

task Compile -depends Init {
    foreach ($slnFile in $slnFiles) {
        exec { msbuild $slnFile /v:n /nologo /p:Configuration=Release /m /p:AllowedReferenceRelatedFileExtensions=none /p:OutDir="$buildDir\" }
    }
}

task Package -depends Compile {
    Copy-Item $baseDir\README.txt $artifactsDir
    Copy-Item $buildDir\Horton.exe $artifactsDir
}

task Build -depends Compile, Package {

}
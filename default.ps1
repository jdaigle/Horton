Framework "4.5.2"

properties {
    $baseDir  = resolve-path .
    $buildDir = "$baseDir\build"
    $artifactsDir = "$baseDir\artifacts"
    $toolsDir = "$baseDir\tools"
    $slnFiles = "$baseDir\src\Horton.sln"
    $global:msbuildconfig = "debug"
}

task default -depends local
task local -depends init, compile, package
task ci -depends clean, release, compile, package

task release {
    $global:msbuildconfig = "release"
}

task clean {
    remove-item "$buildDir" -recurse -force  -ErrorAction SilentlyContinue | out-null
    remove-item "$artifactsDir" -recurse -force  -ErrorAction SilentlyContinue | out-null

    foreach ($slnFile in $slnFiles) {
        exec { msbuild $slnFile /v:m /nologo /p:Configuration=Debug /m /target:Clean }
        exec { msbuild $slnFile /v:m /nologo /p:Configuration=Release /m /target:Clean }
    }
}

task init {
    echo "Creating build directory at the follwing path $buildDir"
    New-Item $buildDir -ItemType Directory -Force | Out-Null
    echo "Creating artifacts directory at the follwing path $artifactsDir"
    New-Item $artifactsDir -ItemType Directory -Force | Out-Null

    $currentDirectory = Resolve-Path .

    echo "Current Directory: $currentDirectory"
}

task compile -depends init {
    foreach ($slnFile in $slnFiles) {
        exec { msbuild $slnFile /v:n /nologo /p:Configuration=$msbuildconfig /m /p:AllowedReferenceRelatedFileExtensions=none /p:OutDir="$buildDir\" }
    }
}

task package -depends compile {
    Copy-Item $baseDir\README.txt $artifactsDir
    Copy-Item $buildDir\Horton.exe $artifactsDir
}
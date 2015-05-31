# Restore our NuGet packages
nuget sources Add -Name CitiesSkylines -Source $env:NUGET_CSL_URL -UserName $env:NUGET_CSL_USERNAME -Password $env:NUGET_CSL_PASSWORD -Verbosity quiet -NonInteractive
nuget restore .\appveyor\packages.config -SolutionDirectory .\ -NonInteractive

[xml]$packages = Get-Content .\appveyor\packages.config
$referencePath = ($packages.packages.package | % {"..\packages\$($_.id).$($_.version)\lib\$($_.targetFramework)\"}) -Join ";"

# Do the actual build
msbuild /verbosity:minimal /p:ReferencePath="$referencePath"

# Copy the files we need to .\bin
if (Test-Path bin) {
    Remove-Item bin -recurse -force
}
mkdir bin | Out-Null;
Copy-Item "CSL Scrollable Toolbar\bin\$env:CONFIGURATION\*.dll" -destination bin -recurse

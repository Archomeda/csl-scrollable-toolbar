if (Test-Path bin) {
    Remove-Item bin -recurse -force
}
mkdir bin | Out-Null;
Copy-Item "CSL Scrollable Toolbar/bin/$env:CONFIGURATION/*.dll" -destination bin -recurse

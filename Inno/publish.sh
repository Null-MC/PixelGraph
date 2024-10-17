dotnet publish ..\\PixelGraph.UI -c Release -r win-x64 -o src -p:EnableWindowsTargeting=true --self-contained false
"C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe" "setup.iss"

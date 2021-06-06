Echo Compiling shader HLSL to CSO
cd "%~dp0"
PATH="c:\Program Files (x86)\Microsoft DirectX SDK (June 2010)\Utilities\bin\x86\"
fxc /T ps_5_0 /E PSMain "psBasic.hlsl" /Fo "psBasic.cso"

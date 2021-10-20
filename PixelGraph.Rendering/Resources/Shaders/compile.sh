echo Compiling shader HLSL to CSO
cd `dirname "$0"`
PATH="/c/Program Files (x86)/Microsoft DirectX SDK (June 2010)/Utilities/bin/x86/fxc.exe"

"$PATH" /T vs_4_0 /E main "diffuse_vs.hlsl" /Fo "compiled/diffuse_vs.cso"
"$PATH" /T ps_4_0 /E main "diffuse_ps.hlsl" /Fo "compiled/diffuse_ps.cso"

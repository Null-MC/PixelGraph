using HelixToolkit.SharpDX.Core;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ShaderDescription = HelixToolkit.SharpDX.Core.Shaders.ShaderDescription;

namespace PixelGraph.Rendering.Shaders;

public interface IShaderByteCodeManager
{
    void Add(string name, ShaderSourceDescription shader);
    bool LoadAll(out ShaderCompileError[] compileErrors);
    ShaderBytecode GetCode(string name);
    ShaderDescription BuildDescription(string name, ShaderStage type);
}

public class ShaderByteCodeManager : IShaderByteCodeManager, IDisposable
{
    private readonly Dictionary<string, ShaderSourceDescription> map;


    protected ShaderByteCodeManager()
    {
        map = new Dictionary<string, ShaderSourceDescription>(StringComparer.InvariantCultureIgnoreCase);
    }

    public void Dispose()
    {
        foreach (var shader in map.Values)
            shader.Dispose();

        map.Clear();
    }

    public void Add(string name, ShaderSourceDescription shader)
    {
        map[name] = shader;
    }

    public void Add(string profile, string entryName, string fileName)
    {
        map[entryName] = new ShaderSourceDescription {
            RawFileName = $"{fileName}.hlsl",
            CompiledResourceName = $"{fileName}.cso",
            Profile = profile,
        };
    }

    public bool LoadAll(out ShaderCompileError[] compileErrors)
    {
        var shaderPath = Path.GetFullPath("shaders");

#if DEBUG
        if (Debugger.IsAttached) {
            var a = Assembly.GetExecutingAssembly();
            var p = Path.GetDirectoryName(a.Location);

            //var p = Path.GetDirectoryName(Environment.CurrentDirectory);

            while (p != null) {
                var t = Path.Combine(p, "PixelGraph.Rendering", "Resources", "Shaders");
                if (Directory.Exists(t)) {
                    shaderPath = t;
                    break;
                }

                p = Path.GetDirectoryName(p);
            }
        }
#endif

        var hasShaderFolder = Directory.Exists(shaderPath);
        var _errors = new List<ShaderCompileError>();

        foreach (var shader in map.Values) {
            if (hasShaderFolder && shader.TryLoadFromPath(shaderPath, _errors)) {
                continue;
            }

            shader.LoadFromAssembly();
        }

        compileErrors = _errors.ToArray();
        return compileErrors.Length == 0;
    }

    public ShaderBytecode GetCode(string name)
    {
        if (!map.TryGetValue(name, out var shader))
            throw new ApplicationException($"Shader '{name}' not found!");

        return shader.Code;
    }

    public ShaderDescription BuildDescription(string name, ShaderStage type)
    {
        return new(name, type, GetCode(name));
    }
}
using PixelGraph.Common.Extensions;
using PixelGraph.Rendering.Utilities;
using SharpDX;
using SharpDX.D3DCompiler;

namespace PixelGraph.Rendering.Shaders;

public class ShaderSourceDescription : IDisposable
{
    public string EntryPoint {get; set;} = "main";
    public string? Profile {get; set;}

    public string? RawFileName {get; set;}
    public string? CompiledResourceName {get; set;}
    public ShaderBytecode? Code {get; private set;}


    public void Dispose()
    {
        Code?.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool TryLoadFromPath(string path, IList<ShaderCompileError> errorList)
    {
        var filename = PathEx.Join(path, RawFileName);
        if (!File.Exists(filename)) return false;
            
        try {
            using var includeMgr = new CustomShaderFileInclude(path);
            using var result = ShaderBytecode.CompileFromFile(filename, EntryPoint, Profile, include: includeMgr);

            if (result == null || result.HasErrors) {
                errorList.Add(new ShaderCompileError {
                    Filename = filename,
                    Message = result?.Message ?? "An unknown error occurred!",
                    //ResultCode = result?.ResultCode?.Code,
                });
                return false;
            }

            Code?.Dispose();
            Code = result.Bytecode;
            return true;
        }
        catch (CompilationException error) {
            errorList.Add(new ShaderCompileError {
                Filename = filename,
                Message = error.Message,
                //ResultCode = error.ResultCode?.Code,
            });
            return false;
        }
    }

    public void LoadFromAssembly()
    {
        ArgumentNullException.ThrowIfNull(CompiledResourceName);

        var resourcePath = GetResourcePath(CompiledResourceName);
        using var stream = ResourceLoader.Open(resourcePath)
            ?? throw new FileNotFoundException($"Unable to locate embedded resource '{resourcePath}'!", resourcePath);

        Code?.Dispose();
        Code = ShaderBytecode.FromStream(stream);
    }

    private static string GetResourcePath(string resourceName)
    {
        return $"PixelGraph.Rendering.Resources.Shaders.compiled.{resourceName}";
    }
}

internal class CustomShaderFileInclude(string sourcePath) : Include
{
    public IDisposable? Shadow {get; set;}


    public void Dispose()
    {
        Shadow?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Stream Open(IncludeType type, string fileName, Stream parentStream)
    {
        var path = sourcePath;

        if (parentStream is FileStream fileStream) {
            var p = Path.GetDirectoryName(fileStream.Name);
            if (p != null) path = p;
        }

        var fullFile = Path.Combine(path, fileName).Replace('/', '\\');
        return File.Open(fullFile, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public void Close(Stream stream)
    {
        stream.Dispose();
    }
}

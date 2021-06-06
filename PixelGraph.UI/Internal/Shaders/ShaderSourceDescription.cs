using PixelGraph.UI.Internal.Utilities;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.UI.Internal.Shaders
{
    public class ShaderSourceDescription : IDisposable
    {
        public string EntryPoint {get; set;} = "main";
        public string Profile {get; set;}

        public string RawFileName {get; set;}
        public string CompiledResourceName {get; set;}
        public ShaderBytecode Code {get; set;}


        public void Dispose()
        {
            Code?.Dispose();
        }

        public bool TryLoadFromPath(string path, IList<ShaderCompileError> errorList)
        {
            var filename = Path.Combine(path, RawFileName);
            if (!File.Exists(filename)) return false;

            using var includeMgr = new CustomShaderFileInclude(path);
            using var result = ShaderBytecode.CompileFromFile(filename, EntryPoint, Profile, include: includeMgr);

            if (result == null || result.HasErrors) {
                errorList.Add(new ShaderCompileError {
                    Filename = filename,
                    Message = result?.Message,
                    // TODO: more details
                });

                return false;
            }

            Code?.Dispose();
            Code = result.Bytecode;
            return true;
        }

        public void LoadFromAssembly()
        {
            var resourcePath = GetResourcePath(CompiledResourceName);
            using var stream = ResourceLoader.Open(resourcePath);

            Code?.Dispose();
            Code = ShaderBytecode.FromStream(stream);
        }

        public static string GetResourcePath(string resourceName)
        {
            return $"PixelGraph.UI.Resources.Shaders.compiled.{resourceName}";
        }
    }

    internal class CustomShaderFileInclude : Include
    {
        public string SourcePath {get;}
        public IDisposable Shadow {get; set;}


        public CustomShaderFileInclude(string sourcePath)
        {
            SourcePath = sourcePath;
        }

        public void Dispose()
        {
            Shadow?.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            var fullFile = Path.Combine(SourcePath, fileName);
            return File.Open(fullFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void Close(Stream stream)
        {
            stream.Dispose();
        }
    }
}

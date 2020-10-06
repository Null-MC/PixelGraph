using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using SysPath = System.IO.Path;

namespace McPbrPipeline.Internal.Input
{
    internal class PbrProperties : PropertiesFile
    {
        public string Name {get; set;}
        public string Path {get; set;}
        public bool UseGlobalMatching {get; set;}

        public bool Wrap => Get("wrap", true);
        public bool ResizeEnabled => Get("resize.enabled", true);

        public string AlbedoTexture => Get<string>("albedo.texture");
        public string AlbedoInputR => Get<string>("albedo.input.r");
        public string AlbedoInputG => Get<string>("albedo.input.g");
        public string AlbedoInputB => Get<string>("albedo.input.b");
        public string AlbedoInputA => Get<string>("albedo.input.a");
        public float AlbedoScaleR => Get("albedo.scale.r", 1f);
        public float AlbedoScaleG => Get("albedo.scale.g", 1f);
        public float AlbedoScaleB => Get("albedo.scale.b", 1f);
        public float AlbedoScaleA => Get("albedo.scale.a", 1f);

        public string HeightTexture => Get<string>("height.texture");
        public float HeightScale => Get("height.scale", 1f);

        public string NormalTexture => Get<string>("normal.texture");
        public string NormalInputR => Get<string>("normal.input.r");
        public string NormalInputG => Get<string>("normal.input.g");
        public string NormalInputB => Get<string>("normal.input.b");
        public string NormalInputA => Get<string>("normal.input.a");
        public bool NormalFromHeight => Get("normal.from-height", true);
        public float NormalStrength => Get("normal.strength", 1f);
        public float NormalDepthScale => Get("normal.depth.scale", 1f);
        public float? NormalX => Get<float?>("normal.x");
        public float? NormalY => Get<float?>("normal.y");
        public float? NormalZ => Get<float?>("normal.z");

        public string SpecularTexture => Get<string>("specular.texture");
        public string SpecularInputR => Get<string>("specular.input.r");
        public string SpecularInputG => Get<string>("specular.input.g");
        public string SpecularInputB => Get<string>("specular.input.b");
        public string SpecularInputA => Get<string>("specular.input.a");
        public string SpecularColor => Get<string>("specular.color");
        public float SmoothScale => Get("smooth.scale", 1f);
        //public float PerceptualSmoothScale => Get("smooth.scale", 1f);
        public float RoughScale => Get("rough.scale", 1f);
        public float ReflectScale => Get("reflect.scale", 1f);

        public string EmissiveTexture => Get<string>("emissive.texture");
        public string EmissiveInputR => Get<string>("emissive.input.r");
        public string EmissiveInputG => Get<string>("emissive.input.g");
        public string EmissiveInputB => Get<string>("emissive.input.b");
        public string EmissiveInputA => Get<string>("emissive.input.a");
        public float EmissiveScale => Get("emissive.scale", 1f);


        public IEnumerable<string> GetAllTextures(IInputReader reader)
        {
            return TextureTags.All
                .Select(tag => GetTextureFile(reader, tag))
                .Where(file => file != null).Distinct();
        }

        public string GetTextureFile(IInputReader reader, string type)
        {
            var filename = TextureTags.Get(this, type);

            while (filename != null) {
                var linkedFilename = TextureTags.Get(this, filename);
                if (string.IsNullOrEmpty(linkedFilename)) break;

                type = filename;
                filename = linkedFilename;
            }

            var srcPath = UseGlobalMatching
                ? Path : SysPath.Combine(Path, Name);

            if (!string.IsNullOrEmpty(filename)) {
                return SysPath.Combine(srcPath, filename);
            }

            var matchName = TextureTags.GetMatchName(this, type);

            return reader.EnumerateFiles(srcPath, matchName).FirstOrDefault(f => {
                var ext = SysPath.GetExtension(f);
                return ImageExtensions.Supported.Contains(ext, StringComparer.InvariantCultureIgnoreCase);
            });
        }
    }
}

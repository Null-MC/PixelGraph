using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Encoding
{
    internal static class EncodingFormat
    {
        public const string Default = "default";
        public const string Legacy = "legacy";
        public const string Lab13 = "lab-1.3";


        public static void ApplyInputFormat(IDictionary<string, string> properties, string format)
        {
            if (string.Equals(Default, format, StringComparison.InvariantCultureIgnoreCase)) {
                Set(properties, "albedo.input.r", EncodingChannel.AlbedoR);
                Set(properties, "albedo.input.g", EncodingChannel.AlbedoG);
                Set(properties, "albedo.input.b", EncodingChannel.AlbedoB);
                Set(properties, "albedo.input.a", EncodingChannel.AlbedoA);

                Set(properties, "height.input.r", EncodingChannel.Height);

                Set(properties, "normal.input.r", EncodingChannel.NormalX);
                Set(properties, "normal.input.g", EncodingChannel.NormalY);
                Set(properties, "normal.input.b", EncodingChannel.NormalZ);

                Set(properties, "emissive.input.r", EncodingChannel.Emissive);

                Set(properties, "occlusion.input.r", EncodingChannel.Occlusion);

                return;
            }

            if (string.Equals(Legacy, format, StringComparison.InvariantCultureIgnoreCase)) {
                Set(properties, "albedo.input.r", EncodingChannel.AlbedoR);
                Set(properties, "albedo.input.g", EncodingChannel.AlbedoG);
                Set(properties, "albedo.input.b", EncodingChannel.AlbedoB);
                Set(properties, "albedo.input.a", EncodingChannel.AlbedoA);

                Set(properties, "normal.input.r", EncodingChannel.NormalX);
                Set(properties, "normal.input.g", EncodingChannel.NormalY);
                Set(properties, "normal.input.b", EncodingChannel.NormalZ);
                Set(properties, "normal.input.a", EncodingChannel.Height);

                Set(properties, "specular.input.r", EncodingChannel.Smooth);
                Set(properties, "specular.input.g", EncodingChannel.Reflect);
                Set(properties, "specular.input.b", EncodingChannel.Emissive);
                return;
            }

            if (string.Equals(Lab13, format, StringComparison.InvariantCultureIgnoreCase)) {
                Set(properties, "albedo.input.r", EncodingChannel.AlbedoR);
                Set(properties, "albedo.input.g", EncodingChannel.AlbedoG);
                Set(properties, "albedo.input.b", EncodingChannel.AlbedoB);
                Set(properties, "albedo.input.a", EncodingChannel.AlbedoA);

                Set(properties, "normal.input.r", EncodingChannel.NormalX);
                Set(properties, "normal.input.g", EncodingChannel.NormalY);
                Set(properties, "normal.input.b", EncodingChannel.Occlusion);
                Set(properties, "normal.input.a", EncodingChannel.Height);

                Set(properties, "specular.input.r", EncodingChannel.PerceptualSmooth);
                Set(properties, "specular.input.g", EncodingChannel.Reflect);
                Set(properties, "specular.input.b", EncodingChannel.Porosity_SSS);
                Set(properties, "specular.input.a", EncodingChannel.Emissive);
                return;
            }

            throw new ApplicationException($"Unknown input format '{format}'!");
        }

        public static void ApplyOutputFormat(IDictionary<string, string> properties, string format)
        {
            if (string.Equals(Default, format, StringComparison.InvariantCultureIgnoreCase)) {
                Set(properties, "output.albedo", bool.TrueString);
                Set(properties, "output.albedo.r", EncodingChannel.AlbedoR);
                Set(properties, "output.albedo.g", EncodingChannel.AlbedoG);
                Set(properties, "output.albedo.b", EncodingChannel.AlbedoB);
                Set(properties, "output.albedo.a", EncodingChannel.AlbedoA);

                Set(properties, "output.height", bool.TrueString);
                Set(properties, "output.height.r", EncodingChannel.Height);
                Set(properties, "output.height.a", EncodingChannel.White);

                Set(properties, "output.normal", bool.TrueString);
                Set(properties, "output.normal.r", EncodingChannel.NormalX);
                Set(properties, "output.normal.g", EncodingChannel.NormalY);
                Set(properties, "output.normal.b", EncodingChannel.NormalZ);
                Set(properties, "output.normal.a", EncodingChannel.White);

                Set(properties, "output.emissive", bool.TrueString);
                Set(properties, "output.emissive.r", EncodingChannel.Emissive);
                Set(properties, "output.emissive.a", EncodingChannel.White);

                Set(properties, "output.occlusion", bool.TrueString);
                Set(properties, "output.occlusion.r", EncodingChannel.Occlusion);
                Set(properties, "output.occlusion.a", EncodingChannel.White);
                return;
            }

            if (string.Equals(Legacy, format, StringComparison.InvariantCultureIgnoreCase)) {
                Set(properties, "output.albedo", bool.TrueString);
                Set(properties, "output.albedo.r", EncodingChannel.AlbedoR);
                Set(properties, "output.albedo.g", EncodingChannel.AlbedoG);
                Set(properties, "output.albedo.b", EncodingChannel.AlbedoB);
                Set(properties, "output.albedo.a", EncodingChannel.AlbedoA);

                Set(properties, "output.normal", bool.TrueString);
                Set(properties, "output.normal.r", EncodingChannel.NormalX);
                Set(properties, "output.normal.g", EncodingChannel.NormalY);
                Set(properties, "output.normal.b", EncodingChannel.NormalZ);
                Set(properties, "output.normal.a", EncodingChannel.Height);

                Set(properties, "output.specular", bool.TrueString);
                Set(properties, "output.specular.r", EncodingChannel.Smooth);
                Set(properties, "output.specular.g", EncodingChannel.Reflect);
                Set(properties, "output.specular.b", EncodingChannel.Emissive);
                return;
            }

            if (string.Equals(Lab13, format, StringComparison.InvariantCultureIgnoreCase)) {
                Set(properties, "output.albedo", bool.TrueString);
                Set(properties, "output.albedo.r", EncodingChannel.AlbedoR);
                Set(properties, "output.albedo.g", EncodingChannel.AlbedoG);
                Set(properties, "output.albedo.b", EncodingChannel.AlbedoB);
                Set(properties, "output.albedo.a", EncodingChannel.AlbedoA);

                Set(properties, "output.normal", bool.TrueString);
                Set(properties, "output.normal.r", EncodingChannel.NormalX);
                Set(properties, "output.normal.g", EncodingChannel.NormalY);
                Set(properties, "output.normal.b", EncodingChannel.Occlusion);
                Set(properties, "output.normal.a", EncodingChannel.Height);

                Set(properties, "output.specular", bool.TrueString);
                Set(properties, "output.specular.r", EncodingChannel.PerceptualSmooth);
                Set(properties, "output.specular.g", EncodingChannel.Reflect);
                Set(properties, "output.specular.b", EncodingChannel.Porosity_SSS);
                Set(properties, "output.specular.a", EncodingChannel.Emissive);
                return;
            }

            throw new ApplicationException($"Unknown output format '{format}'!");
        }

        private static void Set(IDictionary<string, string> properties, string key, string value)
        {
            if (!properties.ContainsKey(key)) properties[key] = value;
        }
    }
}

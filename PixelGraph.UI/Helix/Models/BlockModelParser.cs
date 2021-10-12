using MinecraftMappings.Internal.Models;
using MinecraftMappings.Minecraft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Extensions;
using SharpDX;
using System;
using System.IO;
using System.Linq;

namespace PixelGraph.UI.Helix.Models
{
    internal interface IBlockModelParser
    {
        ModelVersion LoadRecursive(string rootPath, string localFile);
    }

    internal class BlockModelParser : IBlockModelParser
    {
        public ModelVersion LoadRecursive(string rootPath, string localFile)
        {
            var finalModel = new ModelVersion();
            var filename = localFile;

            do {
                var parentModel = FindModel(rootPath, filename);
                if (parentModel == null) break;

                MergeModels(finalModel, parentModel);
                filename = parentModel.Parent;
            }
            while (filename != null);

            if (finalModel.Elements.Count == 0)
                throw new ApplicationException("No elements found in model hierarchy!");

            return finalModel;
        }

        private static ModelVersion FindModel(string rootPath, string localFile)
        {
            var searchPath = rootPath;

            var localFile2 = PathEx.Localize(localFile);

            if (!localFile2.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                localFile2 += ".json";

            var modelsBlockPath = PathEx.Join(rootPath, "assets/minecraft/models/block", localFile2);
            modelsBlockPath = PathEx.Localize(modelsBlockPath);

            if (File.Exists(modelsBlockPath)) {
                var json = ParseModelJson(modelsBlockPath);
                return ParseModelFile(json);
            }

            var modelsPath = PathEx.Join(rootPath, "assets/minecraft/models", localFile2);
            modelsPath = PathEx.Localize(modelsPath);

            if (File.Exists(modelsPath)) {
                var json = ParseModelJson(modelsPath);
                return ParseModelFile(json);
            }

            while (searchPath != null) {
                var localPath = PathEx.Join(searchPath, localFile2);
                localPath = PathEx.Localize(localPath);

                //if (!localPath.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                //    localPath += ".json";

                if (File.Exists(localPath)) {
                    var json = ParseModelJson(localPath);
                    return ParseModelFile(json);
                }

                searchPath = Path.GetDirectoryName(searchPath);
            }

            // TODO: locate matching path segment

            var name = Path.GetFileName(localFile);

            var parentModel = Minecraft.Java.FindModelVersionById(name).FirstOrDefault();
            if (parentModel != null) return parentModel;

            throw new ApplicationException($"Failed to locate parent model file '{name}'!");
        }

        private static ModelVersion ParseModelFile(JToken jsonData)
        {
            var model = new ModelVersion {
                Parent = jsonData.Value<string>("parent"),
            };

            var textureDataList = jsonData.Value<JObject>("textures");
            if (textureDataList != null) {
                foreach (var textureData in textureDataList.Properties())
                    model.Textures[textureData.Name] = textureData.Value.ToObject<string>();
            }

            var elementDataList = jsonData.Value<JArray>("elements");
            if (elementDataList != null) {
                foreach (var elementData in elementDataList) {
                    var element = new ModelElement();
                    ParseElement(element, elementData);
                    model.Elements.Add(element);
                }
            }

            return model;
        }

        private static void ParseElement(ModelElement modelElement, JToken element)
        {
            modelElement.Name = element.Value<string>("name");

            var from_array = element.Value<JArray>("from")?.ToObject<float[]>();
            if (from_array == null || from_array.Length != 3)
                throw new ApplicationException("Element 'from' must contain 3 values!");

            modelElement.From.X = from_array[0];
            modelElement.From.Y = from_array[1];
            modelElement.From.Z = from_array[2];

            var to_array = element.Value<JArray>("to")?.ToObject<float[]>();
            if (to_array == null || to_array.Length != 3)
                throw new ApplicationException("Element 'to' must contain 3 values!");

            modelElement.To.X = to_array[0];
            modelElement.To.Y = to_array[1];
            modelElement.To.Z = to_array[2];

            var faces = element.Value<JObject>("faces");
            if (faces == null || !faces.HasValues)
                throw new ApplicationException("Element has no faces");

            var rotationData = element.Value<JObject>("rotation");
            if (rotationData != null) {
                modelElement.Rotation = new ModelElementRotation();

                var origin_array = rotationData.Value<JArray>("origin")?.ToObject<float[]>();
                if (origin_array != null) {
                    if (origin_array.Length != 3)
                        throw new ApplicationException("Element rotation origin expects 3 parts!");

                    modelElement.Rotation.Origin.X = origin_array[0];
                    modelElement.Rotation.Origin.Y = origin_array[1];
                    modelElement.Rotation.Origin.Z = origin_array[2];
                }

                var axisName = rotationData.Value<string>("axis");
                if (!ModelAxiss.TryParse(axisName, out modelElement.Rotation.Axis))
                    throw new ApplicationException("Element rotation axis is undefined!");

                modelElement.Rotation.Angle = rotationData.Value<decimal>("angle");
            }

            foreach (var (faceName, faceData) in faces) {
                if (!ElementFace.TryParse(faceName, out var face))
                    throw new ApplicationException($"Unknown element face '{face}'!");

                var elementFace = new ModelFace {
                    Texture = faceData.Value<string>("texture"),
                    Rotation = faceData.Value<int?>("rotation") ?? 0,
                };

                var uv_array = faceData.Value<JArray>("uv")?.ToObject<float[]>();
                if (uv_array != null) {
                    if (uv_array.Length != 4)
                        throw new ApplicationException($"Element face '{faceName}' uv must contain 4 values!");

                    elementFace.UV = new RectangleF {
                        Left = uv_array[0],
                        Top = uv_array[1],
                        Right = uv_array[2],
                        Bottom = uv_array[3],
                    };
                }

                modelElement.SetFace(face, elementFace);
            }
        }

        private static void MergeModels(ModelVersion editModel, ModelVersion parentModel)
        {
            foreach (var textureName in parentModel.Textures.Keys) {
                if (editModel.Textures.ContainsKey(textureName)) continue;
                editModel.Textures[textureName] = parentModel.Textures[textureName];
            }

            if (editModel.Elements.Count == 0)
                editModel.Elements.AddRange(parentModel.Elements);

            editModel.Parent = parentModel.Parent;
        }

        private static JObject ParseModelJson(string filename)
        {
            using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            return JObject.Load(jsonReader);
        }
    }
}

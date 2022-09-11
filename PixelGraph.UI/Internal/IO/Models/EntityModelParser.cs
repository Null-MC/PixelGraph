using MinecraftMappings.Internal.Models.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.UI.Internal.IO.Models
{
    //internal interface IEntityModelParser
    //{
    //    void Build<T>(T baseModel, string localFile) where T : EntityModelVersion, new();
    //}

    internal class EntityModelParser //: IEntityModelParser
    {
        private readonly MinecraftResourceLocator locator;


        public EntityModelParser(MinecraftResourceLocator locator)
        {
            this.locator = locator;
        }

        public void Build<T>(T baseModel, string localFile)
            where T : EntityModelVersion, new()
        {
            var customModel = FindModel<T>(localFile);
            if (customModel == null) return;

            foreach (var model in customModel.Elements) {
                if (!baseModel.TryReplacePart(model))
                    throw new ApplicationException($"Unable to locate base element for part '{model.Part}'!");
            }
        }

        private T FindModel<T>(string searchFile)
            where T : EntityModelVersion, new()
        {
            var modelFile = searchFile;

            if (modelFile.StartsWith("entity/")) modelFile = modelFile[7..];

            if (!modelFile.EndsWith(".jem", StringComparison.InvariantCultureIgnoreCase))
                modelFile = $"{modelFile}.jem";

            if (!modelFile.Contains('/', StringComparison.InvariantCulture))
                modelFile = $"assets/minecraft/optifine/cem/{modelFile}";

            JObject json = null;
            var result = locator.FindEntityModel(modelFile, stream => {
                using var reader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(reader);
                json = JObject.Load(jsonReader);

            });

            if (result) return ParseModelFile<T>(json);

            return null;
            //var name = Path.GetFileName(searchFile);

            //var parentModel = Minecraft.Java.FindEntityModelVersionById<T>(name).FirstOrDefault();
            //if (parentModel != null) return parentModel;

            //throw new ApplicationException($"Failed to locate parent model file '{name}'!");
        }

        private EntityModelPart FindModelPart(string searchFile)
        {
            var modelFile = searchFile;

            if (!modelFile.EndsWith(".jpm", StringComparison.InvariantCultureIgnoreCase))
                modelFile = $"{modelFile}.jpm";

            if (!modelFile.Contains("/", StringComparison.InvariantCulture))
                modelFile = $"assets/minecraft/optifine/cem/{modelFile}";

            JObject json = null;
            var result = locator.FindEntityModel(modelFile, stream => {
                using var reader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(reader);
                json = JObject.Load(jsonReader);
            });

            if (result) return ParseModelPartFile(json);
            
            var name = Path.GetFileName(searchFile);

            //var parentModel = Minecraft.Java.FindEntityModelVersionById<T>(name).FirstOrDefault();
            //if (parentModel != null) return parentModel;

            throw new ApplicationException($"Failed to locate parent model file '{name}'!");
        }

        private T ParseModelFile<T>(JToken jsonData)
            where T : EntityModelVersion, new()
        {
            var model = new T {
                Texture = jsonData.Value<string>("texture"),
            };

            var textureSize = jsonData.Value<JArray>("textureSize")?.ToObject<int[]>();
            if (textureSize != null) {
                if (textureSize is not { Length: 2 })
                    throw new ApplicationException("Entity 'textureSize' must contain 2 values!");

                model.TextureSize = new Vector2(textureSize[0], textureSize[1]);
            }

            var elementDataList = jsonData.Value<JArray>("models");
            if (elementDataList != null) {
                foreach (var elementData in elementDataList) {
                    var element = new EntityElement();
                    ParseElement(element, elementData);
                    model.Elements.Add(element);
                }
            }

            return model;
        }

        private EntityModelPart ParseModelPartFile(JToken jsonData)
        {
            var model = new EntityModelPart {
                Texture = jsonData.Value<string>("folder/texture"),
            };

            var textureSize = jsonData.Value<JArray>("textureSize")?.ToObject<int[]>();
            if (textureSize != null) {
                if (textureSize is not { Length: 2 })
                    throw new ApplicationException("Entity 'textureSize' must contain 2 values!");

                model.TextureSize = new Vector2(textureSize[0], textureSize[1]);
            }

            var elementDataList = jsonData.Value<JArray>("submodels");
            if (elementDataList != null) {
                foreach (var elementData in elementDataList) {
                    var element = new EntityElement();
                    ParseElement(element, elementData);
                    model.Submodels.Add(element);
                }
            }

            return model;
        }

        private void ParseElement(EntityElement modelElement, JToken element)
        {
            modelElement.Id = element.Value<string>("id");
            modelElement.Part = element.Value<string>("part");

            var model_file = element.Value<string>("model");
            if (model_file != null) {
                try {
                    var partModel = FindModelPart(model_file);
                    if (partModel == null) throw new ApplicationException($"Unable to locate entity model part '{model_file}'!");

                    modelElement.Model = partModel;
                }
                catch (Exception) {
                    // TODO: log?
                }

                //var elementDataList = jsonData.Value<JArray>("models");
                //if (elementDataList != null) {
                //    foreach (var elementData in elementDataList) {
                //        var element = new EntityElement();
                //        ParseElement(element, elementData);
                //        model.Elements.Add(element);
                //    }
                //}
                //modelElement.Model = partModel;

                // TODO: Add support for loading JPM model parts
                //throw new NotImplementedException("JPM is not currently supported!");
                //return;
            }
            
            var translate_array = element.Value<JArray>("translate")?.ToObject<float[]>();
            if (translate_array != null) {
                if (translate_array != null && translate_array is not {Length: 3})
                    throw new ApplicationException("Element 'translate' must contain 3 values!");

                modelElement.Translate.X = translate_array[0];
                modelElement.Translate.Y = translate_array[1];
                modelElement.Translate.Z = translate_array[2];
            }

            var rotate_array = element.Value<JArray>("rotate")?.ToObject<float[]>();
            if (rotate_array != null) {
                if (rotate_array is not {Length: 3})
                    throw new ApplicationException("Element 'rotate' must contain 3 values!");

                modelElement.RotationAngleX = rotate_array[0];
                modelElement.RotationAngleY = rotate_array[1];
                modelElement.RotationAngleZ = rotate_array[2];
            }

            var invertAxis_data = element.Value<string>("invertAxis");
            if (invertAxis_data != null) {
                // WARN: This is probably NOT how the "invertAxis" property works!
                modelElement.InvertAxisX = invertAxis_data.Contains('x', StringComparison.InvariantCultureIgnoreCase);
                modelElement.InvertAxisY = invertAxis_data.Contains('y', StringComparison.InvariantCultureIgnoreCase);
                modelElement.InvertAxisZ = invertAxis_data.Contains('z', StringComparison.InvariantCultureIgnoreCase);
            }

            var mirrorTexture_data = element.Value<string>("mirrorTexture");
            modelElement.MirrorTexU = string.Equals(mirrorTexture_data, "u", StringComparison.InvariantCultureIgnoreCase); 

            var boxes = element.Value<JArray>("boxes");
            if (boxes != null) {
                foreach (var box in boxes) {
                    var modelBox = new EntityElementCube();
                    modelElement.Boxes.Add(modelBox);

                    var coordinates_array = box.Value<JArray>("coordinates")?.ToObject<float[]>();
                    if (coordinates_array is not {Length: 6})
                        throw new ApplicationException("Element box 'coordinates' must contain 6 values!");

                    modelBox.Position.X = coordinates_array[0];
                    modelBox.Position.Y = coordinates_array[1];
                    modelBox.Position.Z = coordinates_array[2];
                    modelBox.Size.X = coordinates_array[3];
                    modelBox.Size.Y = coordinates_array[4];
                    modelBox.Size.Z = coordinates_array[5];

                    var textureOffset_array = box.Value<JArray>("textureOffset")?.ToObject<float[]>();
                    if (textureOffset_array != null) {
                        if (textureOffset_array.Length != 2)
                            throw new ApplicationException("Element box 'textureOffset' must contain 2 values!");

                        modelBox.UV.X = textureOffset_array[0];
                        modelBox.UV.Y = textureOffset_array[1];
                    }

                    var sizeAdd = box.Value<float?>("sizeAdd");
                    if (sizeAdd.HasValue) modelBox.SizeAdd = sizeAdd.Value;

                    var uvDown_array = box.Value<JArray>("uvDown")?.ToObject<float[]>();
                    if (uvDown_array != null) modelBox.UV_Down = UVMap(uvDown_array);

                    var uvUp_array = box.Value<JArray>("uvUp")?.ToObject<float[]>();
                    if (uvUp_array != null) modelBox.UV_Up = UVMap(uvUp_array);

                    var uvNorth_array = box.Value<JArray>("uvNorth")?.ToObject<float[]>();
                    if (uvNorth_array != null) modelBox.UV_North = UVMap(uvNorth_array);

                    var uvSouth_array = box.Value<JArray>("uvSouth")?.ToObject<float[]>();
                    if (uvSouth_array != null) modelBox.UV_South = UVMap(uvSouth_array);

                    var uvWest_array = box.Value<JArray>("uvWest")?.ToObject<float[]>();
                    if (uvWest_array != null) modelBox.UV_West = UVMap(uvWest_array);

                    var uvEast_array = box.Value<JArray>("uvEast")?.ToObject<float[]>();
                    if (uvEast_array != null) modelBox.UV_East = UVMap(uvEast_array);
                }
            }

            var submodels = element.Value<JArray>("submodels");
            if (submodels != null) {
                foreach (var submodel in submodels) {
                    var modelSub = new EntityElement();
                    ParseElement(modelSub, submodel);
                    modelElement.Submodels.Add(modelSub);
                }
            }
        }

        private static RectangleF UVMap(IReadOnlyList<float> region)
        {
            return new RectangleF(region[0], region[1], region[2] - region[0], region[3] - region[1]);
        }
    }
}

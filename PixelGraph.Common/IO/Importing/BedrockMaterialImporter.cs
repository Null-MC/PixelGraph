using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Importing
{
    internal class BedrockMaterialImporter : MaterialImporterBase
    {
        private static readonly Regex isPathMaterialExp = new(@"(?:^|\/)textures\/(?:blocks|entity)(?:$|\/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public BedrockMaterialImporter(IServiceProvider provider) : base(provider) {}

        public override bool IsMaterialFile(string filename, out string name)
        {
            name = Path.GetFileNameWithoutExtension(filename);
            
            // TODO: get all input names
            //foreach (var tag in context.OutputEncoding.Select()) {
            //    var tagName = texWriter.Get(name, tag);
            //}

            if (filename.EndsWith(".texture_set.json", StringComparison.InvariantCultureIgnoreCase)) {
                name = name[..^12];
                return true;
            }

            if (name.EndsWith("_heightmap", StringComparison.InvariantCultureIgnoreCase)) {
                name = name[..^10];
                return true;
            }

            if (name.EndsWith("_normal", StringComparison.InvariantCultureIgnoreCase)) {
                name = name[..^7];
                return true;
            }

            if (name.EndsWith("_mer", StringComparison.InvariantCultureIgnoreCase)) {
                name = name[..^4];
                return true;
            }

            var path = Path.GetDirectoryName(filename);
            path = PathEx.Normalize(path);
            if (path != null && isPathMaterialExp.IsMatch(path)) return true;

            return false;
        }

        protected override async Task OnImportMaterialAsync(IServiceProvider scope, CancellationToken token = default)
        {
            var context = scope.GetRequiredService<ITextureGraphContext>();

            context.Mapping = new BedrockToJavaImportMapping();

            await ParseTextureSetAsync(context, token);

            await base.OnImportMaterialAsync(scope, token);
        }

        private async Task ParseTextureSetAsync(ITextureGraphContext context, CancellationToken token)
        {
            var localPath = PathEx.Localize(context.Material.LocalPath);
            var textureSetFilename = PathEx.Join(localPath, $"{context.Material.Name}.texture_set.json");
            if (!Reader.FileExists(textureSetFilename)) return;

            var data = await ParseJsonAsync(textureSetFilename, token);
            var textureSet = data.SelectToken("['minecraft:texture_set']");
            if (textureSet == null) throw new ApplicationException("Invalid texture_set json file!");

            var colorData = textureSet.SelectToken("color");
            if (colorData != null) {
                var channelColorR = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.ColorRed));
                var channelColorG = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.ColorGreen));
                var channelColorB = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.ColorBlue));
                var channelOpacity = context.InputEncoding.FirstOrDefault(e => TextureTags.Is(e.ID, TextureTags.Opacity));
                var opacityRange = ((float?)channelOpacity?.MaxValue ?? 0f) - ((float?)channelOpacity?.MinValue ?? 0f);

                if (colorData.Type == JTokenType.String) {
                    var colorValue = colorData.Value<string>();

                    if (colorValue?.StartsWith('#') ?? false) {
                        if (colorValue.Length is not (7 or 9))
                            throw new ApplicationException("Expected 6 or 8 characters in hexadecimal color value!");

                        var hasAlpha = colorValue.Length == 9;
                        context.Material.Color.Value = hasAlpha ? $"#{colorValue[3 - 8]}" : colorValue;

                        channelColorR?.Reset();
                        channelColorG?.Reset();
                        channelColorB?.Reset();

                        if (channelOpacity != null && hasAlpha) {
                            var opacity = byte.Parse(colorValue[7..8], NumberStyles.HexNumber);
                            context.Material.Opacity.Value = (decimal)(opacity / 255f * opacityRange) + channelOpacity.MinValue;
                            channelOpacity.Reset();
                        }
                    }
                    else {
                        var filename = PathEx.Join(context.Material.LocalPath, colorValue);
                        if (channelColorR != null) channelColorR.__Filename = filename;
                        if (channelColorG != null) channelColorG.__Filename = filename;
                        if (channelColorB != null) channelColorB.__Filename = filename;
                        if (channelOpacity != null) channelOpacity.__Filename = filename;
                    }
                }
                else if (colorData.Type == JTokenType.Array) {
                    var colorValues = colorData.Values<byte>().ToArray();

                    if (colorValues.Length is not (3 or 4))
                        throw new ApplicationException("Expected 3 or 4 color values!");

                    context.Material.Color.Value = $"#{colorValues[0]:X2}{colorValues[1]:X2}{colorValues[2]:X2}";

                    context.Project.Input.ColorRed.Reset();
                    context.Project.Input.ColorGreen.Reset();
                    context.Project.Input.ColorBlue.Reset();

                    if (channelOpacity != null && colorValues.Length == 4) {
                        context.Material.Opacity.Value = (decimal)(colorValues[3] / 255f * opacityRange) + channelOpacity.MinValue;
                        context.Project.Input.Opacity.Reset();
                    }
                }
                else { throw new ApplicationException($"Unexpected data-type '{colorData.Type}' for element 'minecraft:texture_set/color'!"); }
            }

            var merData = textureSet.SelectToken("metalness_emissive_roughness");
            if (merData != null) {
                if (merData.Type == JTokenType.String) {
                    var merValue = merData.Value<string>();

                    if (merValue?.StartsWith('#') ?? false) {
                        if (merValue.Length is not 7)
                            throw new ApplicationException("Expected 6 characters in hexadecimal MER value!");

                        var metal = byte.Parse(merValue[1..2], NumberStyles.HexNumber);
                        var emissive = byte.Parse(merValue[3..4], NumberStyles.HexNumber);
                        var rough = byte.Parse(merValue[5..6], NumberStyles.HexNumber);

                        var metalChannel = context.InputEncoding.FirstOrDefault(e => TextureTags.Is(e.ID, TextureTags.Metal));
                        if (metalChannel != null) {
                            var metalRange = ((float?)metalChannel.MaxValue ?? 0f) - ((float?)metalChannel.MinValue ?? 0f);
                            context.Material.Metal.Value = (decimal)(metal / 255f * metalRange) + metalChannel.MinValue;
                            context.Project.Input.Metal.Reset();
                        }

                        var emissiveChannel = context.InputEncoding.FirstOrDefault(e => TextureTags.Is(e.ID, TextureTags.Emissive));
                        if (emissiveChannel != null) {
                            var emissiveRange = ((float?)emissiveChannel.MaxValue ?? 0f) - ((float?)emissiveChannel.MinValue ?? 0f);
                            context.Material.Metal.Value = (decimal)(emissive / 255f * emissiveRange) + emissiveChannel.MinValue;
                            context.Project.Input.Emissive.Reset();
                        }

                        var roughChannel = context.InputEncoding.FirstOrDefault(e => TextureTags.Is(e.ID, TextureTags.Rough));
                        if (roughChannel != null) {
                            var roughRange = ((float?)roughChannel.MaxValue ?? 0f) - ((float?)roughChannel.MinValue ?? 0f);
                            context.Material.Rough.Value = (decimal)(rough / 255f * roughRange) + roughChannel.MinValue;
                            context.Project.Input.Rough.Reset();
                        }
                    }
                    else {
                        var channelMetal = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.Metal));
                        var channelEmissive = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.Emissive));
                        var channelRough = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.Rough));

                        var filename = PathEx.Join(context.Material.LocalPath, merValue);
                        if (channelMetal != null) channelMetal.__Filename = filename;
                        if (channelEmissive != null) channelEmissive.__Filename = filename;
                        if (channelRough != null) channelRough.__Filename = filename;
                    }
                }
                else if (merData.Type == JTokenType.Array) {
                    var merValues = merData.Values<byte>().ToArray();
                    if (merValues.Length != 3) throw new ApplicationException("Expected 3 MER values!");

                    var metalChannel = context.InputEncoding.FirstOrDefault(e => TextureTags.Is(e.ID, TextureTags.Metal));
                    if (metalChannel != null) {
                        var metalRange = ((float?)metalChannel.MaxValue ?? 0f) - ((float?)metalChannel.MinValue ?? 0f);
                        context.Material.Metal.Value = (decimal)(merValues[0] / 255f * metalRange) + metalChannel.MinValue;
                        context.Project.Input.Metal.Reset();
                    }

                    var emissiveChannel = context.InputEncoding.FirstOrDefault(e => TextureTags.Is(e.ID, TextureTags.Emissive));
                    if (emissiveChannel != null) {
                        var emissiveRange = ((float?)emissiveChannel.MaxValue ?? 0f) - ((float?)emissiveChannel.MinValue ?? 0f);
                        context.Material.Metal.Value = (decimal)(merValues[1] / 255f * emissiveRange) + emissiveChannel.MinValue;
                        context.Project.Input.Emissive.Reset();
                    }

                    var roughChannel = context.InputEncoding.FirstOrDefault(e => TextureTags.Is(e.ID, TextureTags.Rough));
                    if (roughChannel != null) {
                        var roughRange = ((float?)roughChannel.MaxValue ?? 0f) - ((float?)roughChannel.MinValue ?? 0f);
                        context.Material.Rough.Value = (decimal)(merValues[1] / 255f * roughRange) + roughChannel.MinValue;
                        context.Project.Input.Rough.Reset();
                    }
                }
                else { throw new ApplicationException($"Unexpected data-type '{merData.Type}' for element 'minecraft:texture_set/metalness_emissive_roughness'!"); }
            }

            var normalData = textureSet.SelectToken("normal");
            if (normalData != null) {
                var channelHeight = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.Height));
                if (channelHeight != null) channelHeight.Texture = null;

                if (normalData.Type == JTokenType.String) {
                    var normalValue = normalData.Value<string>();

                    var channelNormalX = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.NormalX));
                    var channelNormalY = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.NormalY));
                    var channelNormalZ = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.NormalZ));

                    var filename = PathEx.Join(context.Material.LocalPath, normalValue);
                    if (channelNormalX != null) channelNormalX.__Filename = filename;
                    if (channelNormalY != null) channelNormalY.__Filename = filename;
                    if (channelNormalZ != null) channelNormalZ.__Filename = filename;
                }
                else { throw new ApplicationException($"Unexpected data-type '{normalData.Type}' for element 'minecraft:texture_set/normal'!"); }
            }

            var heightData = textureSet.SelectToken("heightmap");
            if (heightData != null) {
                var channelNormalX = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.NormalX));
                var channelNormalY = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.NormalY));
                var channelNormalZ = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.NormalZ));

                if (channelNormalX != null) channelNormalX.Texture = null;
                if (channelNormalY != null) channelNormalY.Texture = null;
                if (channelNormalZ != null) channelNormalZ.Texture = null;

                if (heightData.Type == JTokenType.String) {
                    var heightValue = heightData.Value<string>();

                    var channelHeight = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.Height));

                    var filename = PathEx.Join(context.Material.LocalPath, heightValue);
                    if (channelHeight != null) channelHeight.__Filename = filename;
                }
                else { throw new ApplicationException($"Unexpected data-type '{heightData.Type}' for element 'minecraft:texture_set/heightmap'!"); }
            }

            //if (normalData == null && heightData == null) throw new ApplicationException("Invalid texture_set, no heightmap or normal data!");
            //if (normalData != null && heightData != null) throw new ApplicationException("Invalid texture_set, contains both heightmap & normal data!");
        }

        private async Task<JObject> ParseJsonAsync(string localFile, CancellationToken token)
        {
            try {
                await using var stream = Reader.Open(localFile);
                using var reader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(reader);
                return await JObject.LoadAsync(jsonReader, token);
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to parse JSON file '{localFile}'!", error);
            }
        }
    }
}

using PixelGraph.Common.IO;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.InputTests
{
    public class InputMappingTests : TestBase
    {
        public InputMappingTests(ITestOutputHelper output) : base(output) {}

        // Substance
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_basecolor.png", TextureTags.Color)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_ambientocclusion.png", TextureTags.Occlusion)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_height.png", TextureTags.Height)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_metal.png", TextureTags.Metal)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_metallic.png", TextureTags.Metal)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_normal.png", TextureTags.Normal)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_porosity.png", TextureTags.Porosity)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_roughness.png", TextureTags.Rough)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_SSS.png", TextureTags.SubSurfaceScattering)]
        [InlineData("assets/minecraft/textures/block/iron_block/Substance_graph_emissive.png", TextureTags.Emissive)]

        [InlineData("assets/minecraft/textures/block/iron_block/opacity.png", TextureTags.Opacity)]
        [InlineData("assets/minecraft/textures/block/iron_block/alpha.png", TextureTags.Opacity)]
        [InlineData("assets/minecraft/textures/block/iron_block/color.png", TextureTags.Color)]
        [InlineData("assets/minecraft/textures/block/iron_block/basecolor.png", TextureTags.Color)]
        [InlineData("assets/minecraft/textures/block/iron_block/albedo.png", TextureTags.Color)]
        [InlineData("assets/minecraft/textures/block/iron_block/diffuse.png", TextureTags.Color)]
        [InlineData("assets/minecraft/textures/block/iron_block/height.png", TextureTags.Height)]
        [InlineData("assets/minecraft/textures/block/iron_block/bump.png", TextureTags.Bump)]
        [InlineData("assets/minecraft/textures/block/iron_block/normal.png", TextureTags.Normal)]
        [InlineData("assets/minecraft/textures/block/iron_block/occlusion.png", TextureTags.Occlusion)]
        [InlineData("assets/minecraft/textures/block/iron_block/ambientOcclusion.png", TextureTags.Occlusion)]
        [InlineData("assets/minecraft/textures/block/iron_block/AO.png", TextureTags.Occlusion)]
        [InlineData("assets/minecraft/textures/block/iron_block/smooth.png", TextureTags.Smooth)]
        [InlineData("assets/minecraft/textures/block/iron_block/smoothness.png", TextureTags.Smooth)]
        [InlineData("assets/minecraft/textures/block/iron_block/rough.png", TextureTags.Rough)]
        [InlineData("assets/minecraft/textures/block/iron_block/roughness.png", TextureTags.Rough)]
        [InlineData("assets/minecraft/textures/block/iron_block/f0.png", TextureTags.F0)]
        [InlineData("assets/minecraft/textures/block/iron_block/metal.png", TextureTags.Metal)]
        [InlineData("assets/minecraft/textures/block/iron_block/metallic.png", TextureTags.Metal)]
        [InlineData("assets/minecraft/textures/block/iron_block/metalness.png", TextureTags.Metal)]
        [InlineData("assets/minecraft/textures/block/iron_block/emissive.png", TextureTags.Emissive)]
        [InlineData("assets/minecraft/textures/block/iron_block/emission.png", TextureTags.Emissive)]
        [InlineData("assets/minecraft/textures/block/iron_block/porosity.png", TextureTags.Porosity)]
        [InlineData("assets/minecraft/textures/block/iron_block/sss.png", TextureTags.SubSurfaceScattering)]
        [InlineData("assets/minecraft/textures/block/iron_block/scattering.png", TextureTags.SubSurfaceScattering)]
        [Theory] public void DetectsFileAsType(string filename, string type)
        {
            Assert.True(NamingStructure.IsLocalFileTag(filename, type));
        }

        [InlineData("assets/minecraft/textures/block/iron_block/rebasecolor.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/poopacity.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/ralpha.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/alphas.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/occlusioned.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/AOL.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/opacity.ignore.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/opacity-x.png")]
        [InlineData("assets/minecraft/textures/block/iron_block/asss.png")]
        [Theory] public void IgnoresFiles(string filename)
        {
            Assert.False(NamingStructure.IsLocalFileTag(filename));
        }
    }
}

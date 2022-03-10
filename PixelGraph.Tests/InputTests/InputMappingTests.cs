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
        [InlineData("~/Substance_graph_basecolor.png", TextureTags.Color)]
        [InlineData("~/Substance_graph_ambientocclusion.png", TextureTags.Occlusion)]
        [InlineData("~/Substance_graph_height.png", TextureTags.Height)]
        [InlineData("~/Substance_graph_metal.png", TextureTags.Metal)]
        [InlineData("~/Substance_graph_metallic.png", TextureTags.Metal)]
        [InlineData("~/Substance_graph_normal.png", TextureTags.Normal)]
        [InlineData("~/Substance_graph_porosity.png", TextureTags.Porosity)]
        [InlineData("~/Substance_graph_roughness.png", TextureTags.Rough)]
        [InlineData("~/Substance_graph_SSS.png", TextureTags.SubSurfaceScattering)]
        [InlineData("~/Substance_graph_emissive.png", TextureTags.Emissive)]

        [InlineData("~/opacity.png", TextureTags.Opacity)]
        [InlineData("~/alpha.png", TextureTags.Opacity)]
        [InlineData("~/color.png", TextureTags.Color)]
        [InlineData("~/basecolor.png", TextureTags.Color)]
        [InlineData("~/albedo.png", TextureTags.Color)]
        [InlineData("~/diffuse.png", TextureTags.Color)]
        [InlineData("~/height.png", TextureTags.Height)]
        [InlineData("~/bump.png", TextureTags.Bump)]
        [InlineData("~/normal.png", TextureTags.Normal)]
        [InlineData("~/occlusion.png", TextureTags.Occlusion)]
        [InlineData("~/ambientOcclusion.png", TextureTags.Occlusion)]
        [InlineData("~/AO.png", TextureTags.Occlusion)]
        [InlineData("~/smooth.png", TextureTags.Smooth)]
        [InlineData("~/smoothness.png", TextureTags.Smooth)]
        [InlineData("~/rough.png", TextureTags.Rough)]
        [InlineData("~/roughness.png", TextureTags.Rough)]
        [InlineData("~/f0.png", TextureTags.F0)]
        [InlineData("~/metal.png", TextureTags.Metal)]
        [InlineData("~/metallic.png", TextureTags.Metal)]
        [InlineData("~/metalness.png", TextureTags.Metal)]
        [InlineData("~/emissive.png", TextureTags.Emissive)]
        [InlineData("~/emission.png", TextureTags.Emissive)]
        [InlineData("~/porosity.png", TextureTags.Porosity)]
        [InlineData("~/sss.png", TextureTags.SubSurfaceScattering)]
        [InlineData("~/scattering.png", TextureTags.SubSurfaceScattering)]
        [Theory] public void DetectsFileAsType(string filename, string type)
        {
            Assert.True(NamingStructure.IsLocalFileTag(filename, type));
        }

        [InlineData("~/rebasecolor.png")]
        [InlineData("~/poopacity.png")]
        [InlineData("~/ralpha.png")]
        [InlineData("~/alphas.png")]
        [InlineData("~/occlusioned.png")]
        [InlineData("~/AOL.png")]
        [InlineData("~/opacity.ignore.png")]
        [InlineData("~/opacity-x.png")]
        [InlineData("~/asss.png")]
        [Theory] public void IgnoresFiles(string filename)
        {
            Assert.False(NamingStructure.IsLocalFileTag(filename));
        }
    }
}

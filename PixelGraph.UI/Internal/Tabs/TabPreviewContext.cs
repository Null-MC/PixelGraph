using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Preview;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

#if !NORENDER
using PixelGraph.Rendering.Models;
using PixelGraph.UI.Helix.Models;
#endif

namespace PixelGraph.UI.Internal.Tabs
{
    public class TabPreviewContext : IDisposable
    {
        private readonly object lockHandle;
        private CancellationTokenSource tokenSource;
        private BitmapSource _layerImageSource;
        
        public Guid Id {get; set;}
        public string SourceFile {get; set;}
        public Image LayerImage {get; set;}
        public bool IsMaterialBuilderValid {get; private set;}
        public bool IsMaterialValid {get; private set;}
        public bool IsLayerValid {get; private set;}
        public bool IsLayerSourceValid {get; private set;}

#if !NORENDER
        public MultiPartMeshBuilder Mesh {get;}
#endif


        public TabPreviewContext(IServiceProvider provider)
        {
            lockHandle = new object();

#if !RELEASENORENDER
            Mesh = provider.GetRequiredService<MultiPartMeshBuilder>();
#endif
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#if !NORENDER
        public async Task BuildModelMeshAsync(IRenderContext renderContext, CancellationToken token = default)
        {
            var mergedToken = StartNewToken(token);

            await Mesh.BuildAsync(renderContext, mergedToken);
            //Mesh.UpdateModelParts();

            //IsMaterialBuilderValid = true;
        }

        public void UpdateMaterials(IRenderContext renderContext)
        {
            Mesh.UpdateMaterials(renderContext);
            Mesh.UpdateModelParts();
            IsMaterialBuilderValid = true;
        }

        public void UpdateModelParts()
        {
            Mesh.UpdateModelParts();
            IsMaterialBuilderValid = true;
        }
#endif

        public void SetImageSource(Image image)
        {
            LayerImage?.Dispose();
            LayerImage = image;
            IsLayerValid = true;
            IsLayerSourceValid = false;
        }

        public BitmapSource GetLayerImageSource()
        {
            if (IsLayerSourceValid) return _layerImageSource;

            if (SourceFile != null) {
                var texImage = BuildFileSource(SourceFile);

                if (texImage.CanFreeze) texImage.Freeze();
                IsLayerSourceValid = true;
                return _layerImageSource = texImage;
            }

            if (!IsLayerValid) return null;

            _layerImageSource = LayerImage switch {
                Image<Rgb24> imageRgb24 => new ImageSharpSource<Rgb24>(imageRgb24),
                Image<L8> imageL8 => new ImageSharpSource<L8>(imageL8),
                _ => null,
            };

            if (_layerImageSource != null) {
                //_layerImageSource.in

                if (_layerImageSource.CanFreeze) _layerImageSource.Freeze();
            }

            IsLayerSourceValid = true;
            return _layerImageSource;
        }

        #if !NORENDER

        public void InvalidateMaterialBuilder(bool clear)
        {
            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
            //if (clear) ModelMaterial = null;
            if (clear) Mesh.ClearTextureBuilders();
        }

        public void InvalidateMaterialBuilder(string[] channels, bool clear)
        {
            // TODO: only clear the builders containing the channels

            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
            //if (clear) ModelMaterial = null;
            if (clear) Mesh.ClearTextureBuilders();
        }

        public void InvalidateMaterial(bool clear)
        {
            IsMaterialValid = false;
            //if (clear) ModelMaterial = null;
            if (clear) Mesh.ClearTextureBuilders();
        }
#endif

        public void InvalidateLayer(bool clear)
        {
            IsLayerValid = false;
            IsLayerSourceValid = false;

            if (clear) {
                LayerImage?.Dispose();
                LayerImage = null;
                _layerImageSource = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
#if !NORENDER
            //materialBuilder?.Dispose();
            Mesh.Dispose();
#endif

            LayerImage?.Dispose();
            LayerImage = null;
            tokenSource?.Dispose();
            IsLayerValid = false;
            IsMaterialBuilderValid = false;
            IsMaterialValid = false;
        }
        
#if !NORENDER

        private CancellationToken StartNewToken(CancellationToken token)
        {
            lock (lockHandle) {
                tokenSource?.Cancel();
                tokenSource?.Dispose();

                tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                return tokenSource.Token;
            }
        }
#endif

        private static BitmapSource BuildFileSource(string filename)
        {
            var texImage = new BitmapImage();

            texImage.BeginInit();
            texImage.CacheOption = BitmapCacheOption.None; //.OnLoad;
            texImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            texImage.UriSource = new Uri(filename);
            texImage.EndInit();

            return texImage;
        }
    }
}

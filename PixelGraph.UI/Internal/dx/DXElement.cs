using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelGraph.UI.Internal.dx
{
    /// <summary>
    /// A <see cref="UIElement"/> displaying DirectX scene. 
    /// Takes care of resizing and refreshing a <see cref="DXImageSource"/>.
    /// It does no Direct3D work, which is delegated to
    /// the <see cref="IDirect3D"/> <see cref="Renderer"/> object.
    /// </summary>
    public class DXElement : FrameworkElement, INotifyPropertyChanged
    {
        private readonly DXImageSource surface;
        private readonly Stopwatch renderTimer;
        private bool mIsLoopRendering = true;
        private DrawEventArgs lastDEA;
        private bool IsReallyLoopRendering => mIsReallyLoopRendering;
        private bool mIsReallyLoopRendering;

        public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// The image source where the DirectX scene (from the <see cref="Renderer"/>) will be rendered.
		/// </summary>
		public DXImageSource Surface => surface;

		/// <summary>
		/// The D3D device that will handle the drawing
		/// </summary>
		public IDirect3D Renderer
		{
			get => (IDirect3D)GetValue(RendererProperty);
            set => SetValue(RendererProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode
        /// (running in Blend or Visual Studio).
        /// </summary>
        public bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(this);

		/// <summary>
		/// Whether or not the DirectX scene will be redrawn continuously
		/// </summary>
		public bool IsLoopRendering
		{
			get => mIsLoopRendering;
            set {
				if (value == mIsLoopRendering) return;
				mIsLoopRendering = value;
				UpdateReallyLoopRendering();
				OnPropertyChanged("IsLoopRendering");
			}
		}


		public DXElement()
        {
			SnapsToDevicePixels = true;

            renderTimer = new Stopwatch();
			surface = new DXImageSource();
			surface.IsFrontBufferAvailableChanged += delegate {
				UpdateReallyLoopRendering();

				if (!IsReallyLoopRendering && surface.IsFrontBufferAvailable)
					Render();
			};

			IsVisibleChanged += delegate {
                UpdateReallyLoopRendering();
            };
        }

        /// <summary>
        /// Will redraw the underlying surface once.
        /// </summary>
        public void Render()
        {
            if (Renderer == null || IsInDesignMode) return;

            Renderer.Render(GetDrawEventArgs());
            Surface.Invalidate();
        }

        public DrawEventArgs GetDrawEventArgs()
        {
            return lastDEA = new DrawEventArgs {
                TotalTime = renderTimer.Elapsed,
                DeltaTime = lastDEA != null ? renderTimer.Elapsed - lastDEA.TotalTime : TimeSpan.Zero,
                RenderSize = DesiredSize,
                Target = Surface,
            };
        }

		protected override Size ArrangeOverride(Size finalSize)
		{
			base.ArrangeOverride(finalSize);
			UpdateSize();
			return finalSize;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			var w = (int)Math.Ceiling(availableSize.Width);
			var h = (int)Math.Ceiling(availableSize.Height);
			return new Size(w, h);
		}

		protected override Visual GetVisualChild(int index)
		{
			throw new ArgumentOutOfRangeException();
		}

		protected override int VisualChildrenCount => 0;

        protected override void OnRender(DrawingContext dc)
		{
			dc.DrawImage(Surface, new Rect(RenderSize));
		}

        private void UpdateReallyLoopRendering()
		{
			var newValue = !IsInDesignMode
                           && IsLoopRendering
                           && Renderer != null
                           && Surface.IsFrontBufferAvailable
                           && VisualParent != null
                           && IsVisible;

            if (newValue == IsReallyLoopRendering) return;
            mIsReallyLoopRendering = newValue;

            if (IsReallyLoopRendering) {
                renderTimer.Start();
                CompositionTarget.Rendering += OnLoopRendering;
            }
            else {
                CompositionTarget.Rendering -= OnLoopRendering;
                renderTimer.Stop();
            }
        }

        private void OnLoopRendering(object sender, EventArgs e) 
		{
			if (!IsReallyLoopRendering) return;
			Render(); 
		}

        private void UpdateSize()
		{
			if (Renderer == null) return;
			Renderer.Reset(GetDrawEventArgs());
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			if (Renderer is IInteractiveDirect3D id3d)
				id3d.OnMouseDown(this, e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (Renderer is IInteractiveDirect3D id3d)
				id3d.OnMouseMove(this, e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnMouseUp(this, e);
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			base.OnMouseWheel(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnMouseWheel(this, e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnKeyDown(this, e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (Renderer is IInteractiveDirect3D)
				((IInteractiveDirect3D)Renderer).OnKeyUp(this, e);
		}

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (IsInDesignMode) return;
            UpdateReallyLoopRendering();
        }

        private void OnRendererChanged(IDirect3D oldValue, IDirect3D newValue)
        {
            UpdateSize();
            UpdateReallyLoopRendering();
            Focusable = newValue is IInteractiveDirect3D;
        }

        private void OnPropertyChanged(string name)
		{
			var e = PropertyChanged;
            e?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static readonly DependencyProperty RendererProperty =
            DependencyProperty.Register(
                "Renderer",
                typeof(IDirect3D),
                typeof(DXElement),
                new PropertyMetadata((d, e) => ((DXElement)d).OnRendererChanged((IDirect3D)e.OldValue, (IDirect3D)e.NewValue)));
    }
}

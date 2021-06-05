using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Internal.dx
{
    /// <summary>
    /// A vanilla implementation of <see cref="IDirect3D"/> with some common wiring already done.
    /// </summary>
    public abstract class D3D : IDirect3D, IInteractiveDirect3D, INotifyPropertyChanged, IDisposable
	{
        private BaseCamera mCurrentCamera;

        public event EventHandler<DrawEventArgs> Resetted;
        public event EventHandler<DrawEventArgs> Rendering;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Size set with call to <see cref="Reset(DrawEventArgs)"/>
        /// </summary>
        public Vector2 RenderSize { get; protected set; }

        /// <summary>
        /// Time in the last <see cref="DrawEventArgs"/> passed to <see cref="Render(DrawEventArgs)"/>
        /// </summary>
        public TimeSpan RenderTime { get; protected set; }

        public BaseCamera CurrentCamera {
            get => mCurrentCamera;
            set {
                if (value == mCurrentCamera) return;
                mCurrentCamera = value;
                OnPropertyChanged("CurrentCamera");
            }
        }


        protected D3D()
		{
			OnInteractiveInit();
		}

        ~D3D()
        {
            Dispose(false);
        }

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {}

		public virtual void Reset(DrawEventArgs args)
		{
			var w = (int)Math.Ceiling(args.RenderSize.Width);
			var h = (int)Math.Ceiling(args.RenderSize.Height);
            if (w < 1 || h < 1) return;

			RenderSize = new Vector2(w, h);
			if (CurrentCamera != null)
				CurrentCamera.AspectRatio = (float)(args.RenderSize.Width / args.RenderSize.Height);

			Reset(w, h);
            Resetted?.Invoke(this, args);
            Render(args);

			if (args.Target != null)
				SetBackBuffer(args.Target);
		}

		public virtual void Reset(int w, int h) {}

		/// <summary>
		/// SharpDX 1.3 requires explicit dispose of all its ComObject.
		/// This method makes it easy.
		/// (Remark: I attempted to hack a correct Dispose implementation but it crashed the app on first GC!)
		/// </summary>
		public static void Set<T>(ref T field, T newValue)
			where T : IDisposable
		{
			if (field != null)
				field.Dispose();

			field = newValue;
		}

		public abstract System.Windows.Media.Imaging.WriteableBitmap ToImage();

		public abstract void SetBackBuffer(DXImageSource dximage);

		public void Render(DrawEventArgs args)
		{
			RenderTime = args.TotalTime;

			if (CurrentCamera != null)
				CurrentCamera.FrameMove(args.DeltaTime);

			BeginRender(args);
			RenderScene(args);
			EndRender(args);
		}

		public virtual void BeginRender(DrawEventArgs args) { }

		public virtual void RenderScene(DrawEventArgs args)
        {
            Rendering?.Invoke(this, args);
        }

		public virtual void EndRender(DrawEventArgs args) { }

		void OnInteractiveInit();

        /// <summary>
        /// Override this to focus the view, capture the mouse and select the <see cref="CurrentCamera"/> 
        /// </summary>
        public virtual void OnMouseDown(UIElement ui, MouseButtonEventArgs e)
        {
            if (CurrentCamera == null) return;
            ui.CaptureMouse();
            ui.Focus();
            CurrentCamera.HandleMouseDown(ui, e);
        }

        public virtual void OnMouseMove(UIElement ui, MouseEventArgs e)
        {
            if (CurrentCamera != null && ui.IsMouseCaptured)
                CurrentCamera.HandleMouseMove(ui, e);
        }

        public virtual void OnMouseUp(UIElement ui, MouseButtonEventArgs e)
        {
            if (CurrentCamera != null && ui.IsMouseCaptured)
                CurrentCamera.HandleMouseUp(ui, e);

            ui.ReleaseMouseCapture();
        }

        public virtual void OnMouseWheel(UIElement ui, MouseWheelEventArgs e)
        {
            if (CurrentCamera != null)
                CurrentCamera.HandleMouseWheel(ui, e);
        }

        public virtual void OnKeyDown(UIElement ui, KeyEventArgs e)
        {
            if (CurrentCamera != null)
                CurrentCamera.HandleKeyDown(ui, e);
        }

        public virtual void OnKeyUp(UIElement ui, KeyEventArgs e)
        {
            if (CurrentCamera != null)
                CurrentCamera.HandleKeyUp(ui, e);
        }

        protected void OnPropertyChanged(string name)
        {
            var e = PropertyChanged;
            e?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

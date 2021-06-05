using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.UI.Internal.dx
{
    /// <summary>
    /// SharpDX 1.3 requires explicit dispose of all its ComObject.
    /// This method makes it easier.
    /// (Remark: I attempted to hack a correct Dispose implementation but it crashed the app on first GC!)
    /// </summary>
    public class DisposeGroup : IDisposable
    {
        private readonly List<IDisposable> list;


        public DisposeGroup()
        {
            list = new List<IDisposable>();
        }

        public void Dispose()
        {
            for (var i = list.Count - 1; i >= 0; i--) {
                var d = list[i];
                list.RemoveAt(i);
                d.Dispose();
            }
        }

        public void Add(params IDisposable[] objects)
        {
            list.AddRange(from o in objects where o != null select o);
        }

        public T Add<T>(T ob)
            where T : IDisposable
        {
            if (ob != null) list.Add(ob);
            return ob;
        }
    }
}

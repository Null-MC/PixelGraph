using PixelGraph.Common.IO.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Projects
{
    public interface IProjectContextManager
    {
        IProjectContext GetContext();
        void SetContext(IProjectContext context);
        Task SaveAsync();
    }

    internal class ProjectContextManager : IProjectContextManager, IDisposable
    {
        private readonly ProjectSerializer serializer;
        private readonly ReaderWriterLockSlim _lock;
        private IProjectContext _context;


        public ProjectContextManager()
        {
            serializer = new ProjectSerializer();
            _lock = new ReaderWriterLockSlim();
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }

        public IProjectContext GetContext()
        {
            _lock.EnterReadLock();

            try {
                return _context;
            }
            finally {
                _lock.ExitReadLock();
            }
        }

        public void SetContext(IProjectContext context)
        {
            _lock.EnterWriteLock();

            try {
                _context = context;
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        public Task SaveAsync()
        {
            var context = GetContext();
            if (context == null) return Task.CompletedTask;

            return serializer.SaveAsync(context.Project, context.ProjectFilename);
        }
    }
}

using PixelGraph.Common.IO.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    public interface IProjectContextManager
    {
        ProjectContext GetContext();
        void SetContext(ProjectContext context);
        Task SaveAsync();
    }

    internal class ProjectContextManager : IProjectContextManager, IDisposable
    {
        private readonly ProjectSerializer serializer;
        private readonly ReaderWriterLockSlim _lock;
        private ProjectContext _context;


        public ProjectContextManager()
        {
            serializer = new ProjectSerializer();
            _lock = new ReaderWriterLockSlim();
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }

        public ProjectContext GetContext()
        {
            _lock.EnterReadLock();

            try {
                return _context;
            }
            finally {
                _lock.ExitReadLock();
            }
        }

        public void SetContext(ProjectContext context)
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

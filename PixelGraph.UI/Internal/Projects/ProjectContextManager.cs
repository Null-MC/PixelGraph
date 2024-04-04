using PixelGraph.Common.IO.Serialization;

namespace PixelGraph.UI.Internal.Projects;

public interface IProjectContextManager
{
    IProjectContext? GetContext();
    IProjectContext GetContextRequired();
    void SetContext(IProjectContext? context);
    Task SaveAsync();
}

internal class ProjectContextManager : IProjectContextManager, IDisposable
{
    private readonly ProjectSerializer serializer = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private IProjectContext? _context;


    public void Dispose()
    {
        _lock.Dispose();
    }

    public IProjectContext? GetContext()
    {
        _lock.EnterReadLock();

        try {
            return _context;
        }
        finally {
            _lock.ExitReadLock();
        }
    }

    public IProjectContext GetContextRequired() => GetContext() ?? throw new ApplicationException("Failed to retrieve project context!");

    public void SetContext(IProjectContext? context)
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
        var context = GetContextRequired();

        if (context.Project == null) {
            // logger.Warn();
            return Task.CompletedTask;
        }

        if (string.IsNullOrEmpty(context.ProjectFilename))
            throw new ApplicationException("Project filename is undefined!");

        return serializer.SaveAsync(context.Project, context.ProjectFilename);
    }
}

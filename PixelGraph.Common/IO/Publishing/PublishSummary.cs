using System.Threading;

namespace PixelGraph.Common.IO.Publishing;

public interface IPublishSummary
{
    int TextureCount {get;}
    int MaterialCount {get;}
    long DiskByteSize {get;}
    long RawByteSize {get;}
    //int UntrackedFileCount {get;}

    string DiskSize {get;}
    string RawSize {get;}

    void Reset();
    void IncrementMaterialCount();
    void IncrementTextureCount();
    void AddRawBytes(in long size);
    void AddRawBytes(in int width, in int height);

    void AddDiskBytes(in long size);
    //void AddTextureCount(in int count);
}

internal class PublishSummary : IPublishSummary
{
    private int _textureCount, _materialCount;
    private long _diskByteSize, _rawByteSize;

    public int TextureCount => _textureCount;
    public int MaterialCount => _materialCount;
    public long DiskByteSize => _diskByteSize;
    public long RawByteSize => _rawByteSize;

    public string DiskSize => UnitHelper.GetReadableSize(_diskByteSize);
    public string RawSize => UnitHelper.GetReadableSize(_rawByteSize);


    public void Reset()
    {
        _textureCount = 0;
        _materialCount = 0;
        _diskByteSize = 0;
        _rawByteSize = 0;
    }

    public void IncrementMaterialCount()
    {
        Interlocked.Increment(ref _materialCount);
    }

    public void IncrementTextureCount()
    {
        Interlocked.Increment(ref _textureCount);
    }

    public void AddDiskBytes(in long size)
    {
        Interlocked.Add(ref _diskByteSize, size);
    }

    public void AddRawBytes(in long size)
    {
        Interlocked.Add(ref _rawByteSize, size);
    }

    public void AddRawBytes(in int width, in int height)
    {
        long size = width * height * 4;
        Interlocked.Add(ref _rawByteSize, size);
    }

    //public void AddTextureCount(in int count)
    //{
    //    Interlocked.Add(ref _textureCount, count);
    //}
}
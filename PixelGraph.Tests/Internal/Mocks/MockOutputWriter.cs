using System;
using System.IO;
using System.Threading.Tasks;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;

namespace PixelGraph.Tests.Internal.Mocks
{
    internal class MockOutputWriter : IOutputWriter
    {
        public MockFileContent Content {get;}
        public string Root {get; set;} = ".";


        public MockOutputWriter(MockFileContent content)
        {
            Content = content;
        }

        public void SetRoot(string absolutePath)
        {
            Root = absolutePath;
        }

        public void Prepare() {}

        public Stream Open(string localFilename)
        {
            var mockStream = new MockStream();
            Content.Add(localFilename, mockStream);
            return mockStream;
        }

        public bool FileExists(string localFile)
        {
            var fullFile = PathEx.Join(Root, localFile);
            return Content.FileExists(fullFile);
        }

        public DateTime? GetWriteTime(string localFile) => null;

        public void Delete(string localFile)
        {
            throw new NotImplementedException();
        }

        public void Clean()
        {
            Content.Files.Clear();
        }

        public void Dispose() {}
        public ValueTask DisposeAsync() => default;
    }

    public class MockStream : Stream
    {
        public MemoryStream BaseStream {get;}

        public override long Position {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override bool CanRead => BaseStream.CanRead;
        public override bool CanSeek => BaseStream.CanSeek;
        public override bool CanWrite => BaseStream.CanWrite;
        public override long Length => BaseStream.Length;


        public MockStream()
        {
            BaseStream = new MemoryStream();
        }

        public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

        public override void SetLength(long value) => BaseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

        public override void Flush() => BaseStream.Flush();

        protected override void Dispose(bool disposing)
        {
            Flush();
        }

        public override async ValueTask DisposeAsync()
        {
            await FlushAsync();
        }
    }
}

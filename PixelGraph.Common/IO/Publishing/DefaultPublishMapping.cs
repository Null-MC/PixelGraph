namespace PixelGraph.Common.IO.Publishing
{
    public interface IDefaultPublishMapping : IPublisherMapping {}

    internal class DefaultPublishMapping : PublisherMappingBase, IDefaultPublishMapping
    {
        public override bool Contains(string sourceFile)
        {
            return true;
        }

        public override bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName)
        {
            destPath = sourcePath;
            destName = sourceName;
            return true;
        }
    }
}

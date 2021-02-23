namespace PixelGraph.UI.Internal
{
    internal interface IAppConfiguration
    {
        //bool PublishAsArchive {get; set;}
        //bool PublishCleanDestination {get; set;}
        bool PublishCloseOnComplete {get; set;}
        //bool AutoMaterial {get; set;}
    }

    internal class AppConfiguration : IAppConfiguration
    {
        //public bool PublishAsArchive {get; set;}
        //public bool PublishCleanDestination {get; set;}
        public bool PublishCloseOnComplete {get; set;}
        //public bool AutoMaterial {get; set;}


        //public AppSettings()
        //{
        //    AutoMaterial = true;
        //}
    }
}

﻿using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    internal interface IAppSettings
    {
        //bool PublishAsArchive {get; set;}
        //bool PublishCleanDestination {get; set;}
        bool PublishCloseOnComplete {get; set;}

        Task SaveAsync();
    }

    internal class AppSettings : IAppSettings
    {
        //public bool PublishAsArchive {get; set;}
        //public bool PublishCleanDestination {get; set;}
        public bool PublishCloseOnComplete {get; set;}


        public Task SaveAsync()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
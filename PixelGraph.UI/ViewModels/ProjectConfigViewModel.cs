using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using System;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class ProjectConfigViewModel : ModelBase
    {
        private IProjectContextManager projectContextMgr;
        private ProjectData _project;

        public ProjectData Project {
            get => _project;
            private set {
                _project = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(PackName));
                OnPropertyChanged(nameof(PackDescription));
                OnPropertyChanged(nameof(PackAuthor));
                OnPropertyChanged(nameof(Format));
            }
        }

        public string PackName {
            get => Project?.Name;
            set {
                if (Project == null) return;
                Project.Name = value;
                OnPropertyChanged();
            }
        }

        public string PackDescription {
            get => Project?.Description;
            set {
                if (Project == null) return;
                Project.Description = value;
                OnPropertyChanged();
            }
        }

        public string PackAuthor {
            get => Project?.Author;
            set {
                if (Project == null) return;
                Project.Author = value;
                OnPropertyChanged();
            }
        }

        public string Format {
            get => Project?.Input?.Format;
            set {
                if (Project == null) return;
                Project.Input ??= new PackInputEncoding();
                if (Project.Input.Format == value) return;
                Project.Input.Format = value;
                OnPropertyChanged();
            }
        }


        public void Initialize(IServiceProvider provider)
        {
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            Project = (ProjectData)projectContextMgr.GetContext()?.Project.Clone();
        }

        public async Task SaveAsync()
        {
            var projectContext = projectContextMgr.GetContext();

            projectContext.Project = Project;

            await projectContextMgr.SaveAsync();
        }
    }
}

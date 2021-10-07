using System;

namespace MinecraftMappings.Internal.Models
{
    public class ModelVersionBuilder
    {
        private readonly ModelVersion modelVersion;


        public ModelVersionBuilder(ModelVersion modelVersion)
        {
            this.modelVersion = modelVersion;
        }

        public ModelVersionBuilder WithPath(string path)
        {
            modelVersion.Path = path;
            return this;
        }

        public ModelVersionBuilder AddElement(Action<ModelElement> elementAction)
        {
            var element = new ModelElement();
            modelVersion.Elements.Add(element);

            //var elementBuilder = new ModelElementBuilder(element);
            elementAction(element);

            return this;
        }
    }
}

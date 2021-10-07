using System;

namespace MinecraftMappings.Internal
{
    public abstract class Versionable
    {
        private readonly Lazy<Version> _parsedVersion;

        public string TextVersion {get; set;}

        public Version ParsedVersion => _parsedVersion.Value;


        protected Versionable()
        {
            _parsedVersion = new Lazy<Version>(() => {
                if (TextVersion == null) return null;
                return Version.Parse(TextVersion);
            });
        }
    }
}

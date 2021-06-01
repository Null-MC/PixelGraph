namespace MinecraftMappings.Internal
{
    public abstract class JavaEntityData : EntityData<JavaEntityDataVersion>
    {
        protected JavaEntityData(string name) : base(name) {}
    }

    public class JavaEntityDataVersion : EntityDataVersion {}
}

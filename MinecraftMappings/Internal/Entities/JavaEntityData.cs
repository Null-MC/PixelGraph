namespace MinecraftMappings.Internal.Entities
{
    public abstract class JavaEntityData : EntityData<JavaEntityDataVersion>
    {
        protected JavaEntityData(string name) : base(name) {}
    }

    public class JavaEntityDataVersion : EntityDataVersion {}
}

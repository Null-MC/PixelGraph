namespace MinecraftMappings.Internal
{
    public abstract class BedrockEntityData : EntityData<BedrockEntityDataVersion>
    {
        protected BedrockEntityData(string name) : base(name) {}
    }

    public class BedrockEntityDataVersion : EntityDataVersion {}
}

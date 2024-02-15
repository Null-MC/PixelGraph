using System.ComponentModel;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace PixelGraph.Common.IO.Serialization;

public class CustomGraphVisitor : ChainedObjectGraphVisitor
{
    public CustomGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor) : base(nextVisitor) {}

    private static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    public override bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, IEmitter context)
    {
        return !Equals(value.Value, GetDefault(value.Type))
               && base.EnterMapping(key, value, context);
    }

    public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
    {
        if (value.Value is IHaveData dataObj) return dataObj.HasAnyData();

        var defaultValueAttribute = key.GetCustomAttribute<DefaultValueAttribute>();
        var defaultValue = defaultValueAttribute?.Value;

        return !Equals(value.Value, defaultValue)
               && base.EnterMapping(key, value, context);
    }
}

using System.ComponentModel;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace PixelGraph.Common.IO.Serialization;

public class CustomGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor) : ChainedObjectGraphVisitor(nextVisitor)
{
    private static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    public override bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
    {
        return !Equals(value.Value, GetDefault(value.Type))
               && base.EnterMapping(key, value, context, serializer);
    }

    public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
    {
        if (value.Value is IHaveData dataObj) return dataObj.HasAnyData();

        var defaultValueAttribute = key.GetCustomAttribute<DefaultValueAttribute>();
        var defaultValue = defaultValueAttribute?.Value;

        return !Equals(value.Value, defaultValue)
               && base.EnterMapping(key, value, context, serializer);
    }
}

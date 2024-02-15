using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Extensions;

public class YamlStringEnumConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type.IsEnum || (Nullable.GetUnderlyingType(type)?.IsEnum ?? false);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        var parsedEnum = parser.Consume<Scalar>();

        var nullType = Nullable.GetUnderlyingType(type);
        var t = nullType ?? type;

        var serializableValues = ScanAssembly(t).ToDictionary(pa => pa.Key, pa => pa.Value);

        if (!serializableValues.ContainsKey(parsedEnum.Value))
            throw new YamlException(parsedEnum.Start, parsedEnum.End, $"Value '{parsedEnum.Value}' not found in enum '{t.Name}'");

        return Enum.Parse(t, serializableValues[parsedEnum.Value].Name);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var nullType = Nullable.GetUnderlyingType(type);
        var t = nullType ?? type;

        var memberName = value?.ToString() ?? throw new ApplicationException("Member name is undefined!");
        var enumMember = t.GetMember(memberName).FirstOrDefault();

        var yamlValue = enumMember?.GetCustomAttributes<EnumMemberAttribute>(true)
            .Select(ema => ema.Value).FirstOrDefault() ?? value.ToString();

        if (yamlValue != null) emitter.Emit(new Scalar(yamlValue));
    }

    private static IEnumerable<KeyValuePair<string, MemberInfo>> ScanAssembly(Type type) {
        foreach (var member in type.GetMembers()) {
            var key = member.GetCustomAttributes<EnumMemberAttribute>(true)
                .Select(ema => ema.Value).FirstOrDefault();

            if (!string.IsNullOrEmpty(key))
                yield return new KeyValuePair<string, MemberInfo>(key, member);
        }
    }
}

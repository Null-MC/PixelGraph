using System;
using System.Runtime.Serialization;

namespace PixelGraph.Common.Textures
{
    [Flags]
    public enum ColorChannel
    {
        [EnumMember(Value = "none")]
        None,

        [EnumMember(Value = "red")]
        Red,

        [EnumMember(Value = "green")]
        Green,

        [EnumMember(Value = "blue")]
        Blue,

        [EnumMember(Value = "alpha")]
        Alpha,
    }
}

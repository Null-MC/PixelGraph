using PixelGraph.Common.Material;
using System;
using System.ComponentModel;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace PixelGraph.Common.IO.Serialization
{
    public class CustomObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public CustomObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor) : base(nextVisitor) {}

        private static object GetDefault(Type type)
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
            if (value.Value is MaterialOpacityProperties matOpacity) return matOpacity.HasAnyData();
            if (value.Value is MaterialColorProperties matColor) return matColor.HasAnyData();
            if (value.Value is MaterialHeightProperties matHeight) return matHeight.HasAnyData();
            if (value.Value is MaterialBumpProperties matBump) return matBump.HasAnyData();
            if (value.Value is MaterialNormalProperties matNormal) return matNormal.HasAnyData();
            if (value.Value is MaterialOcclusionProperties matOcclusion) return matOcclusion.HasAnyData();
            if (value.Value is MaterialSpecularProperties matSpecular) return matSpecular.HasAnyData();
            if (value.Value is MaterialSmoothProperties matSmooth) return matSmooth.HasAnyData();
            if (value.Value is MaterialRoughProperties matRough) return matRough.HasAnyData();
            if (value.Value is MaterialMetalProperties matMetal) return matMetal.HasAnyData();
            if (value.Value is MaterialHcmProperties matHcm) return matHcm.HasAnyData();
            if (value.Value is MaterialF0Properties matF0) return matF0.HasAnyData();
            if (value.Value is MaterialPorosityProperties matPorosity) return matPorosity.HasAnyData();
            if (value.Value is MaterialSssProperties matSss) return matSss.HasAnyData();
            if (value.Value is MaterialEmissiveProperties matEmissive) return matEmissive.HasAnyData();

            if (value.Value is MaterialConnectionProperties matCTM) return matCTM.HasAnyData();

            var defaultValueAttribute = key.GetCustomAttribute<DefaultValueAttribute>();
            var defaultValue = defaultValueAttribute?.Value;

            return !Equals(value.Value, defaultValue)
                   && base.EnterMapping(key, value, context);
        }
    }}

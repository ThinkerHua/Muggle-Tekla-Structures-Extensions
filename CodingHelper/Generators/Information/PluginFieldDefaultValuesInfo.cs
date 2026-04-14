using System;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct PluginFieldDefaultValuesInfo : IRequiredInformation<PluginFieldDefaultValuesInfo> {

        public ClassInfo ClassInfo;

        public AttributeTargets TargetType;

        public string TargetMemberName;

        public ArgumentsDictionary<DefaultValueDictionary> Arguments;
    }
}

using Microsoft.CodeAnalysis;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct PluginFieldsInfo : IRequiredInformation<PluginFieldsInfo> {

        public ClassInfo ClassInfo;

        public ITypeSymbol DataType;

        public ArgumentsDictionary<NameOrNumberSet> Arguments;
    }
}

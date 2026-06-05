using Microsoft.CodeAnalysis;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct PluginFieldsInfo {

        public ClassInfo ClassInfo;

        public ITypeSymbol DataType;

        public ArgumentsDictionary<IdSet> Arguments;
    }
}

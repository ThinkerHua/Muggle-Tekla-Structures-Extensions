namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct PluginDataFieldsInfo : IRequiredInformation<PluginDataFieldsInfo> {

        public ClassInfo ClassInfo;

        public ArgumentsDictionary<NameOrNumberSet> Arguments;
    }
}

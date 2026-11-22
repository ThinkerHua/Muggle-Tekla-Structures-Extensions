namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct UserInterfaceInfo(
        UserInterfaceType UiType,
        string ViewModelQualifiedName,
        ClassInfo ClassInfo,
        PluginTypes PluginType,
        string PluginName,
        ArgumentsDictionary<IdSet> Arguments) {

        public UserInterfaceType UiType = UiType;

        public string ViewModelQualifiedName = ViewModelQualifiedName;

        public ClassInfo ClassInfo = ClassInfo;

        public PluginTypes PluginType = PluginType;

        public string PluginName = PluginName;

        public ArgumentsDictionary<IdSet> Arguments = Arguments;

    }
}
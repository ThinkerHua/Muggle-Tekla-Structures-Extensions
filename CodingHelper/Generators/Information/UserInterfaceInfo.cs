namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct UserInterfaceInfo(
        UserInterfaceType UiType,
        string ViewModelQualifiedName,
        ClassInfo ClassInfo,
        PluginTypes PluginType,
        ArgumentsDictionary<IdSet> Arguments) {
        
        public UserInterfaceType UiType = UiType;
        
        public string ViewModelQualifiedName = ViewModelQualifiedName;
        
        public ClassInfo ClassInfo = ClassInfo;

        public PluginTypes PluginType = PluginType;
        
        public ArgumentsDictionary<IdSet> Arguments = Arguments;

    }
}
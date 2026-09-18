namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct UserInterfaceInfo(
        UserInterfaceType UiType,
        string ViewModelQualifiedName,
        PluginDataFieldsInfo PluginDataFieldsInfo) {

        /// <summary>
        /// <inheritdoc cref="UserInterfaceAttribute.UiType"/>
        /// </summary>
        public UserInterfaceType UiType = UiType;
        
        /// <summary>
        /// <inheritdoc cref="UserInterfaceAttribute.ViewModelQualifiedName"/>
        /// </summary>
        public string ViewModelQualifiedName = ViewModelQualifiedName;
        
        public PluginDataFieldsInfo PluginDataFieldsInfo = PluginDataFieldsInfo;

    }
}
namespace Muggle.TsExtensions.CodingHelper.Generators.Information {
    internal record struct PluginDataFieldsInfo {
        public ClassInfo ClassInfo;

        /// <summary>
        /// <list type="bullet">
        ///     <item>Key - for 'GeneralFieldsAttribute', key is one of 'int', 'double', 'string';
        ///         for other '*FieldsAttribute', key is attribute name.</item>
        ///     <item>Value - id set.</item>
        /// </list>
        /// </summary>
        public ArgumentsDictionary<IdSet> Arguments;
    }
}
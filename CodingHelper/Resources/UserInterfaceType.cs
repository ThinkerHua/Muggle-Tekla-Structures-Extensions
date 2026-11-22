namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// User interface type.
    /// </summary>
    public enum UserInterfaceType {
        /// <summary>
        /// INP-type user interface.
        /// </summary>
        INP,

        /// <summary>
        /// WinForm-type user interface.
        /// </summary>
        [System.Obsolete("Not supported yet.", true)] WinForm,

        /// <summary>
        /// WPF-type user interface.
        /// </summary>
        WPF
    }
}
using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Used to generate a simple user interface for plugin.
    /// </summary>
    /// <remarks>Should be applied on plugin data class.</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class UserInterfaceAttribute : Attribute {

        /// <summary>
        /// The type of user interface to be generated.
        /// </summary>
        public UserInterfaceType UiType { get; set; }

        /// <summary>
        /// Only used when <see cref="UiType"/> is <see cref="UserInterfaceType.WPF"/>.
        /// </summary>
        public string ViewModelQualifiedName { get; set; }

        /// <summary>
        /// Register to generate a user interface.
        /// </summary>
        /// <param name="uiType"><inheritdoc cref="UiType"/></param>
        /// <param name="viewModelQualifiedName"><inheritdoc cref="ViewModelQualifiedName"/></param>
        public UserInterfaceAttribute(UserInterfaceType uiType, string viewModelQualifiedName) {
            UiType = uiType;
            ViewModelQualifiedName = UiType is UserInterfaceType.WPF && viewModelQualifiedName is not null
                ? viewModelQualifiedName
                : "";
        }
    }
}
using System.Windows.Media;

namespace Muggle.TsExtensions.Common.WPF.DataTemplates {

    /// <summary>
    /// 符号。
    /// </summary>
    public class Symbol {

        /// <summary>
        /// 符号名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 符号图像。
        /// </summary>
        /// <remarks>推荐高度12像素，推荐宽度不超过28像素。</remarks>
        public DrawingGroup Drawing { get; set; }
    }
}
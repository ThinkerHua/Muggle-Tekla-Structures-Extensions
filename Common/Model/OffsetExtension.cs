using System;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {
    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="Offset"/> 的扩展。
    /// </summary>
    public static class OffsetExtension {
        /// <summary>
        /// 克隆一个实例。
        /// </summary>
        /// <param name="offset">当前实例</param>
        /// <returns>克隆的实例。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Offset Clone(this Offset offset) {
            if (offset is null) {
                throw new ArgumentNullException(nameof(offset));
            }

            return new Offset() {
                Dx = offset.Dx,
                Dy = offset.Dy,
                Dz = offset.Dz,
            };
        }
    }
}

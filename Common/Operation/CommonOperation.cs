/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
 *
 *  Copyright © 2024 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  CommonOperation.cs: commonly operations
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections.Generic;

namespace Muggle.TsExtensions.Common.Operation {
    /// <summary>
    /// 通用操作
    /// </summary>
    public static class CommonOperation {
        /// <summary>
        /// 交换数据。
        /// </summary>
        /// <typeparam name="T">执行交换操作的数据类型</typeparam>
        /// <param name="a">要交换的数据</param>
        /// <param name="b">要交换的数据</param>
        public static void Swap<T>(ref T a, ref T b) {
            T t;
            t = a;
            a = b;
            b = t;
        }
        /// <summary>
        /// 极值枚举
        /// </summary>
        public enum ExtremeTypeEnum {
            /// <summary>
            /// 局部极小值
            /// </summary>
            LocalMinimum,
            /// <summary>
            /// 局部极大值
            /// </summary>
            LocalMaximum,
        }
        /// <summary>
        /// 找出数据集合 <paramref name="data"/> 中的局部极值，并返回其序号集合。
        /// </summary>
        /// <typeparam name="T">数据集合中数据的类型。</typeparam>
        /// <param name="data">给定数据集合。</param>
        /// <param name="type">极值类型。</param>
        /// <param name="interval">用来判断极值的最小区间，应当为 3 以上的奇数，输入偶数则向上进 1。</param>
        /// <returns>可能的局部极值的序号集合。入参为 null 或不符合规则，则返回 null。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">数据集合 <paramref name="data"/> 为空时引发。</exception>
        public static List<int> GetLocalExtremeIndexes<T>(
            IList<T> data,
            ExtremeTypeEnum type,
            int interval = 3)
            where T : IComparable<T> {

            if (data is null) {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Count == 0)
                throw new ArgumentException($"“{nameof(data)}”中项目数应大于0。");

            if (interval < 3) interval = 3;
            if (interval % 2 == 0) interval++;

            var result = new List<int>();
            var midIndex = interval / 2;

            if (data.Count == 1) {
                result.Add(0);
                return result;
            }

            //          ...
            //       +      -
            //     +         -
            //    =           =
            //  ??             ??
            //  扩展变化值中与端部紧邻的值以端部为中心对称取反；若相等则扩大一位取反；
            //  其余扩展变化值均等于此侧与端部紧邻的值
            //  示例中两端分别为 --, ++
            var variations = new List<int>(new int[data.Count + interval - 2]);
            for (int i = 0; i < data.Count - 1; i++) {
                variations[i + midIndex] = data[i + 1].CompareTo(data[i]);
            }
            if (variations[midIndex] == 0) {
                variations[midIndex - 1] = -1 * variations[midIndex + 1];
            } else {
                variations[midIndex - 1] = -1 * variations[midIndex];
            }
            if (variations[data.Count + midIndex - 2] == 0) {
                variations[data.Count + midIndex - 1] = -1 * variations[data.Count + midIndex - 3];
            } else {
                variations[data.Count + midIndex - 1] = -1 * variations[data.Count + midIndex - 2];
            }
            for (int i = 0; i < midIndex - 1; i++) {
                variations[midIndex - 2 - i] = variations[midIndex - 1 - i];
                variations[data.Count + midIndex + i] = variations[data.Count + midIndex - 1 + i];
            }


            var subVariations = new List<int>(new int[interval - 1]);
            for (int i = 0; i < data.Count; i++) {
                for (int j = 0; j < interval - 1; j++) {
                    subVariations[j] = variations[i + j];
                }

                if (MaybeExtreme(subVariations, type)) {
                    result.Add(i);
                    i += midIndex;//  成功找到极值点跳转到当前局部区间右侧下一个值
                }
            }

            return result;
        }
        /// <summary>
        /// 判断变化集合 <paramref name="variations"/> 是否符合极值分布。
        /// </summary>
        /// <remarks>是 <see cref="GetLocalExtremeIndexes{T}(IList{T}, ExtremeTypeEnum, int)"/> 的配套方法。
        /// <paramref name="variations"/> 元素数量必定为大于等于 2 的偶数。</remarks>
        /// <param name="variations">给定数据集合。</param>
        /// <param name="type">极值类型。</param>
        /// <returns>是否符合极值类型 <paramref name="type"/> 。</returns>
        /// <exception cref="ArgumentNullException"></exception>

        //                                            +         
        //                          +               +   -       
        //                        +   -           +       -     
        //              =       +       -       +               
        //            +   -   +           -   +                 
        //          +       -               =                   
        //        +                                             
        private static bool MaybeExtreme(List<int> variations, ExtremeTypeEnum type) {
            if (variations is null) {
                throw new ArgumentNullException(nameof(variations));
            }

            int cnt = variations.Count;
            int half = cnt / 2;

            //  左右只能有一个值相等
            if (variations[half - 1] == 0 && variations[half] == 0) return false;

            switch (type) {
                case ExtremeTypeEnum.LocalMinimum:
                    for (int i = 0; i < half; i++) {
                        //  左边不允许正值,右边不允许负值
                        if (variations[i] > 0 || variations[cnt - 1 - i] < 0) return false;
                        //  只能中间两个值有一个值相等，其余值不允许相等
                        if ((variations[i] == 0 || variations[cnt - 1 - i] == 0) && i != half - 1) return false;
                    }
                    break;
                case ExtremeTypeEnum.LocalMaximum:
                    for (int i = 0; i < half; i++) {
                        //  左边不允许负值,右边不允许正值
                        if (variations[i] < 0 || variations[cnt - 1 - i] > 0) return false;
                        //  只能中间两个值有一个值相等，其余值不允许相等
                        if ((variations[i] == 0 || variations[cnt - 1 - i] == 0) && i != half - 1) return false;
                    }
                    break;
                default:

                    break;
            }

            return true;
        }
    }
}

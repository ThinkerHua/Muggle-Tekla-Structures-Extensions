/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2024 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  ColorExtension.cs: extension of Tekla.Structures.Model.UI.Color
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.Common.ModelUI {
    /// <summary>
    /// <see cref="Tekla.Structures.Model.UI"/>.<see cref="Color"/> 的扩展。
    /// </summary>
    /// <remarks>参见"<a href="https://github.com/ThinkerHua/Muggle-Tekla-Plugins/blob/master/Documents/%E9%A2%9C%E8%89%B2%E7%A0%81%E5%AF%B9%E7%85%A7%E8%A1%A8.png">颜色码对照表</a>"。
    /// 表中RGB值是通常定义的 0~255 整数，但官方文档定义取值范围为 0.0~1.0，所以代码中的实际值做了转换，保留4位小数。</remarks>
    public static class ColorExtension {
        #region 预设属性
        /// <summary>
        /// 纯黑
        /// </summary>
        /// <value>(0, 0, 0)</value>
        public static Color Black => new Color(0.0000, 0.0000, 0.0000);
        /// <summary>
        /// 海军蓝
        /// </summary>
        /// <value>(0, 0, 128)</value>
        public static Color Navy => new Color(0.0000, 0.0000, 0.5020);
        /// <summary>
        /// 深蓝色
        /// </summary>
        /// <value>(0, 0, 139)</value>
        public static Color DarkBlue => new Color(0.0000, 0.0000, 0.5451);
        /// <summary>
        /// 适中的蓝色
        /// </summary>
        /// <value>(0, 0, 205)</value>
        public static Color MediumBlue => new Color(0.0000, 0.0000, 0.8039);
        /// <summary>
        /// 纯蓝
        /// </summary>
        /// <value>(0, 0, 255)</value>
        public static Color Blue => new Color(0.0000, 0.0000, 1.0000);
        /// <summary>
        /// 深绿色
        /// </summary>
        /// <value>(0, 100, 0)</value>
        public static Color DarkGreen => new Color(0.0000, 0.3922, 0.0000);
        /// <summary>
        /// 纯绿
        /// </summary>
        /// <value>(0, 128, 0)</value>
        public static Color Green => new Color(0.0000, 0.5020, 0.0000);
        /// <summary>
        /// 水鸭色
        /// </summary>
        /// <value>(0, 128, 128)</value>
        public static Color Teal => new Color(0.0000, 0.5020, 0.5020);
        /// <summary>
        /// 深青色
        /// </summary>
        /// <value>(0, 139, 139)</value>
        public static Color DarkCyan => new Color(0.0000, 0.5451, 0.5451);
        /// <summary>
        /// 深天蓝
        /// </summary>
        /// <value>(0, 191, 255)</value>
        public static Color DeepSkyBlue => new Color(0.0000, 0.7490, 1.0000);
        /// <summary>
        /// 深绿宝石
        /// </summary>
        /// <value>(0, 206, 209)</value>
        public static Color DarkTurquoise => new Color(0.0000, 0.8078, 0.8196);
        /// <summary>
        /// 适中的碧绿色
        /// </summary>
        /// <value>(0, 250, 154)</value>
        public static Color MediumAquamarine => new Color(0.0000, 0.9804, 0.6039);
        /// <summary>
        /// 酸橙色
        /// </summary>
        /// <value>(0, 255, 0)</value>
        public static Color Lime => new Color(0.0000, 1.0000, 0.0000);
        /// <summary>
        /// 适中的春天的绿色
        /// </summary>
        /// <value>(0, 255, 127)</value>
        public static Color MediumSpringGreen => new Color(0.0000, 1.0000, 0.4980);
        /// <summary>
        /// 青色
        /// </summary>
        /// <value>(0, 255, 255)</value>
        public static Color Cyan => new Color(0.0000, 1.0000, 1.0000);
        /// <summary>
        /// 午夜的蓝色
        /// </summary>
        /// <value>(25, 25, 112)</value>
        public static Color MidnightBlue => new Color(0.0980, 0.0980, 0.4392);
        /// <summary>
        /// 道奇蓝
        /// </summary>
        /// <value>(30, 144, 255)</value>
        public static Color DoderBlue => new Color(0.1176, 0.5647, 1.0000);
        /// <summary>
        /// 浅海洋绿
        /// </summary>
        /// <value>(32, 178, 170)</value>
        public static Color LightSeaGreen => new Color(0.1255, 0.6980, 0.6667);
        /// <summary>
        /// 森林绿
        /// </summary>
        /// <value>(34, 139, 34)</value>
        public static Color ForestGreen => new Color(0.1333, 0.5451, 0.1333);
        /// <summary>
        /// 海洋绿
        /// </summary>
        /// <value>(46, 139, 87)</value>
        public static Color SeaGreen => new Color(0.1804, 0.5451, 0.3412);
        /// <summary>
        /// 深石板灰
        /// </summary>
        /// <value>(47, 79, 79)</value>
        public static Color DarkSlateGray => new Color(0.1843, 0.3098, 0.3098);
        /// <summary>
        /// 酸橙绿
        /// </summary>
        /// <value>(50, 205, 50)</value>
        public static Color LimeGreen => new Color(0.1961, 0.8039, 0.1961);
        /// <summary>
        /// 春天的绿色
        /// </summary>
        /// <value>(60, 179, 113)</value>
        public static Color SpringGreen => new Color(0.2353, 0.7020, 0.4431);
        /// <summary>
        /// 绿宝石
        /// </summary>
        /// <value>(64, 224, 208)</value>
        public static Color Turquoise => new Color(0.2510, 0.8784, 0.8157);
        /// <summary>
        /// 皇家蓝
        /// </summary>
        /// <value>(65, 105, 225)</value>
        public static Color RoyalBlue => new Color(0.2549, 0.4118, 0.8824);
        /// <summary>
        /// 钢蓝
        /// </summary>
        /// <value>(70, 130, 180)</value>
        public static Color SteelBlue => new Color(0.2745, 0.5098, 0.7059);
        /// <summary>
        /// 深岩暗蓝灰色
        /// </summary>
        /// <value>(72, 61, 139)</value>
        public static Color DarkSlateBlue => new Color(0.2824, 0.2392, 0.5451);
        /// <summary>
        /// 适中的绿宝石
        /// </summary>
        /// <value>(72, 209, 204)</value>
        public static Color MediumTurquoise => new Color(0.2824, 0.8196, 0.8000);
        /// <summary>
        /// 靛青
        /// </summary>
        /// <value>(75, 0, 130)</value>
        public static Color Indigo => new Color(0.2941, 0.0000, 0.5098);
        /// <summary>
        /// 橄榄土褐色
        /// </summary>
        /// <value>(85, 107, 47)</value>
        public static Color OliveDrab => new Color(0.3333, 0.4196, 0.1843);
        /// <summary>
        /// 军校蓝
        /// </summary>
        /// <value>(95, 158, 160)</value>
        public static Color CadetBlue => new Color(0.3725, 0.6196, 0.6275);
        /// <summary>
        /// 矢车菊的蓝色
        /// </summary>
        /// <value>(100, 149, 237)</value>
        public static Color CornflowerBlue => new Color(0.3922, 0.5843, 0.9294);
        /// <summary>
        /// 暗淡的灰色
        /// </summary>
        /// <value>(105, 105, 105)</value>
        public static Color DimGray => new Color(0.4118, 0.4118, 0.4118);
        /// <summary>
        /// 板岩暗蓝灰色
        /// </summary>
        /// <value>(106, 90, 205)</value>
        public static Color SlateBlue => new Color(0.4157, 0.3529, 0.8039);
        /// <summary>
        /// 石板灰
        /// </summary>
        /// <value>(112, 128, 144)</value>
        public static Color SlateGray => new Color(0.4392, 0.5020, 0.5647);
        /// <summary>
        /// 浅石板灰
        /// </summary>
        /// <value>(119, 136, 153)</value>
        public static Color LightSlateGray => new Color(0.4667, 0.5333, 0.6000);
        /// <summary>
        /// 适中的板岩暗蓝灰色
        /// </summary>
        /// <value>(123, 104, 238)</value>
        public static Color MediumSlateBlue => new Color(0.4824, 0.4078, 0.9333);
        /// <summary>
        /// 草坪绿
        /// </summary>
        /// <value>(124, 252, 0)</value>
        public static Color LawnGreen => new Color(0.4863, 0.9882, 0.0000);
        /// <summary>
        /// 查特酒绿
        /// </summary>
        /// <value>(127, 255, 0)</value>
        public static Color Chartreuse => new Color(0.4980, 1.0000, 0.0000);
        /// <summary>
        /// 绿玉\碧绿色
        /// </summary>
        /// <value>(127, 255, 170)</value>
        public static Color Auqamarin => new Color(0.4980, 1.0000, 0.6667);
        /// <summary>
        /// 栗色
        /// </summary>
        /// <value>(128, 0, 0)</value>
        public static Color Maroon => new Color(0.5020, 0.0000, 0.0000);
        /// <summary>
        /// 紫色
        /// </summary>
        /// <value>(128, 0, 128)</value>
        public static Color Purple => new Color(0.5020, 0.0000, 0.5020);
        /// <summary>
        /// 橄榄
        /// </summary>
        /// <value>(128, 128, 0)</value>
        public static Color Olive => new Color(0.5020, 0.5020, 0.0000);
        /// <summary>
        /// 灰色
        /// </summary>
        /// <value>(128, 128, 128)</value>
        public static Color Gray => new Color(0.5020, 0.5020, 0.5020);
        /// <summary>
        /// 天蓝色
        /// </summary>
        /// <value>(135, 206, 235)</value>
        public static Color SkyBlue => new Color(0.5294, 0.8078, 0.9216);
        /// <summary>
        /// 淡蓝色
        /// </summary>
        /// <value>(135, 206, 250)</value>
        public static Color LightSkyBlue => new Color(0.5294, 0.8078, 0.9804);
        /// <summary>
        /// 深紫罗兰的蓝色
        /// </summary>
        /// <value>(138, 43, 226)</value>
        public static Color BlueViolet => new Color(0.5412, 0.1686, 0.8863);
        /// <summary>
        /// 深红色
        /// </summary>
        /// <value>(139, 0, 0)</value>
        public static Color DarkRed => new Color(0.5451, 0.0000, 0.0000);
        /// <summary>
        /// 深洋红色
        /// </summary>
        /// <value>(139, 0, 139)</value>
        public static Color DarkMagenta => new Color(0.5451, 0.0000, 0.5451);
        /// <summary>
        /// 马鞍棕色
        /// </summary>
        /// <value>(139, 69, 19)</value>
        public static Color SaddleBrown => new Color(0.5451, 0.2706, 0.0745);
        /// <summary>
        /// 深海洋绿
        /// </summary>
        /// <value>(143, 188, 143)</value>
        public static Color DarkSeaGreen => new Color(0.5608, 0.7373, 0.5608);
        /// <summary>
        /// 淡绿色
        /// </summary>
        /// <value>(144, 238, 144)</value>
        public static Color LightGreen => new Color(0.5647, 0.9333, 0.5647);
        /// <summary>
        /// 适中的紫色
        /// </summary>
        /// <value>(147, 112, 219)</value>
        public static Color MediumPurple => new Color(0.5765, 0.4392, 0.8588);
        /// <summary>
        /// 深紫罗兰色
        /// </summary>
        /// <value>(148, 0, 211)</value>
        public static Color DarkVoilet => new Color(0.5804, 0.0000, 0.8275);
        /// <summary>
        /// 苍白的绿色
        /// </summary>
        /// <value>(152, 251, 152)</value>
        public static Color PaleGreen => new Color(0.5961, 0.9843, 0.5961);
        /// <summary>
        /// 深兰花紫
        /// </summary>
        /// <value>(153, 50, 204)</value>
        public static Color DarkOrchid => new Color(0.6000, 0.1961, 0.8000);
        /// <summary>
        /// 黄土赭色
        /// </summary>
        /// <value>(160, 82, 45)</value>
        public static Color Sienna => new Color(0.6275, 0.3216, 0.1765);
        /// <summary>
        /// 棕色
        /// </summary>
        /// <value>(165, 42, 42)</value>
        public static Color Brown => new Color(0.6471, 0.1647, 0.1647);
        /// <summary>
        /// 深灰色
        /// </summary>
        /// <value>(169, 169, 169)</value>
        public static Color DarkGray => new Color(0.6627, 0.6627, 0.6627);
        /// <summary>
        /// 淡蓝
        /// </summary>
        /// <value>(173, 216, 230)</value>
        public static Color LightBLue => new Color(0.6784, 0.8471, 0.9020);
        /// <summary>
        /// 绿黄色
        /// </summary>
        /// <value>(173, 255, 47)</value>
        public static Color GreenYellow => new Color(0.6784, 1.0000, 0.1843);
        /// <summary>
        /// 苍白的绿宝石
        /// </summary>
        /// <value>(175, 238, 238)</value>
        public static Color PaleTurquoise => new Color(0.6863, 0.9333, 0.9333);
        /// <summary>
        /// 淡钢蓝
        /// </summary>
        /// <value>(176, 196, 222)</value>
        public static Color LightSteelBlue => new Color(0.6902, 0.7686, 0.8706);
        /// <summary>
        /// 火药蓝
        /// </summary>
        /// <value>(176, 224, 230)</value>
        public static Color PowDerBlue => new Color(0.6902, 0.8784, 0.9020);
        /// <summary>
        /// 耐火砖
        /// </summary>
        /// <value>(178, 34, 34)</value>
        public static Color FireBrick => new Color(0.6980, 0.1333, 0.1333);
        /// <summary>
        /// 适中的兰花紫
        /// </summary>
        /// <value>(186, 85, 211)</value>
        public static Color MediumOrchid => new Color(0.7294, 0.3333, 0.8275);
        /// <summary>
        /// 玫瑰棕色
        /// </summary>
        /// <value>(188, 143, 143)</value>
        public static Color RosyBrown => new Color(0.7373, 0.5608, 0.5608);
        /// <summary>
        /// 深卡其布
        /// </summary>
        /// <value>(189, 183, 107)</value>
        public static Color DarkKhaki => new Color(0.7412, 0.7176, 0.4196);
        /// <summary>
        /// 银白色
        /// </summary>
        /// <value>(192, 192, 192)</value>
        public static Color Silver => new Color(0.7529, 0.7529, 0.7529);
        /// <summary>
        /// 适中的紫罗兰红色
        /// </summary>
        /// <value>(199, 21, 133)</value>
        public static Color MediumVioletRed => new Color(0.7804, 0.0824, 0.5216);
        /// <summary>
        /// 印度红
        /// </summary>
        /// <value>(205, 92, 92)</value>
        public static Color IndianRed => new Color(0.8039, 0.3608, 0.3608);
        /// <summary>
        /// 秘鲁
        /// </summary>
        /// <value>(205, 133, 63)</value>
        public static Color Peru => new Color(0.8039, 0.5216, 0.2471);
        /// <summary>
        /// 巧克力
        /// </summary>
        /// <value>(210, 105, 30)</value>
        public static Color Chocolate => new Color(0.8235, 0.4118, 0.1176);
        /// <summary>
        /// 晒黑
        /// </summary>
        /// <value>(210, 180, 140)</value>
        public static Color Tan => new Color(0.8235, 0.7059, 0.5490);
        /// <summary>
        /// 浅灰色
        /// </summary>
        /// <value>(211, 211, 211)</value>
        public static Color LightGrey => new Color(0.8275, 0.8275, 0.8275);
        /// <summary>
        /// 水绿色
        /// </summary>
        /// <value>(212, 242, 231)</value>
        public static Color Aqua => new Color(0.8314, 0.9490, 0.9059);
        /// <summary>
        /// 蓟
        /// </summary>
        /// <value>(216, 191, 216)</value>
        public static Color Thistle => new Color(0.8471, 0.7490, 0.8471);
        /// <summary>
        /// 兰花的紫色
        /// </summary>
        /// <value>(218, 112, 214)</value>
        public static Color Orchid => new Color(0.8549, 0.4392, 0.8392);
        /// <summary>
        /// 秋麒麟
        /// </summary>
        /// <value>(218, 165, 32)</value>
        public static Color GoldEnrod => new Color(0.8549, 0.6471, 0.1255);
        /// <summary>
        /// 苍白的紫罗兰红色
        /// </summary>
        /// <value>(219, 112, 147)</value>
        public static Color PaleVioletRed => new Color(0.8588, 0.4392, 0.5765);
        /// <summary>
        /// 猩红
        /// </summary>
        /// <value>(220, 20, 60)</value>
        public static Color Crimson => new Color(0.8627, 0.0784, 0.2353);
        /// <summary>
        /// 亮灰色
        /// </summary>
        /// <value>(220, 220, 220)</value>
        public static Color Gainsboro => new Color(0.8627, 0.8627, 0.8627);
        /// <summary>
        /// 李子
        /// </summary>
        /// <value>(221, 160, 221)</value>
        public static Color Plum => new Color(0.8667, 0.6275, 0.8667);
        /// <summary>
        /// 结实的树
        /// </summary>
        /// <value>(222, 184, 135)</value>
        public static Color BrulyWood => new Color(0.8706, 0.7216, 0.5294);
        /// <summary>
        /// 淡青色
        /// </summary>
        /// <value>(225, 255, 255)</value>
        public static Color LightCyan => new Color(0.8824, 1.0000, 1.0000);
        /// <summary>
        /// 熏衣草花的淡紫色
        /// </summary>
        /// <value>(230, 230, 250)</value>
        public static Color Lavender => new Color(0.9020, 0.9020, 0.9804);
        /// <summary>
        /// 深鲜肉(鲑鱼)色
        /// </summary>
        /// <value>(233, 150, 122)</value>
        public static Color DarkSalmon => new Color(0.9137, 0.5882, 0.4784);
        /// <summary>
        /// 紫罗兰
        /// </summary>
        /// <value>(238, 130, 238)</value>
        public static Color Violet => new Color(0.9333, 0.5098, 0.9333);
        /// <summary>
        /// 灰秋麒麟
        /// </summary>
        /// <value>(238, 232, 170)</value>
        public static Color PaleGodenrod => new Color(0.9333, 0.9098, 0.6667);
        /// <summary>
        /// 淡珊瑚色
        /// </summary>
        /// <value>(240, 128, 128)</value>
        public static Color LightCoral => new Color(0.9412, 0.5020, 0.5020);
        /// <summary>
        /// 卡其布
        /// </summary>
        /// <value>(240, 230, 140)</value>
        public static Color Khaki => new Color(0.9412, 0.9020, 0.5490);
        /// <summary>
        /// 爱丽丝蓝
        /// </summary>
        /// <value>(240, 248, 255)</value>
        public static Color AliceBlue => new Color(0.9412, 0.9725, 1.0000);
        /// <summary>
        /// 蜂蜜
        /// </summary>
        /// <value>(240, 255, 240)</value>
        public static Color Honeydew => new Color(0.9412, 1.0000, 0.9412);
        /// <summary>
        /// 蔚蓝色
        /// </summary>
        /// <value>(240, 255, 255)</value>
        public static Color Azure => new Color(0.9412, 1.0000, 1.0000);
        /// <summary>
        /// 沙棕色
        /// </summary>
        /// <value>(244, 164, 96)</value>
        public static Color SandyBrown => new Color(0.9569, 0.6431, 0.3765);
        /// <summary>
        /// 小麦色
        /// </summary>
        /// <value>(245, 222, 179)</value>
        public static Color Wheat => new Color(0.9608, 0.8706, 0.7020);
        /// <summary>
        /// 米色(浅褐色)
        /// </summary>
        /// <value>(245, 245, 220)</value>
        public static Color Beige => new Color(0.9608, 0.9608, 0.8627);
        /// <summary>
        /// 白烟
        /// </summary>
        /// <value>(245, 245, 245)</value>
        public static Color WhiteSmoke => new Color(0.9608, 0.9608, 0.9608);
        /// <summary>
        /// 薄荷奶油
        /// </summary>
        /// <value>(245, 255, 250)</value>
        public static Color MintCream => new Color(0.9608, 1.0000, 0.9804);
        /// <summary>
        /// 幽灵的白色
        /// </summary>
        /// <value>(248, 248, 255)</value>
        public static Color GhostWhite => new Color(0.9725, 0.9725, 1.0000);
        /// <summary>
        /// 鲜肉(鲑鱼)色
        /// </summary>
        /// <value>(250, 128, 114)</value>
        public static Color Salmon => new Color(0.9804, 0.5020, 0.4471);
        /// <summary>
        /// 古代的白色
        /// </summary>
        /// <value>(250, 235, 215)</value>
        public static Color AntiqueWhite => new Color(0.9804, 0.9216, 0.8431);
        /// <summary>
        /// 亚麻布
        /// </summary>
        /// <value>(250, 240, 230)</value>
        public static Color Linen => new Color(0.9804, 0.9412, 0.9020);
        /// <summary>
        /// 浅秋麒麟黄
        /// </summary>
        /// <value>(250, 250, 210)</value>
        public static Color LightGoldenrodYellow => new Color(0.9804, 0.9804, 0.8235);
        /// <summary>
        /// 老饰带
        /// </summary>
        /// <value>(253, 245, 230)</value>
        public static Color OldLace => new Color(0.9922, 0.9608, 0.9020);
        /// <summary>
        /// 纯红
        /// </summary>
        /// <value>(255, 0, 0)</value>
        public static Color Red => new Color(1.0000, 0.0000, 0.0000);
        /// <summary>
        /// 洋红
        /// </summary>
        /// <value>(255, 0, 255)</value>
        public static Color Magenta => new Color(1.0000, 0.0000, 1.0000);
        /// <summary>
        /// 灯笼海棠(紫红色)
        /// </summary>
        /// <value>(255, 0, 255)</value>
        public static Color Fuchsia => new Color(1.0000, 0.0000, 1.0000);
        /// <summary>
        /// 深粉色
        /// </summary>
        /// <value>(255, 20, 147)</value>
        public static Color DeepPink => new Color(1.0000, 0.0784, 0.5765);
        /// <summary>
        /// 橙红色
        /// </summary>
        /// <value>(255, 69, 0)</value>
        public static Color OrangeRed => new Color(1.0000, 0.2706, 0.0000);
        /// <summary>
        /// 番茄
        /// </summary>
        /// <value>(255, 99, 71)</value>
        public static Color Tomato => new Color(1.0000, 0.3882, 0.2784);
        /// <summary>
        /// 热情的粉红
        /// </summary>
        /// <value>(255, 105, 180)</value>
        public static Color HotPink => new Color(1.0000, 0.4118, 0.7059);
        /// <summary>
        /// 珊瑚
        /// </summary>
        /// <value>(255, 127, 80)</value>
        public static Color Coral => new Color(1.0000, 0.4980, 0.3137);
        /// <summary>
        /// 深橙色
        /// </summary>
        /// <value>(255, 140, 0)</value>
        public static Color DarkOrange => new Color(1.0000, 0.5490, 0.0000);
        /// <summary>
        /// 浅鲜肉(鲑鱼)色
        /// </summary>
        /// <value>(255, 160, 122)</value>
        public static Color LightSalmon => new Color(1.0000, 0.6275, 0.4784);
        /// <summary>
        /// 橙色
        /// </summary>
        /// <value>(255, 165, 0)</value>
        public static Color Orange => new Color(1.0000, 0.6471, 0.0000);
        /// <summary>
        /// 浅粉红
        /// </summary>
        /// <value>(255, 182, 193)</value>
        public static Color LightPink => new Color(1.0000, 0.7137, 0.7569);
        /// <summary>
        /// 粉红
        /// </summary>
        /// <value>(255, 192, 203)</value>
        public static Color Pink => new Color(1.0000, 0.7529, 0.7961);
        /// <summary>
        /// 金
        /// </summary>
        /// <value>(255, 215, 0)</value>
        public static Color Gold => new Color(1.0000, 0.8431, 0.0000);
        /// <summary>
        /// 桃色
        /// </summary>
        /// <value>(255, 218, 185)</value>
        public static Color PeachPuff => new Color(1.0000, 0.8549, 0.7255);
        /// <summary>
        /// 纳瓦霍白
        /// </summary>
        /// <value>(255, 222, 173)</value>
        public static Color NavajoWhite => new Color(1.0000, 0.8706, 0.6784);
        /// <summary>
        /// 鹿皮鞋
        /// </summary>
        /// <value>(255, 228, 181)</value>
        public static Color Moccasin => new Color(1.0000, 0.8941, 0.7098);
        /// <summary>
        /// (浓汤)乳脂,番茄等
        /// </summary>
        /// <value>(255, 228, 196)</value>
        public static Color Bisque => new Color(1.0000, 0.8941, 0.7686);
        /// <summary>
        /// 薄雾玫瑰
        /// </summary>
        /// <value>(255, 228, 225)</value>
        public static Color MistyRose => new Color(1.0000, 0.8941, 0.8824);
        /// <summary>
        /// 漂白的杏仁
        /// </summary>
        /// <value>(255, 235, 205)</value>
        public static Color BlanchedAlmond => new Color(1.0000, 0.9216, 0.8039);
        /// <summary>
        /// 番木瓜
        /// </summary>
        /// <value>(255, 239, 213)</value>
        public static Color PapayaWhip => new Color(1.0000, 0.9373, 0.8353);
        /// <summary>
        /// 脸红的淡紫色
        /// </summary>
        /// <value>(255, 240, 245)</value>
        public static Color LavenderBlush => new Color(1.0000, 0.9412, 0.9608);
        /// <summary>
        /// 海贝壳
        /// </summary>
        /// <value>(255, 245, 238)</value>
        public static Color SeaShell => new Color(1.0000, 0.9608, 0.9333);
        /// <summary>
        /// 玉米色
        /// </summary>
        /// <value>(255, 248, 220)</value>
        public static Color Cornislk => new Color(1.0000, 0.9725, 0.8627);
        /// <summary>
        /// 柠檬薄纱
        /// </summary>
        /// <value>(255, 250, 205)</value>
        public static Color LemonChiffon => new Color(1.0000, 0.9804, 0.8039);
        /// <summary>
        /// 花的白色
        /// </summary>
        /// <value>(255, 250, 240)</value>
        public static Color FloralWhite => new Color(1.0000, 0.9804, 0.9412);
        /// <summary>
        /// 雪
        /// </summary>
        /// <value>(255, 250, 250)</value>
        public static Color Snow => new Color(1.0000, 0.9804, 0.9804);
        /// <summary>
        /// 纯黄
        /// </summary>
        /// <value>(255, 255, 0)</value>
        public static Color Yellow => new Color(1.0000, 1.0000, 0.0000);
        /// <summary>
        /// 浅黄色
        /// </summary>
        /// <value>(255, 255, 224)</value>
        public static Color LightYellow => new Color(1.0000, 1.0000, 0.8784);
        /// <summary>
        /// 象牙
        /// </summary>
        /// <value>(255, 255, 240)</value>
        public static Color Ivory => new Color(1.0000, 1.0000, 0.9412);
        /// <summary>
        /// 纯白
        /// </summary>
        /// <value>(255, 255, 255)</value>
        public static Color White => new Color(1.0000, 1.0000, 1.0000);
        #endregion
        /// <summary>
        /// 获取颜色的字符串表示形式。
        /// </summary>
        /// <remarks>有关数字格式字符串的详细信息，请参阅
        /// <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/standard-numeric-format-strings">标准数字格式字符串</a>
        /// 和 <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-numeric-format-strings">自定义数字格式字符串</a>。
        /// </remarks>
        /// <param name="color">当前颜色</param>
        /// <param name="format">复合格式字符串。默认值 null。</param>
        /// <returns>颜色的字符串表示形式。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToString(this Color color, string format = default) {
            if (color is null) {
                throw new ArgumentNullException(nameof(color));
            }

            return $"({color.Red.ToString(format)}, " +
                $"{color.Green.ToString(format)}, " +
                $"{color.Blue.ToString(format)}, " +
                $"{color.Transparency.ToString(format)})";
        }
        /// <summary>
        /// 用给定颜色对当前颜色进行偏移。
        /// </summary>
        /// <param name="color">当前颜色</param>
        /// <param name="shifter">给定颜色</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Shift(this Color color, Color shifter) {
            if (color is null) {
                throw new ArgumentNullException(nameof(color));
            }

            if (shifter is null) {
                throw new ArgumentNullException(nameof(shifter));
            }

            double value;
            value = color.Red + shifter.Red;
            color.Red = value > 1.0 ? value - 1.0 : value;
            value = color.Green + shifter.Green;
            color.Green = value > 1.0 ? value - 1.0 : value;
            value = color.Blue + shifter.Blue;
            color.Blue = value > 1.0 ? value - 1.0 : value;
            value = color.Transparency + shifter.Transparency;
            color.Transparency = value > 1.0 ? value - 1.0 : value;
        }
        /// <summary>
        /// 获取用给定颜色对当前颜色进行偏移后的新颜色。
        /// </summary>
        /// <param name="color">当前颜色</param>
        /// <param name="shifter">给定颜色</param>
        /// <returns>偏移后的新颜色。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Color GetShifted(this Color color, Color shifter) {
            if (color is null) {
                throw new ArgumentNullException(nameof(color));
            }

            if (shifter is null) {
                throw new ArgumentNullException(nameof(shifter));
            }

            var newColor = new Color(color.Red, color.Green, color.Blue, color.Transparency);
            newColor.Shift(shifter);

            return newColor;
        }
    }
}

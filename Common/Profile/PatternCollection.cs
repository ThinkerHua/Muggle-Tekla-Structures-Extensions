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
 *  PatternCollection.cs: patterns of profile's text
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 匹配模式集合
    /// </summary>
    public static class PatternCollection {
        /// <summary>
        /// 前置标识符为 H 或 HP 或 HW 或 HM 或 HN 或 HT 或 WH，后续参数形式为 h1[~h2]*b1[/b2]*s*t1[/t2]。
        /// </summary>
        public static string H_1 => @"^((H[PWMNT]?)|WH)(?<h1>\d+\.?\d*)(~(?<h2>\d+\.?\d*))?"
                                    + @"\*(?<b1>\d+\.?\d*)(/(?<b2>\d+\.?\d*))?"
                                    + @"\*(?<s>\d+\.?\d*)\*(?<t1>\d+\.?\d*)(/(?<t2>\d+\.?\d*))?$";
        /// <summary>
        /// 前置标识符为 HI 或 PHI 或 WI，后续参数形式为 h1[-h2]-s-t1*b1[-t2*b2]。
        /// </summary>
        public static string H_2 => @"^((P?H)|W)I(?<h1>\d+\.?\d*)(-(?<h2>\d+\.?\d*))?-(?<s>\d+\.?\d*)"
                                    + @"-(?<t1>\d+\.?\d*)\*(?<b1>\d+\.?\d*)(-(?<t2>\d+\.?\d*)\*(?<b2>\d+\.?\d*))?$";
        /// <summary>
        /// 前置标识符为 H 或 HW 或 HM 或 HN 或 HT，用"TYPE"分组接收，后续参数形式为 H*B，仅支持整数。
        /// </summary>
        public static string H_3 => @"^(?<TYPE>H([WMN]?|T))(?<H>\d+)\*(?<B>\d+)$";
        /// <summary>
        /// 前置标识符为 B_WLD_K，后续参数形式为 h1*h2*b1*s*t1。
        /// </summary>
        public static string H_4 => @"^B_WLD_K(?<h1>\d+\.?\d*)\*(?<h2>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t1>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 B_WLD_A，后续参数形式为 h1*b1*s*t1。
        /// </summary>
        public static string H_5 => @"^B_WLD_A(?<h1>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t1>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 B_WLD_H，后续参数形式为 h1*b1*b2*s*t1*t2。
        /// </summary>
        public static string H_6 => @"^B_WLD_H(?<h1>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<b2>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t1>\d+\.?\d*)\*(?<t2>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 I_VAR_A，后续参数形式为 h1-h2*b1-b2*s*t1。
        /// </summary>
        public static string H_7 => @"^I_VAR_A(?<h1>\d+\.?\d*)-(?<h2>\d+\.?\d*)\*(?<b1>\d+\.?\d*)-(?<b2>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t1>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 PL，后续参数形式为 t*b*l。
        /// </summary>
        public static string PL_1 => @"\APL(?<t>\d+\.?\d*)(\*(?<b>\d+\.?\d*))?(\*(?<l>\d+\.?\d*))?\Z";
        /// <summary>
        /// 前置标识符为 φ 或 D 或 ELD 或 ROD，后续参数形式为 d1。
        /// </summary>
        public static string CIRC_1 => @"^(φ|D|(ELD)|(ROD))(?<d1>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 PIP 或 CFCHS 或 CHS 或 EPD 或 O 或 PD 或 TUBE，后续参数形式为 d1*t。
        /// </summary>
        public static string CIRC_2 => @"^((PIP)|(CFCHS)|(CHS)|(EPD)|(O)|(PD)|(TUBE))(?<d1>\d+\.?\d*)\*(?<t>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 CFCHS 或 CHS 或 EPD 或 O 或 PD 或 TUBE，后续参数形式为 d1*d2*t。
        /// </summary>
        public static string CIRC_3 => @"^((CFCHS)|(CHS)|(EPD)|O|(PD)|(TUBE))(?<d1>\d+\.?\d*)\*(?<d2>\d+\.?\d*)\*(?<t>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 D 或 ELD 或 ROD，后续参数形式为 d1*r1*d2*r2。
        /// </summary>
        public static string CIRC_4 => @"^((EL)|(RO))?D(?<d1>\d+\.?\d*)\*(?<r1>\d+\.?\d*)\*(?<d2>\d+\.?\d*)\*(?<r2>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 CFCHS 或 CHS 或 EPD 或 O 或 PD 或 TUBE，后续参数形式为 d1*r1*d2*r2*t。
        /// </summary>
        public static string CIRC_5 => @"^((CFCHS)|(CHS)|(EPD)|O|(PD)|(TUBE))(?<d1>\d+\.?\d*)\*(?<r1>\d+\.?\d*)"
                                        + @"\*(?<d2>\d+\.?\d*)\*(?<r2>\d+\.?\d*)\*(?<t>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 P 或 TUB 或 RHS 或 SHS 或 CFRHS，后续参数形式为 h1*s。
        /// </summary>
        public static string CFH_J_1 => @"^(P|(TUB)|(RHS)|(SHS)|(CFRHS))(?<h1>\d+\.?\d*)\*(?<s>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 P 或 RHS 或 SHS 或 CFRHS，后续参数形式为 h1*b1*s。
        /// </summary>
        public static string CFH_J_2 => @"^(P|(RHS)|(SHS)|(CFRHS))(?<h1>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<s>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 P 或 RHS 或 SHS 或 CFRHS，后续参数形式为 h1*b1-h2*b2*s。
        /// </summary>
        public static string CFH_J_3 => @"^(P|(RHS)|(SHS)|(CFRHS))(?<h1>\d+\.?\d*)\*(?<b1>\d+\.?\d*)-(?<h2>\d+\.?\d*)\*(?<b2>\d+\.?\d*)\*(?<s>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 F，后续参数形式为 h1Xb1Xs。
        /// </summary>
        public static string CFH_J_4 => @"^F(?<h1>\d+\.?\d*)X(?<b1>\d+\.?\d*)X(?<s>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 B_WLD_F，后续参数形式为 h1*b1*s。
        /// </summary>
        public static string RECT_1 => @"^B_WLD_F(?<h1>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<s>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 B_WLD_F 或 B_BUILT，后续参数形式为 h1*b1*s*t。
        /// </summary>
        public static string RECT_2 => @"^((B_WLD_F)|(B_BUILT))(?<h1>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 R，后续参数形式为 h1~h2*b1*s*t。
        /// </summary>
        public static string RECT_3 => @"^R(?<h1>\d+\.?\d*)~(?<h2>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 B_WLD_J，后续参数形式为 h1*h2*b1*s*t。
        /// </summary>
        public static string RECT_4 => @"^B_WLD_J(?<h1>\d+\.?\d*)\*(?<h2>\d+\.?\d*)\*(?<b1>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 B_VAR_A 或 B_VAR_B 或 B_VAR_C，后续参数形式为 h1-h2*s[*b1[-b2]]。b1, b2 值均忽略。
        /// </summary>
        public static string RECT_5 => @"^B_VAR_[ABC](?<h1>\d+\.?\d*)-(?<h2>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(\d+\.?\d*)-(\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 L，后续参数形式为 h*s。
        /// </summary>
        public static string L_1 => @"^L(?<h>\d+\.?\d*)\*(?<s>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 BLL 或 CLL 或 L，后续参数形式为 h*b*s。
        /// </summary>
        public static string L_2 => @"^(([BC]L)?L(?<h>\d+\.?\d*)\*(?<b>\d+\.?\d*)\*(?<s>\d+\.?\d*)$";
        /// <summary>
        /// 前置标识符为 L，后续参数形式为 h*b*s*t。
        /// </summary>
        public static string L_3 => @"^L(?<h>\d+\.?\d*)\*(?<b>\d+\.?\d*)\*(?<s>\d+\.?\d*)\*(?<t>\d+\.?\d*)$";
    }
}

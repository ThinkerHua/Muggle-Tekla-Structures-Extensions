using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    /// <summary>
    /// Register default values for weld fields.
    /// </summary>
    /// <remarks>
    /// You need to manually call the "SetDataToDefaultIfUnset" method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class WeldFieldDefaultValuesAttribute : Attribute {
        /// <summary>
        /// Register default values by weld name.
        /// </summary>
        /// <param name="weldName">The name registered by <see cref="WeldFieldsAttribute"/>.</param>
        /// <param name="typeAbove">The default value for the "TypeAbove" property of "BaseWeld".</param>
        /// <param name="typeBelow">The default value for the "TypeBelow" property of "BaseWeld".</param>
        /// <param name="sizeAbove">The default value for the "SizeAbove" property of "BaseWeld".</param>
        /// <param name="sizeBelow">The default value for the "SizeBelow" property of "BaseWeld".</param>
        /// <param name="angleAbove">The default value for the "AngleAbove" property of "BaseWeld".</param>
        /// <param name="angleBelow">The default value for the "AngleBelow" property of "BaseWeld".</param>
        /// <param name="contourAbove">The default value for the "ContourAbove" property of "BaseWeld".</param>
        /// <param name="contourBelow">The default value for the "ContourBelow" property of "BaseWeld".</param>
        /// <param name="finishAbove">The default value for the "FinishAbove" property of "BaseWeld".</param>
        /// <param name="finishBelow">The default value for the "FinishBelow" property of "BaseWeld".</param>
        /// <param name="rootFaceAbove">The default value for the "RootFaceAbove" property of "BaseWeld".</param>
        /// <param name="rootFaceBelow">The default value for the "RootFaceBelow" property of "BaseWeld".</param>
        /// <param name="effectiveThroatAbove">The default value for the "EffectiveThroatAbove" property of "BaseWeld".</param>
        /// <param name="effectiveThroatBelow">The default value for the "EffectiveThroatBelow" property of "BaseWeld".</param>
        /// <param name="rootOpeningAbove">The default value for the "RootOpeningAbove" property of "BaseWeld".</param>
        /// <param name="rootOpeningBelow">The default value for the "RootOpeningBelow" property of "BaseWeld".</param>
        /// <param name="incrementAmountAbove">The default value for the "IncrementAmountAbove" property of "BaseWeld".</param>
        /// <param name="incrementAmountBelow">The default value for the "IncrementAmountBelow" property of "BaseWeld".</param>
        /// <param name="lengthAbove">The default value for the "LengthAbove" property of "BaseWeld".</param>
        /// <param name="lengthBelow">The default value for the "LengthBelow" property of "BaseWeld".</param>
        /// <param name="pitchAbove">The default value for the "PitchAbove" property of "BaseWeld".</param>
        /// <param name="pitchBelow">The default value for the "PitchBelow" property of "BaseWeld".</param>
        /// <param name="around">The default value for the "AroundWeld" property of "BaseWeld".</param>
        /// <param name="shop">The default value for the "ShopWeld" property of "BaseWeld".</param>
        /// <param name="placement">The default value for the "Placement" property of "BaseWeld".</param>
        /// <param name="preparation">The default value for the "Preparation" property of "BaseWeld".</param>
        /// <param name="intermittent">The default value for the "IntermittentType" property of "BaseWeld".</param>
        /// <param name="referenceText">The default value for the "ReferenceText" property of "BaseWeld".</param>
        public WeldFieldDefaultValuesAttribute(
            string weldName, int typeAbove = 0, int typeBelow = 0, double sizeAbove = 0.0, double sizeBelow = 0.0,
            double angleAbove = 0.0, double angleBelow = 0.0, int contourAbove = 0, int contourBelow = 0,
            int finishAbove = 0, int finishBelow = 0, double rootFaceAbove = 0.0, double rootFaceBelow = 0.0,
            double effectiveThroatAbove = 0.0, double effectiveThroatBelow = 0.0,
            double rootOpeningAbove = 0.0, double rootOpeningBelow = 0.0,
            int incrementAmountAbove = 0, int incrementAmountBelow = 0,
            double lengthAbove = 0.0, double lengthBelow = 0.0, double pitchAbove = 0.0, double pitchBelow = 0.0,
            int around = 0, int shop = 0, int placement = 0, int preparation = 0,
            int intermittent = 0, string referenceText = "") {

        }

        /// <summary>
        /// Register default values by weld number.
        /// </summary>
        /// <param name="weldNumber">The number registered by <see cref="WeldFieldsAttribute"/>.</param>
        /// <param name="typeAbove">The default value for the "TypeAbove" property of "BaseWeld".</param>
        /// <param name="typeBelow">The default value for the "TypeBelow" property of "BaseWeld".</param>
        /// <param name="sizeAbove">The default value for the "SizeAbove" property of "BaseWeld".</param>
        /// <param name="sizeBelow">The default value for the "SizeBelow" property of "BaseWeld".</param>
        /// <param name="angleAbove">The default value for the "AngleAbove" property of "BaseWeld".</param>
        /// <param name="angleBelow">The default value for the "AngleBelow" property of "BaseWeld".</param>
        /// <param name="contourAbove">The default value for the "ContourAbove" property of "BaseWeld".</param>
        /// <param name="contourBelow">The default value for the "ContourBelow" property of "BaseWeld".</param>
        /// <param name="finishAbove">The default value for the "FinishAbove" property of "BaseWeld".</param>
        /// <param name="finishBelow">The default value for the "FinishBelow" property of "BaseWeld".</param>
        /// <param name="rootFaceAbove">The default value for the "RootFaceAbove" property of "BaseWeld".</param>
        /// <param name="rootFaceBelow">The default value for the "RootFaceBelow" property of "BaseWeld".</param>
        /// <param name="effectiveThroatAbove">The default value for the "EffectiveThroatAbove" property of "BaseWeld".</param>
        /// <param name="effectiveThroatBelow">The default value for the "EffectiveThroatBelow" property of "BaseWeld".</param>
        /// <param name="rootOpeningAbove">The default value for the "RootOpeningAbove" property of "BaseWeld".</param>
        /// <param name="rootOpeningBelow">The default value for the "RootOpeningBelow" property of "BaseWeld".</param>
        /// <param name="incrementAmountAbove">The default value for the "IncrementAmountAbove" property of "BaseWeld".</param>
        /// <param name="incrementAmountBelow">The default value for the "IncrementAmountBelow" property of "BaseWeld".</param>
        /// <param name="lengthAbove">The default value for the "LengthAbove" property of "BaseWeld".</param>
        /// <param name="lengthBelow">The default value for the "LengthBelow" property of "BaseWeld".</param>
        /// <param name="pitchAbove">The default value for the "PitchAbove" property of "BaseWeld".</param>
        /// <param name="pitchBelow">The default value for the "PitchBelow" property of "BaseWeld".</param>
        /// <param name="around">The default value for the "AroundWeld" property of "BaseWeld".</param>
        /// <param name="shop">The default value for the "ShopWeld" property of "BaseWeld".</param>
        /// <param name="placement">The default value for the "Placement" property of "BaseWeld".</param>
        /// <param name="preparation">The default value for the "Preparation" property of "BaseWeld".</param>
        /// <param name="intermittent">The default value for the "IntermittentType" property of "BaseWeld".</param>
        /// <param name="referenceText">The default value for the "ReferenceText" property of "BaseWeld".</param>
        public WeldFieldDefaultValuesAttribute(
            int weldNumber, int typeAbove = 0, int typeBelow = 0, double sizeAbove = 0.0, double sizeBelow = 0.0,
            double angleAbove = 0.0, double angleBelow = 0.0, int contourAbove = 0, int contourBelow = 0,
            int finishAbove = 0, int finishBelow = 0, double rootFaceAbove = 0.0, double rootFaceBelow = 0.0,
            double effectiveThroatAbove = 0.0, double effectiveThroatBelow = 0.0,
            double rootOpeningAbove = 0.0, double rootOpeningBelow = 0.0,
            int incrementAmountAbove = 0, int incrementAmountBelow = 0,
            double lengthAbove = 0.0, double lengthBelow = 0.0, double pitchAbove = 0.0, double pitchBelow = 0.0,
            int around = 0, int shop = 0, int placement = 0, int preparation = 0,
            int intermittent = 0, string referenceText = "") {

        }
    }
}
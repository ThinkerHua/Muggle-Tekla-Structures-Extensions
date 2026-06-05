using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register the weld properties (with default values) that need to be generated for the applied class.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name pattern 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class WeldPropertiesAttribute : Attribute {
        public string Id { get; set; }
        public int TypeAbove { get; set; }
        public int TypeBelow { get; set; }
        public double SizeAbove { get; set; }
        public double SizeBelow { get; set; }
        public double AngleAbove { get; set; }
        public double AngleBelow { get; set; }
        public int ContourAbove { get; set; }
        public int ContourBelow { get; set; }
        public int FinishAbove { get; set; }
        public int FinishBelow { get; set; }
        public double RootFaceAbove { get; set; }
        public double RootFaceBelow { get; set; }
        public double EffectiveThroatAbove { get; set; }
        public double EffectiveThroatBelow { get; set; }
        public double RootOpeningAbove { get; set; }
        public double RootOpeningBelow { get; set; }
        public int IncrementAmountAbove { get; set; }
        public int IncrementAmountBelow { get; set; }
        public double LengthAbove { get; set; }
        public double LengthBelow { get; set; }
        public double PitchAbove { get; set; }
        public double PitchBelow { get; set; }
        public int Around { get; set; }
        public int Shop { get; set; }
        public int Placement { get; set; }
        public int Preparation { get; set; }
        public int Intermittent { get; set; }
        public string ReferenceText { get; set; }

        /// <summary>
        /// Register weld properties with preset default values using the given id.
        /// </summary>
        /// <remarks>The preset default values is: = 0, typeBelow = 0, sizeAbove = 0.0, sizeBelow = 0.0,
        /// angleAbove = 0.0, angleBelow = 0.0, contourAbove = 0, contourBelow = 0, finishAbove = 0, finishBelow = 0,
        /// rootFaceAbove = 0.0, rootFaceBelow = 0.0, effectiveThroatAbove = 0.0, effectiveThroatBelow = 0.0,
        /// rootOpeningAbove = 0.0, rootOpeningBelow = 0.0, incrementAmountAbove = 0, incrementAmountBelow = 0,
        /// lengthAbove = 0.0, lengthBelow = 0.0, pitchAbove = 0.0, pitchBelow = 0.0, around = 0, shop = 0,
        /// placement = 0, preparation = 0, intermittent = 0, referenceText = "".</remarks>
        /// <param name="id">The weld id.</param>
        public WeldPropertiesAttribute(uint id) {
            Id = id.ToString();
            TypeAbove = 0;
            TypeBelow = 0;
            SizeAbove = 0;
            SizeBelow = 0;
            AngleAbove = 0;
            AngleBelow = 0;
            ContourAbove = 0;
            ContourBelow = 0;
            FinishAbove = 0;
            FinishBelow = 0;
            RootFaceAbove = 0;
            RootFaceBelow = 0;
            EffectiveThroatAbove = 0;
            EffectiveThroatBelow = 0;
            RootOpeningAbove = 0;
            RootOpeningBelow = 0;
            IncrementAmountAbove = 0;
            IncrementAmountBelow = 0;
            LengthAbove = 0;
            LengthBelow = 0;
            PitchAbove = 0;
            PitchBelow = 0;
            Around = 0;
            Shop = 0;
            Placement = 0;
            Preparation = 0;
            Intermittent = 0;
            ReferenceText = "";
        }

        /// <summary>
        /// Register weld properties with preset default values using the given id.
        /// </summary>
        /// <remarks>The preset default values is: = 0, typeBelow = 0, sizeAbove = 0.0, sizeBelow = 0.0,
        /// angleAbove = 0.0, angleBelow = 0.0, contourAbove = 0, contourBelow = 0, finishAbove = 0, finishBelow = 0,
        /// rootFaceAbove = 0.0, rootFaceBelow = 0.0, effectiveThroatAbove = 0.0, effectiveThroatBelow = 0.0,
        /// rootOpeningAbove = 0.0, rootOpeningBelow = 0.0, incrementAmountAbove = 0, incrementAmountBelow = 0,
        /// lengthAbove = 0.0, lengthBelow = 0.0, pitchAbove = 0.0, pitchBelow = 0.0, around = 0, shop = 0,
        /// placement = 0, preparation = 0, intermittent = 0, referenceText = "".</remarks>
        /// <param name="id">The weld id.</param>
        public WeldPropertiesAttribute(string id) {
            Id = id;
            TypeAbove = 0;
            TypeBelow = 0;
            SizeAbove = 0;
            SizeBelow = 0;
            AngleAbove = 0;
            AngleBelow = 0;
            ContourAbove = 0;
            ContourBelow = 0;
            FinishAbove = 0;
            FinishBelow = 0;
            RootFaceAbove = 0;
            RootFaceBelow = 0;
            EffectiveThroatAbove = 0;
            EffectiveThroatBelow = 0;
            RootOpeningAbove = 0;
            RootOpeningBelow = 0;
            IncrementAmountAbove = 0;
            IncrementAmountBelow = 0;
            LengthAbove = 0;
            LengthBelow = 0;
            PitchAbove = 0;
            PitchBelow = 0;
            Around = 0;
            Shop = 0;
            Placement = 0;
            Preparation = 0;
            Intermittent = 0;
            ReferenceText = "";
        }

        /// <summary>
        /// Register weld properties with default values using the given id.
        /// </summary>
        /// <param name="id">The weld id.</param>
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
        public WeldPropertiesAttribute(uint id,
            int typeAbove = 0, int typeBelow = 0, double sizeAbove = 0.0, double sizeBelow = 0.0,
            double angleAbove = 0.0, double angleBelow = 0.0, int contourAbove = 0, int contourBelow = 0,
            int finishAbove = 0, int finishBelow = 0, double rootFaceAbove = 0.0, double rootFaceBelow = 0.0,
            double effectiveThroatAbove = 0.0, double effectiveThroatBelow = 0.0,
            double rootOpeningAbove = 0.0, double rootOpeningBelow = 0.0,
            int incrementAmountAbove = 0, int incrementAmountBelow = 0,
            double lengthAbove = 0.0, double lengthBelow = 0.0, double pitchAbove = 0.0, double pitchBelow = 0.0,
            int around = 0, int shop = 0, int placement = 0, int preparation = 0,
            int intermittent = 0, string referenceText = "") {
            
            Id = id.ToString();
            TypeAbove = typeAbove;
            TypeBelow = typeBelow;
            SizeAbove = sizeAbove;
            SizeBelow = sizeBelow;
            AngleAbove = angleAbove;
            AngleBelow = angleBelow;
            ContourAbove = contourAbove;
            ContourBelow = contourBelow;
            FinishAbove = finishAbove;
            FinishBelow = finishBelow;
            RootFaceAbove = rootFaceAbove;
            RootFaceBelow = rootFaceBelow;
            EffectiveThroatAbove = effectiveThroatAbove;
            EffectiveThroatBelow = effectiveThroatBelow;
            RootOpeningAbove = rootOpeningAbove;
            RootOpeningBelow = rootOpeningBelow;
            IncrementAmountAbove = incrementAmountAbove;
            IncrementAmountBelow = incrementAmountBelow;
            LengthAbove = lengthAbove;
            LengthBelow = lengthBelow;
            PitchAbove = pitchAbove;
            PitchBelow = pitchBelow;
            Around = around;
            Shop = shop;
            Placement = placement;
            Preparation = preparation;
            Intermittent = intermittent;
            ReferenceText = referenceText;
        }

        /// <summary>
        /// Register weld properties with default values using the given id.
        /// </summary>
        /// <param name="id">The weld id.</param>
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
        public WeldPropertiesAttribute(string id, 
            int typeAbove = 0, int typeBelow = 0, double sizeAbove = 0.0, double sizeBelow = 0.0,
            double angleAbove = 0.0, double angleBelow = 0.0, int contourAbove = 0, int contourBelow = 0,
            int finishAbove = 0, int finishBelow = 0, double rootFaceAbove = 0.0, double rootFaceBelow = 0.0,
            double effectiveThroatAbove = 0.0, double effectiveThroatBelow = 0.0,
            double rootOpeningAbove = 0.0, double rootOpeningBelow = 0.0,
            int incrementAmountAbove = 0, int incrementAmountBelow = 0,
            double lengthAbove = 0.0, double lengthBelow = 0.0, double pitchAbove = 0.0, double pitchBelow = 0.0,
            int around = 0, int shop = 0, int placement = 0, int preparation = 0,
            int intermittent = 0, string referenceText = "") {
            
            Id = id;
            TypeAbove = typeAbove;
            TypeBelow = typeBelow;
            SizeAbove = sizeAbove;
            SizeBelow = sizeBelow;
            AngleAbove = angleAbove;
            AngleBelow = angleBelow;
            ContourAbove = contourAbove;
            ContourBelow = contourBelow;
            FinishAbove = finishAbove;
            FinishBelow = finishBelow;
            RootFaceAbove = rootFaceAbove;
            RootFaceBelow = rootFaceBelow;
            EffectiveThroatAbove = effectiveThroatAbove;
            EffectiveThroatBelow = effectiveThroatBelow;
            RootOpeningAbove = rootOpeningAbove;
            RootOpeningBelow = rootOpeningBelow;
            IncrementAmountAbove = incrementAmountAbove;
            IncrementAmountBelow = incrementAmountBelow;
            LengthAbove = lengthAbove;
            LengthBelow = lengthBelow;
            PitchAbove = pitchAbove;
            PitchBelow = pitchBelow;
            Around = around;
            Shop = shop;
            Placement = placement;
            Preparation = preparation;
            Intermittent = intermittent;
            ReferenceText = referenceText;
        }

    }

}
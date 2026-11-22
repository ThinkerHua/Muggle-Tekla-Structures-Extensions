namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register weld data.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name patterns 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class WeldAttribute : System.Attribute {
        public string Id { get; set; }
        
        public string PrefixAbove { get; set; }
        
        public string PrefixBelow { get; set; }
        
        public int TypeAbove { get; set; }
        
        public int TypeBelow { get; set; }
        
        public double SizeAbove { get; set; }
        
        public double SizeBelow { get; set; }
        
        public double AdditionalSizeAbove { get; set; }
        
        public double AdditionalSizeBelow { get; set; }
        
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
        
        public int Around { get; set; }
        
        public int Shop { get; set; }
        
        public int Placement { get; set; }
        
        public int Preparation { get; set; }
        
        public int Intermittent { get; set; }
        
        public int IncrementAmountAbove { get; set; }
        
        public int IncrementAmountBelow { get; set; }
        
        public double LengthAbove { get; set; }
        
        public double LengthBelow { get; set; }
        
        public double PitchAbove { get; set; }
        
        public double PitchBelow { get; set; }
        
        public string ReferenceText { get; set; }

        /// <summary>
        /// Register weld data by the given id.
        /// </summary>
        /// <remarks>The default values is: PrefixAbove = "", PrefixBelow = "", TypeAbove = 0, TypeBelow = 0,
        /// SizeAbove = 0.0, SizeBelow = 0.0, AdditionalSizeAbove = 0.0, AdditionalSizeBelow = 0.0, AngleAbove = 0.0,
        /// AngleBelow = 0.0, ContourAbove = 0, ContourBelow = 0, FinishAbove = 0, FinishBelow = 0, RootFaceAbove = 0.0,
        /// RootFaceBelow = 0.0, EffectiveThroatAbove = 0.0, EffectiveThroatBelow = 0.0, RootOpeningAbove = 0.0,
        /// RootOpeningBelow = 0.0, Around = 0, Shop = 0, Placement = 0, Preparation = 0, Intermittent = 0,
        /// IncrementAmountAbove = 0, IncrementAmountBelow = 0, LengthAbove = 0.0, LengthBelow = 0.0, PitchAbove = 0.0,
        /// PitchBelow = 0.0, ReferenceText = "".</remarks>
        /// <param name="id">The weld id.</param>
        public WeldAttribute(uint id) {
            Id = id.ToString();
            PrefixAbove = "";
            PrefixBelow = "";
            TypeAbove = 0;
            TypeBelow = 0;
            SizeAbove = 0;
            SizeBelow = 0;
            AdditionalSizeAbove = 0;
            AdditionalSizeBelow = 0;
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
            Around = 0;
            Shop = 0;
            Placement = 0;
            Preparation = 0;
            Intermittent = 0;
            IncrementAmountAbove = 0;
            IncrementAmountBelow = 0;
            LengthAbove = 0;
            LengthBelow = 0;
            PitchAbove = 0;
            PitchBelow = 0;
            ReferenceText = "";
        }

        /// <summary>
        /// Register weld data by the given id.
        /// </summary>
        /// <remarks>The default values is: PrefixAbove = "", PrefixBelow = "", TypeAbove = 0, TypeBelow = 0,
        /// SizeAbove = 0.0, SizeBelow = 0.0, AdditionalSizeAbove = 0.0, AdditionalSizeBelow = 0.0, AngleAbove = 0.0,
        /// AngleBelow = 0.0, ContourAbove = 0, ContourBelow = 0, FinishAbove = 0, FinishBelow = 0, RootFaceAbove = 0.0,
        /// RootFaceBelow = 0.0, EffectiveThroatAbove = 0.0, EffectiveThroatBelow = 0.0, RootOpeningAbove = 0.0,
        /// RootOpeningBelow = 0.0, Around = 0, Shop = 0, Placement = 0, Preparation = 0, Intermittent = 0,
        /// IncrementAmountAbove = 0, IncrementAmountBelow = 0, LengthAbove = 0.0, LengthBelow = 0.0, PitchAbove = 0.0,
        /// PitchBelow = 0.0, ReferenceText = "".</remarks>
        /// <param name="id">The weld id.</param>
        public WeldAttribute(string id) {
            Id = id;
            PrefixAbove = "";
            PrefixBelow = "";
            TypeAbove = 0;
            TypeBelow = 0;
            SizeAbove = 0;
            SizeBelow = 0;
            AdditionalSizeAbove = 0;
            AdditionalSizeBelow = 0;
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
            Around = 0;
            Shop = 0;
            Placement = 0;
            Preparation = 0;
            Intermittent = 0;
            IncrementAmountAbove = 0;
            IncrementAmountBelow = 0;
            LengthAbove = 0;
            LengthBelow = 0;
            PitchAbove = 0;
            PitchBelow = 0;
            ReferenceText = "";
        }
    }
}
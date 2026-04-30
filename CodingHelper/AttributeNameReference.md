# Mapping relationship between properties and attribute name pattern

Note:

- The string "**\<nameOrNumber>**" represents the actual name or number that is set, including the angle brackets.

- "**Plate**" is not an actual model object type in OpenAPI, the way to create it depends on the programmer.

- "BoltGroup.**BoltDistX**" and "BoltGroup.**BoltDistY**" are not actual properties in OpenAPI, they map to 
  "**AddBoltDistX**" and "**AddBoltDistY**" methods of "BoltArray" or "BoltXYList".

| Properties                       | Attribute name patterns |
|----------------------------------|-------------------------|
| Part\.Name                       | PT\<nameOrNumber>NAME   |
| Part.Profile.ProfileString       | PT\<nameOrNumber>PRF    |
| Part.Material.MaterialString     | PT\<nameOrNumber>MATL   |
| Part.Finish                      | PT\<nameOrNumber>FNSH   |
| Part.Class                       | PT\<nameOrNumber>CLS    |
| Part.PartNumber.Prefix           | PT\<nameOrNumber>PTP    |
| Part.PartNumber.StartNumber      | PT\<nameOrNumber>PTN    |
| Part.AssemblyNumber.Prefix       | PT\<nameOrNumber>ASMP   |
| Part.AssemblyNumber.StartNumber  | PT\<nameOrNumber>ASMN   |
| Plate\.Name                      | PL\<nameOrNumber>NAME   |
| Plate.Thickness                  | PL\<nameOrNumber>T      |
| Plate.Breadth                    | PL\<nameOrNumber>B      |
| Plate.Height                     | PL\<nameOrNumber>H      |
| Plate.Material.MaterialString    | PL\<nameOrNumber>MATL   |
| Plate.Finish                     | PL\<nameOrNumber>FNSH   |
| Plate.Class                      | PL\<nameOrNumber>CLS    |
| Plate.PartNumber.Prefix          | PL\<nameOrNumber>PTP    |
| Plate.PartNumber.StartNumber     | PL\<nameOrNumber>PTN    |
| Plate.AssemblyNumber.Prefix      | PL\<nameOrNumber>ASMP   |
| Plate.AssemblyNumber.StartNumber | PL\<nameOrNumber>ASMN   |
| BaseWeld.SizeAbove               | W\<nameOrNumber>SIZEA   |
| BaseWeld.SizeBelow               | W\<nameOrNumber>SIZEB   |
| BaseWeld.TypeAbove               | W\<nameOrNumber>TYPEA   |
| BaseWeld.TypeBelow               | W\<nameOrNumber>TYPEB   |
| BaseWeld.AngleAbove              | W\<nameOrNumber>ANGA    |
| BaseWeld.AngleBelow              | W\<nameOrNumber>ANGB    |
| BaseWeld.ContourAbove            | W\<nameOrNumber>CTRA    |
| BaseWeld.ContourBelow            | W\<nameOrNumber>CTRB    |
| BaseWeld.FinishAbove             | W\<nameOrNumber>FNSHA   |
| BaseWeld.FinishBelow             | W\<nameOrNumber>FNSHB   |
| BaseWeld.RootFaceAbove           | W\<nameOrNumber>FACEA   |
| BaseWeld.RootFaceBelow           | W\<nameOrNumber>FACEB   |
| BaseWeld.EffectiveThroatAbove    | W\<nameOrNumber>THROA   |
| BaseWeld.EffectiveThroatBelow    | W\<nameOrNumber>THROB   |
| BaseWeld.RootOpeningAbove        | W\<nameOrNumber>OPNGA   |
| BaseWeld.RootOpeningBelow        | W\<nameOrNumber>OPNGB   |
| BaseWeld.IncrementAmountAbove    | W\<nameOrNumber>INCRA   |
| BaseWeld.IncrementAmountBelow    | W\<nameOrNumber>INCRB   |
| BaseWeld.LengthAbove             | W\<nameOrNumber>LENA    |
| BaseWeld.LengthBelow             | W\<nameOrNumber>LENB    |
| BaseWeld.PitchAbove              | W\<nameOrNumber>PITA    |
| BaseWeld.PitchBelow              | W\<nameOrNumber>PITB    |
| BaseWeld.AroundWeld              | W\<nameOrNumber>ARND    |
| BaseWeld.ShopWeld                | W\<nameOrNumber>SHOP    |
| BaseWeld.Placement               | W\<nameOrNumber>PLACE   |
| BaseWeld.Preparation             | W\<nameOrNumber>PREP    |
| BaseWeld.IntermittentType        | W\<nameOrNumber>INTMI   |
| BaseWeld.ReferenceText           | W\<nameOrNumber>TEXT    |
| BoltGroup.BoltSize               | B\<nameOrNumber>SIZE    |
| BoltGroup.BoltStandard           | B\<nameOrNumber>STD     |
| BoltGroup.BoltDistX              | B\<nameOrNumber>DISTX   |
| BoltGroup.BoltDistY              | B\<nameOrNumber>DISTY   |
| BoltGroup.BoltType               | B\<nameOrNumber>TYPE    |
| BoltGroup.ThreadInMaterial       | B\<nameOrNumber>THRD    |
| BoltGroup.CutLength              | B\<nameOrNumber>CLEN    |
| BoltGroup.ExtraLength            | B\<nameOrNumber>XLEN    |
| BoltGroup.Tolerance              | B\<nameOrNumber>TOL     |
| BoltGroup.PlainHoleType          | B\<nameOrNumber>PLAIN   |
| BoltGroup.BlindHoleDepth         | B\<nameOrNumber>DEPTH   |
| BoltGroup.Hole1                  | B\<nameOrNumber>HOLE1   |
| BoltGroup.Hole2                  | B\<nameOrNumber>HOLE2   |
| BoltGroup.Hole3                  | B\<nameOrNumber>HOLE3   |
| BoltGroup.Hole4                  | B\<nameOrNumber>HOLE4   |
| BoltGroup.Hole5                  | B\<nameOrNumber>HOLE5   |
| BoltGroup.HoleType               | B\<nameOrNumber>HOLTY   |
| BoltGroup.SlottedHoleX           | B\<nameOrNumber>SLOTX   |
| BoltGroup.SlottedHoleY           | B\<nameOrNumber>SLOTY   |
| BoltGroup.RotateSlots            | B\<nameOrNumber>RSLOT   |
| BoltGroup.Bolt                   | B\<nameOrNumber>ISBOT   |
| BoltGroup.Nut1                   | B\<nameOrNumber>NUT1    |
| BoltGroup.Nut2                   | B\<nameOrNumber>NUT2    |
| BoltGroup.Washer1                | B\<nameOrNumber>WSHR1   |
| BoltGroup.Washer2                | B\<nameOrNumber>WSHR2   |
| BoltGroup.Washer3                | B\<nameOrNumber>WSHR3   |
| BoltCircle.BoltSize              | BC\<nameOrNumber>SIZE   |
| BoltCircle.BoltStandard          | BC\<nameOrNumber>STD    |
| BoltCircle.NumberOfBolts         | BC\<nameOrNumber>NUM    |
| BoltCircle.Diameter              | BC\<nameOrNumber>DIAM   |
| BoltCircle.BoltType              | BC\<nameOrNumber>TYPE   |
| BoltCircle.ThreadInMaterial      | BC\<nameOrNumber>THRD   |
| BoltCircle.CutLength             | BC\<nameOrNumber>CLEN   |
| BoltCircle.ExtraLength           | BC\<nameOrNumber>XLEN   |
| BoltCircle.Tolerance             | BC\<nameOrNumber>TOL    |
| BoltCircle.PlainHoleType         | BC\<nameOrNumber>PLAIN  |
| BoltCircle.BlindHoleDepth        | BC\<nameOrNumber>DEPTH  |
| BoltCircle.Hole1                 | BC\<nameOrNumber>HOLE1  |
| BoltCircle.Hole2                 | BC\<nameOrNumber>HOLE2  |
| BoltCircle.Hole3                 | BC\<nameOrNumber>HOLE3  |
| BoltCircle.Hole4                 | BC\<nameOrNumber>HOLE4  |
| BoltCircle.Hole5                 | BC\<nameOrNumber>HOLE5  |
| BoltCircle.HoleType              | BC\<nameOrNumber>HOLTY  |
| BoltCircle.SlottedHoleX          | BC\<nameOrNumber>SLOTX  |
| BoltCircle.SlottedHoleY          | BC\<nameOrNumber>SLOTY  |
| BoltCircle.RotateSlots           | BC\<nameOrNumber>RSLOT  |
| BoltCircle.Bolt                  | BC\<nameOrNumber>ISBOT  |
| BoltCircle.Nut1                  | BC\<nameOrNumber>NUT1   |
| BoltCircle.Nut2                  | BC\<nameOrNumber>NUT2   |
| BoltCircle.Washer1               | BC\<nameOrNumber>WSHR1  |
| BoltCircle.Washer2               | BC\<nameOrNumber>WSHR2  |
| BoltCircle.Washer3               | BC\<nameOrNumber>WSHR3  |

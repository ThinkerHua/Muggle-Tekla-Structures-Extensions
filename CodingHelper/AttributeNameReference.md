# Mapping relationship between properties and attribute name pattern

Note:

- The string "**\<Id>**" is used to identify the object, including the angle brackets.

- "**Plate**" is not an actual model object type in OpenAPI, the way to create it depends on the programmer.

- "BoltGroup.**BoltDistX**" and "BoltGroup.**BoltDistY**" are not actual properties in OpenAPI, they map to 
  "**AddBoltDistX**" and "**AddBoltDistY**" methods of "BoltArray" or "BoltXYList".

- "BoltGroup.**SlotOffsetX**", "BoltGroup.**SlotOffsetY**", "BoltCircle.**SlotOffsetX**" 
  and "BoltCircle.**SlotOffsetY**" are only available for Tekla Structures 2023 and later version.

| Properties                       | Attribute name patterns |
|----------------------------------|-------------------------|
| Part\.Name                       | PT\<Id>NAME             |
| Part.Profile.ProfileString       | PT\<Id>PRF              |
| Part.Material.MaterialString     | PT\<Id>MATL             |
| Part.Finish                      | PT\<Id>FNSH             |
| Part.Class                       | PT\<Id>CLS              |
| Part.PartNumber.Prefix           | PT\<Id>PTP              |
| Part.PartNumber.StartNumber      | PT\<Id>PTN              |
| Part.AssemblyNumber.Prefix       | PT\<Id>ASMP             |
| Part.AssemblyNumber.StartNumber  | PT\<Id>ASMN             |
| Plate\.Name                      | PL\<Id>NAME             |
| Plate.Thickness                  | PL\<Id>T                |
| Plate.Breadth                    | PL\<Id>B                |
| Plate.Height                     | PL\<Id>H                |
| Plate.Material.MaterialString    | PL\<Id>MATL             |
| Plate.Finish                     | PL\<Id>FNSH             |
| Plate.Class                      | PL\<Id>CLS              |
| Plate.PartNumber.Prefix          | PL\<Id>PTP              |
| Plate.PartNumber.StartNumber     | PL\<Id>PTN              |
| Plate.AssemblyNumber.Prefix      | PL\<Id>ASMP             |
| Plate.AssemblyNumber.StartNumber | PL\<Id>ASMN             |
| BaseWeld.SizeAbove               | W\<Id>SIZEA             |
| BaseWeld.SizeBelow               | W\<Id>SIZEB             |
| BaseWeld.TypeAbove               | W\<Id>TYPEA             |
| BaseWeld.TypeBelow               | W\<Id>TYPEB             |
| BaseWeld.AngleAbove              | W\<Id>ANGA              |
| BaseWeld.AngleBelow              | W\<Id>ANGB              |
| BaseWeld.ContourAbove            | W\<Id>CTRA              |
| BaseWeld.ContourBelow            | W\<Id>CTRB              |
| BaseWeld.FinishAbove             | W\<Id>FNSHA             |
| BaseWeld.FinishBelow             | W\<Id>FNSHB             |
| BaseWeld.RootFaceAbove           | W\<Id>FACEA             |
| BaseWeld.RootFaceBelow           | W\<Id>FACEB             |
| BaseWeld.EffectiveThroatAbove    | W\<Id>THROA             |
| BaseWeld.EffectiveThroatBelow    | W\<Id>THROB             |
| BaseWeld.RootOpeningAbove        | W\<Id>OPNGA             |
| BaseWeld.RootOpeningBelow        | W\<Id>OPNGB             |
| BaseWeld.IncrementAmountAbove    | W\<Id>INCRA             |
| BaseWeld.IncrementAmountBelow    | W\<Id>INCRB             |
| BaseWeld.LengthAbove             | W\<Id>LENA              |
| BaseWeld.LengthBelow             | W\<Id>LENB              |
| BaseWeld.PitchAbove              | W\<Id>PITA              |
| BaseWeld.PitchBelow              | W\<Id>PITB              |
| BaseWeld.AroundWeld              | W\<Id>ARND              |
| BaseWeld.ShopWeld                | W\<Id>SHOP              |
| BaseWeld.Placement               | W\<Id>PLACE             |
| BaseWeld.Preparation             | W\<Id>PREP              |
| BaseWeld.IntermittentType        | W\<Id>INTMI             |
| BaseWeld.ReferenceText           | W\<Id>TEXT              |
| BoltGroup.BoltSize               | B\<Id>SIZE              |
| BoltGroup.BoltStandard           | B\<Id>STD               |
| BoltGroup.BoltDistX              | B\<Id>DISTX             |
| BoltGroup.BoltDistY              | B\<Id>DISTY             |
| BoltGroup.BoltType               | B\<Id>TYPE              |
| BoltGroup.ThreadInMaterial       | B\<Id>THRD              |
| BoltGroup.Length                 | B\<Id>LEN               |
| BoltGroup.CutLength              | B\<Id>CLEN              |
| BoltGroup.ExtraLength            | B\<Id>XLEN              |
| BoltGroup.Tolerance              | B\<Id>TOL               |
| BoltGroup.PlainHoleType          | B\<Id>PLAIN             |
| BoltGroup.BlindHoleDepth         | B\<Id>DEPTH             |
| BoltGroup.Hole1                  | B\<Id>HOLE1             |
| BoltGroup.Hole2                  | B\<Id>HOLE2             |
| BoltGroup.Hole3                  | B\<Id>HOLE3             |
| BoltGroup.Hole4                  | B\<Id>HOLE4             |
| BoltGroup.Hole5                  | B\<Id>HOLE5             |
| BoltGroup.HoleType               | B\<Id>HOLTY             |
| BoltGroup.SlottedHoleX           | B\<Id>SLOTX             |
| BoltGroup.SlottedHoleY           | B\<Id>SLOTY             |
| BoltGroup.SlotOffsetX            | B\<Id>SOFFX             |
| BoltGroup.SlotOffsetY            | B\<Id>SOFFY             |
| BoltGroup.RotateSlots            | B\<Id>RSLOT             |
| BoltGroup.Bolt                   | B\<Id>ISBOT             |
| BoltGroup.Nut1                   | B\<Id>NUT1              |
| BoltGroup.Nut2                   | B\<Id>NUT2              |
| BoltGroup.Washer1                | B\<Id>WSHR1             |
| BoltGroup.Washer2                | B\<Id>WSHR2             |
| BoltGroup.Washer3                | B\<Id>WSHR3             |
| BoltCircle.BoltSize              | BC\<Id>SIZE             |
| BoltCircle.BoltStandard          | BC\<Id>STD              |
| BoltCircle.NumberOfBolts         | BC\<Id>NUM              |
| BoltCircle.Diameter              | BC\<Id>DIAM             |
| BoltCircle.BoltType              | BC\<Id>TYPE             |
| BoltCircle.ThreadInMaterial      | BC\<Id>THRD             |
| BoltCircle.Length                | BC\<Id>LEN              |
| BoltCircle.CutLength             | BC\<Id>CLEN             |
| BoltCircle.ExtraLength           | BC\<Id>XLEN             |
| BoltCircle.Tolerance             | BC\<Id>TOL              |
| BoltCircle.PlainHoleType         | BC\<Id>PLAIN            |
| BoltCircle.BlindHoleDepth        | BC\<Id>DEPTH            |
| BoltCircle.Hole1                 | BC\<Id>HOLE1            |
| BoltCircle.Hole2                 | BC\<Id>HOLE2            |
| BoltCircle.Hole3                 | BC\<Id>HOLE3            |
| BoltCircle.Hole4                 | BC\<Id>HOLE4            |
| BoltCircle.Hole5                 | BC\<Id>HOLE5            |
| BoltCircle.HoleType              | BC\<Id>HOLTY            |
| BoltCircle.SlottedHoleX          | BC\<Id>SLOTX            |
| BoltCircle.SlottedHoleY          | BC\<Id>SLOTY            |
| BoltCircle.SlotOffsetX           | BC\<Id>SOFFX            |
| BoltCircle.SlotOffsetY           | BC\<Id>SOFFY            |
| BoltCircle.RotateSlots           | BC\<Id>RSLOT            |
| BoltCircle.Bolt                  | BC\<Id>ISBOT            |
| BoltCircle.Nut1                  | BC\<Id>NUT1             |
| BoltCircle.Nut2                  | BC\<Id>NUT2             |
| BoltCircle.Washer1               | BC\<Id>WSHR1            |
| BoltCircle.Washer2               | BC\<Id>WSHR2            |
| BoltCircle.Washer3               | BC\<Id>WSHR3            |
| Chamfer.Type                     | CF\<Id>TYPE             |
| Chamfer.X                        | CF\<Id>X                |
| Chamfer.Y                        | CF\<Id>Y                |
| Chamfer.Dz1                      | CF\<Id>DZ1              |
| Chamfer.Dz2                      | CF\<Id>DZ2              |

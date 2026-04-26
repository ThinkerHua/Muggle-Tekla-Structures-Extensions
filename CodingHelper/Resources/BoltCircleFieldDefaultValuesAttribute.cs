using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    /// <summary>
    /// Register default values for bolt circle fields.
    /// </summary>
    /// <remarks>
    /// You need to manually call the "SetDataToDefaultIfUnset" method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class BoltCircleFieldDefaultValuesAttribute : Attribute {
        /// <summary>
        /// Register default values by bolt circle name.
        /// </summary>
        /// <param name="boltCircleName">The name registered by <see cref="BoltCircleFieldsAttribute"/>.</param>
        /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
        /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
        /// <param name="numberOfBolts">The default value for the "NumberOfBolts" property of "BoltCircle".</param>
        /// <param name="diameter">The default value for the "Diameter" property of "BoltCircle".</param>
        /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
        /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
        /// <param name="cutLength">The default value for the "CutLength" property of "BoltGroup".</param>
        /// <param name="extraLength">The default value for the "ExtraLength" property of "BoltGroup".</param>
        /// <param name="tolerance">The default value for the "Tolerance" property of "BoltGroup".</param>
        /// <param name="plainType">The default value for the "PlainHoleType" property of "BoltGroup".</param>
        /// <param name="blindHoleDepth">The default value for the "BlindHoleDepth" property of "BoltGroup".</param>
        /// <param name="hole1">The default value for the "Hole1" property of "BoltGroup".</param>
        /// <param name="hole2">The default value for the "Hole2" property of "BoltGroup".</param>
        /// <param name="hole3">The default value for the "Hole3" property of "BoltGroup".</param>
        /// <param name="hole4">The default value for the "Hole4" property of "BoltGroup".</param>
        /// <param name="hole5">The default value for the "Hole5" property of "BoltGroup".</param>
        /// <param name="holeType">The default value for the "HoleType" property of "BoltGroup".</param>
        /// <param name="slottedHoleX">The default value for the "SlottedHoleX" property of "BoltGroup".</param>
        /// <param name="slottedHoleY">The default value for the "SlottedHoleY" property of "BoltGroup".</param>
        /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
        /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
        /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
        /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
        /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
        /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
        /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
        public BoltCircleFieldDefaultValuesAttribute(
            string boltCircleName, double size = 8.0, string standard = "A", int numberOfBolts = 6,
            double diameter = 100.0, int type = 0, int threadInMaterial = 1, double cutLength = 100.0,
            double extraLength = 0.0, double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0,
            int hole1 = 1, int hole2 = 1, int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0,
            double slottedHoleX = 0.0, double slottedHoleY = 0.0, int rotateSlots = 2, int isBolt = 1,
            int useNut1 = 1, int useNut2 = 0, int useWasher1 = 0, int useWasher2 = 0, int useWasher3 = 1) {

        }

        /// <summary>
        /// Register default values by bolt circle number.
        /// </summary>
        /// <param name="boltCircleNumber">The number registered by <see cref="BoltCircleFieldsAttribute"/>.</param>
        /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
        /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
        /// <param name="numberOfBolts">The default value for the "NumberOfBolts" property of "BoltCircle".</param>
        /// <param name="diameter">The default value for the "Diameter" property of "BoltCircle".</param>
        /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
        /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
        /// <param name="cutLength">The default value for the "CutLength" property of "BoltGroup".</param>
        /// <param name="extraLength">The default value for the "ExtraLength" property of "BoltGroup".</param>
        /// <param name="tolerance">The default value for the "Tolerance" property of "BoltGroup".</param>
        /// <param name="plainType">The default value for the "PlainHoleType" property of "BoltGroup".</param>
        /// <param name="blindHoleDepth">The default value for the "BlindHoleDepth" property of "BoltGroup".</param>
        /// <param name="hole1">The default value for the "Hole1" property of "BoltGroup".</param>
        /// <param name="hole2">The default value for the "Hole2" property of "BoltGroup".</param>
        /// <param name="hole3">The default value for the "Hole3" property of "BoltGroup".</param>
        /// <param name="hole4">The default value for the "Hole4" property of "BoltGroup".</param>
        /// <param name="hole5">The default value for the "Hole5" property of "BoltGroup".</param>
        /// <param name="holeType">The default value for the "HoleType" property of "BoltGroup".</param>
        /// <param name="slottedHoleX">The default value for the "SlottedHoleX" property of "BoltGroup".</param>
        /// <param name="slottedHoleY">The default value for the "SlottedHoleY" property of "BoltGroup".</param>
        /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
        /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
        /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
        /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
        /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
        /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
        /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
        public BoltCircleFieldDefaultValuesAttribute(
            int boltCircleNumber, double size = 8.0, string standard = "A", int numberOfBolts = 6,
            double diameter = 100.0, int type = 0, int threadInMaterial = 1, double cutLength = 100.0,
            double extraLength = 0.0, double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0,
            int hole1 = 1, int hole2 = 1, int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0,
            double slottedHoleX = 0.0, double slottedHoleY = 0.0, int rotateSlots = 2, int isBolt = 1,
            int useNut1 = 1, int useNut2 = 0, int useWasher1 = 0, int useWasher2 = 0, int useWasher3 = 1) {

        }
    }
}
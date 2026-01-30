using System;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace UnitTesting_NUnit3 {
    public class TGetProfileType {

        private readonly List<Part> partList = [];

        [SetUp]
        public void SetUp() {
            var model = new Model();
            if (!model.GetConnectionStatus()) {
                Assert.Inconclusive($"Model connection is not established.");
            }

            var picker = new Picker();
            var partEnum = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
            foreach (Part part in partEnum) {
                partList.Add(part);
            }
        }

        [Test]
        public void GetProfileType() {
            var formatString = "{0,-30}{1}";
            var profile = string.Empty;
            var profileType = string.Empty;
            Console.WriteLine(string.Format(formatString, "Profile", "Type"));
            foreach (var part in partList) {
                part.GetReportProperty("PROFILE", ref profile);
                part.GetReportProperty("PROFILE_TYPE", ref profileType);

                Console.WriteLine(string.Format(formatString, profile, profileType));
            }
        }
    }
}

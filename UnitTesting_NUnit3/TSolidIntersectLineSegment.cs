using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace UnitTesting_NUnit3 {
    internal class TSolidIntersectLineSegment {
        private Model model;
        private Picker picker;

        [SetUp]
        public void SetUp() {
            model = new Model();
            if (!model.GetConnectionStatus()) {
                Assert.Inconclusive("Connection cannot be established.");
            }
            picker = new Picker();
        }

        [Test]
        public void TestIntersection() {
            try {
                var part = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART) as Part;
                var beam = (Beam)picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART);

                var solid = part.GetSolid(Solid.SolidCreationTypeEnum.RAW);

                var result = solid.Intersect(beam.StartPoint, beam.EndPoint);

                foreach (Point point in result) {
                    Console.WriteLine(point);
                }
            } catch (Exception e) when (e.Message == "User interrupted") {
                Assert.Inconclusive("Test be cancelled.");
            } catch (Exception e) {
                Assert.Fail($"Test failed: {e.Message}");
            }
        }
    }
}

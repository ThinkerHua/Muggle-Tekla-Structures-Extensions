using System;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace UnitTesting_NUnit3 {
    public class TReferenceModel {
        private readonly Model model = new Model();
        private ReferenceModel referenceModel;

        [SetUp]
        public void Setup() {
            if (!model.GetConnectionStatus()) {
                Assert.Inconclusive("Connection to Tekla Structures model could not be established.");
            }

            var picker = new Picker();
            while (referenceModel == null) {
                try {
                    referenceModel = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Select a reference model") as ReferenceModel;
                } catch (Exception e) when (e.Message == "User interrupted") {
                    Assert.Inconclusive("Test was cancelled by the user.");
                } catch (Exception e) {
                    Assert.Fail($"An unexpected error occurred: {e.Message}");
                }
            }
        }

        [Test]
        public void TestGetChildren() {
            Console.WriteLine("By GetChildren method:");
            var enumerator = referenceModel.GetChildren();

            if (enumerator == null) {
                Assert.Inconclusive("The reference model has no children.");
            }

            var cnt = -1;
            foreach (ModelObject obj in enumerator) {
                ++cnt;
                Console.WriteLine(
                    string.Format(
                        "{0} - {1}",
                        cnt,
                        obj.GetType().Name
                    )
                );
            }
        }

        [Test]
        public void TestGetConvertedObjects() {
            Console.WriteLine("By GetConvertedObjects method:");
            var enumerator = referenceModel.GetConvertedObjects();

            if (enumerator == null) {
                Assert.Inconclusive("The reference model has no converted objects.");
            }

            var cnt = -1;
            foreach (ModelObject obj in enumerator) {
                ++cnt;
                Console.WriteLine(
                    string.Format(
                        "{0} - {1}",
                        cnt,
                        obj.GetType().Name
                    )
                );
            }
        }

        [Test]
        public void TestGetChildrenThenGetConvertedObjects() {
            Console.WriteLine("By GetChildren then GetConvertedObjects methods:");
            var enumerator = referenceModel.GetChildren();
            if (enumerator == null) {
                Assert.Inconclusive("The reference model has no children.");
            }
            var cnt = -1;
            foreach (ModelObject obj in enumerator) {
                ++cnt;
                Console.WriteLine(
                    string.Format(
                        "{0} - {1}",
                        cnt,
                        obj.GetType().Name
                    )
                );
                if (obj is ReferenceModel refModel) {
                    var subEnumerator = refModel.GetConvertedObjects();
                    if (subEnumerator != null) {
                        foreach (ModelObject subObj in subEnumerator) {
                            Console.WriteLine(
                                string.Format(
                                    "   {0} - {1}",
                                    subObj.Identifier.ID,
                                    subObj.GetType().Name
                                )
                            );
                        }
                    }
                }
            }
        }
    }
}

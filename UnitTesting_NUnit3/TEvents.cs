using System;
using System.Collections.Generic;
using System.Threading;
using Tekla.Structures.Model;

namespace UnitTesting_NUnit3 {
    internal class TEvents {
        private bool interrupted = false;
        private readonly Model model = new();
        [Test]
        public void TestModelObjectChangedEvent() {
            if (!model.GetConnectionStatus()) {
                Assert.Inconclusive("Tekla Structures model connection is not established.");
            }

            var events = new Events();
            events.ModelObjectChanged += OnModelObjectChanged;
            events.Interrupted += OnInterrupted;
            events.SelectionChange += OnSelectionChanged;
            events.Register();

            while (true) {
                if (interrupted) {
                    Console.WriteLine("User interrupted");
                    break;
                }

                Thread.Sleep(1000);
            }
        }

        private void OnSelectionChanged() {
            Console.WriteLine("Selected model object has been changed.");
        }

        private void OnModelObjectChanged(List<ChangeData> changes) {
            foreach (var change in changes) {
                Console.WriteLine(string.Format(
                    "Model object {0} has been changed, identifier is {1}, change type is {2}",
                    change.Object,
                    change.Object.Identifier,
                    change.Type
                ));
            }
        }

        private void OnInterrupted() {
            interrupted = true;
        }
    }
}

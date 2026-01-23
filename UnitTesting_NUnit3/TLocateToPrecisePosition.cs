using System;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace UnitTesting_NUnit3;

public class TLocateToPrecisePosition {
    private readonly Model model = new();
    private readonly Picker picker = new();
    [SetUp]
    public void Setup() {
        if (model.GetConnectionStatus() == false) {
            throw new Exception("Unable to connect to Tekla Structures model.");
        }
    }
    [Test]
    public void TestLocateToPrecisePosition() {
        ModelObjectEnumerator parts;
        List<Beam> beams = [];
        try {
            parts = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
            if (parts == null || parts.GetSize() == 0) {
                throw new Exception("No parts were selected.");
            }

            foreach (var part in parts) {
                if (part is Beam beam) {
                    Move(beam.StartPoint);
                    Move(beam.EndPoint);
                    beam.Modify();

                    beams.Add(beam);
                }
            }

            model.CommitChanges();

            foreach (var beam in beams) {
                MoveBack(beam.StartPoint);
                MoveBack(beam.EndPoint);
                beam.Modify();
            }

            model.CommitChanges();
        } catch (Exception e) when (e.Message == "User interrupt") {
            return;
        } catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    private void Move(Point point) {
        point.X = Math.Round(point.X, 0) + 10;
        point.Y = Math.Round(point.Y, 0) + 10;
        point.Z = Math.Round(point.Z, 0) + 10;
    }

    private void MoveBack(Point point) {
        point.X -= 10;
        point.Y -= 10;
        point.Z -= 10;
    }
}

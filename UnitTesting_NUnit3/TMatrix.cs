using System;
using Muggle.TsExtensions.Common.Geometry3d;
using Tekla.Structures.Geometry3d;

namespace UnitTesting_NUnit3 {
    public class TMatrix {
        Matrix m, r, d;
        [SetUp]
        public void Setup() {
            m = new Matrix();
            m[0, 0] = 0.21201214989666559;
            m[0, 1] = 0.57357643635105393;
            m[0, 2] = -0.62606091995901925;
            m[1, 0] = -0.79124011523621529;
            m[1, 1] = 0;
            m[1, 2] = -0.16775251791571544;
            m[2, 0] = 0;
            m[2, 1] = 0.79124011523621529;
            m[2, 2] = 0.45383668559518564;
            m[3, 0] = 17575;
            m[3, 1] = 4000;
            m[3, 2] = 300;
            r = m.Inverse();

            d = new Matrix();
        }

        [TestCase(1, 2, 3)]
        [TestCase(236, 876, 235)]
        [TestCase(465, 769, 23)]
        [TestCase(5436, 2, 235)]
        [TestCase(46, 0.12, 26)]
        [TestCase(-32, 45, 6)]
        public void TestInverse(double x, double y, double z) {
            var p1 = new Point(x, y, z);
            var p2 = r.Transform(m.Transform(p1));
            Assert.That(p2, Is.EqualTo(p1));
        }

        [Test]
        public void IsIdentity() {
            var c = m * r;
            Console.WriteLine(c);
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 3; j++) {
                    Assert.That(c[i, j], Is.EqualTo(d[i, j]).Within(1e-10));
                }
            }
        }
    }
}

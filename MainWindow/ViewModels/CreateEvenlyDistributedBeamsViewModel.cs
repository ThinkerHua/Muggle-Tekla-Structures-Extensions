/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  CreateEvenlyDistributedBeamsViewModel.cs: view model for the creation of evenly distributed beams.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using ModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace Muggle.TsExtensions.MainWindow.ViewModels;

public partial class CreateEvenlyDistributedBeamsViewModel : ObservableObject {

    /// <summary>
    /// 高差选项
    /// </summary>
    public enum ElevationOptions {

        /// <summary>
        /// 与较低的相同
        /// </summary>
        SameAsLower,

        /// <summary>
        /// 与较高的相同
        /// </summary>
        SameAsHigher,

        /// <summary>
        /// 随坡
        /// </summary>
        FollowSlope,
    }

    [ObservableProperty] private string _name;

    [ObservableProperty] private string _profile;

    [ObservableProperty] private string _material;

    [ObservableProperty] private int _class;

    [ObservableProperty] private string _finish;

    [ObservableProperty] private string _partPrefix;

    [ObservableProperty] private int _partStartNumber;

    [ObservableProperty] private string _assemblyPrefix;

    [ObservableProperty] private int _assemblyStartNumber;

    public double Spacing {
        get;
        set => SetProperty(ref field, value == 0 ? 1000 : Math.Abs(value));
    }

    [ObservableProperty] private ElevationOptions _elevationOption;

    private readonly IMessageBoxService _messageBoxService;

    private readonly Model _model;
    private readonly Picker _picker;
    private readonly ModelObjectSelector _uiSelector;

    public CreateEvenlyDistributedBeamsViewModel(IMessageBoxService messageBoxService) {
        Name = "BEAM";
        Profile = "HM244*175*7*11";
        Material = "Q355B";
        Class = 99;
        Finish = string.Empty;
        PartPrefix = "H";
        PartStartNumber = 1;
        AssemblyPrefix = "GL-";
        AssemblyStartNumber = 1;
        Spacing = 1000.0;
        ElevationOption = ElevationOptions.SameAsLower;

        _messageBoxService = messageBoxService;
        _model = new Model();
        _picker = new Picker();
        _uiSelector = new ModelObjectSelector();
    }

    [RelayCommand]
    private void Action() {
        try {
            if (!_model.GetConnectionStatus()) {
                throw new Exception("无法连接到Tekla模型");
            }

            var selectedObjects = new List<ModelObject>();

            var enumerator = _uiSelector.GetSelectedObjects();

            if (enumerator.GetSize() != 2) {
                enumerator = _picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
                if (enumerator.GetSize() != 2) {
                    throw new Exception("请选择两根平行的主梁");
                }
            }

            while (enumerator.MoveNext()) {
                selectedObjects.Add(enumerator.Current);
            }

            if (selectedObjects[0] is not Beam beam1 || selectedObjects[1] is not Beam beam2) {
                throw new Exception("选择的不是梁对象");
            }

            var s1 = beam1.StartPoint;
            var e1 = beam1.EndPoint;
            var s2 = beam2.StartPoint;
            var e2 = beam2.EndPoint;

            var xDir1 = new Vector(e1 - s1);
            var xDir2 = new Vector(e2 - s2);

            if (!Parallel.VectorToVector(xDir1, xDir2)) {
                throw new Exception("选择的两根梁不平行");
            }

            if (xDir1.Dot(xDir2) < 0) {
                (e2, s2) = (s2, e2);
            }

            var axisZ = new Vector(0, 0, 1000);
            var axisY = axisZ.Cross(xDir1);
            if (axisY.IsZero(GeometryConstants.DISTANCE_EPSILON)) {
                axisZ = new Vector(1000, 0, 0);
                axisY = axisZ.Cross(xDir1);
            }

            axisZ = xDir1.Cross(axisY);

            var xyPlane1 = new GeometricPlane(s1, axisZ);
            var xyPlane2 = new GeometricPlane(s2, axisZ);
            var slopPlane = new GeometricPlane(s1, xDir1, new Vector(s2 - s1));

            var s2Projection = Projection.PointToPlane(s2, xyPlane1);

            var totalYOffset = new Vector(s2Projection - Projection.PointToLine(s2Projection, new Line(s1, e1)));
            var totalZOffset = new Vector(s2 - s2Projection);

            var totalYDist = totalYOffset.GetLength();
            var totalZDist = totalZOffset.GetLength();

            var yOffsetDir = totalYOffset.GetNormal();
            var zOffsetDir = totalZDist <= GeometryConstants.DISTANCE_EPSILON
                ? axisZ.GetNormal()
                : totalZOffset.GetNormal();

            var numberOfSpacings = (int)(totalYDist / Spacing);
            var remainderY = totalYDist - numberOfSpacings * Spacing;

            int numberOfBeams;
            double startOffsetY;
            if (remainderY <= GeometryConstants.DISTANCE_EPSILON) {
                numberOfBeams = numberOfSpacings - 1;
                startOffsetY = Spacing;
            } else {
                numberOfBeams = numberOfSpacings;
                startOffsetY = (remainderY + Spacing) * 0.5;
            }

            var beamLine = new Line(s1 + yOffsetDir * startOffsetY, xDir1);
            var ssPlane = new GeometricPlane(s1, axisZ, new Vector(s2 - s1));
            var eePlane = new GeometricPlane(e1, axisZ, new Vector(e2 - e1));
            var zIntersectionPlane = ElevationOption switch {
                ElevationOptions.SameAsLower => xyPlane1.Origin.Z < xyPlane2.Origin.Z ? xyPlane1 : xyPlane2,
                ElevationOptions.SameAsHigher => xyPlane1.Origin.Z > xyPlane2.Origin.Z ? xyPlane1 : xyPlane2,
                _ => slopPlane
            };
            for (int i = 0; i < numberOfBeams; i++) {
                var beam = new Beam {
                    Name = Name,
                    Profile = { ProfileString = Profile },
                    Material = { MaterialString = Material },
                    Class = Class.ToString(),
                    Finish = Finish,
                    PartNumber = { Prefix = PartPrefix, StartNumber = PartStartNumber },
                    AssemblyNumber = { Prefix = AssemblyPrefix, StartNumber = AssemblyStartNumber },
                    StartPoint = Intersection.LineToPlane(
                        new Line(Intersection.LineToPlane(beamLine, ssPlane), zOffsetDir), zIntersectionPlane),
                    EndPoint = Intersection.LineToPlane(
                        new Line(Intersection.LineToPlane(beamLine, eePlane), zOffsetDir), zIntersectionPlane),
                };

                beam.Insert();

                beamLine.Origin += yOffsetDir * Spacing;
            }

            _model.CommitChanges();

            Operation.DisplayPrompt($"成功创建 {numberOfSpacings} 根次梁");
        } catch (Exception e) {
            _messageBoxService.ShowError(e.Message + e.StackTrace);
        }
    }
}
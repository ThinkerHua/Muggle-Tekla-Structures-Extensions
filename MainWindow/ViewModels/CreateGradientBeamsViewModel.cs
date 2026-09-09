using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using ModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace Muggle.TsExtensions.MainWindow.ViewModels;

public partial class CreateGradientBeamsViewModel : ObservableObject {

    [ObservableProperty] private string _name;

    [ObservableProperty] private string _profile;

    [ObservableProperty] private string _material;

    [ObservableProperty] private int _class;

    [ObservableProperty] private string _finish;

    [ObservableProperty] private string _partPrefix;

    [ObservableProperty] private int _partStartNumber;

    [ObservableProperty] private string _assemblyPrefix;

    [ObservableProperty] private int _assemblyStartNumber;

    [ObservableProperty] private double _spacing;

    [ObservableProperty] private double _minSpacing;

    private readonly IMessageBoxService _messageBoxService;

    private readonly Model _model;
    private readonly Picker _picker;
    private readonly ModelObjectSelector _uiSelector;

    public CreateGradientBeamsViewModel(IMessageBoxService messageBoxService) {
        Name = "BEAM";
        Profile = "HM244*175*7*11";
        Material = "Q355B";
        Class = 99;
        Finish = string.Empty;
        PartPrefix = "H";
        PartStartNumber = 1;
        AssemblyPrefix = "GL-";
        AssemblyStartNumber = 1;
        Spacing = 500.0;
        MinSpacing = 300.0;

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

            var beam1 = selectedObjects[0] as Beam;
            var beam2 = selectedObjects[1] as Beam;

            if (beam1 == null || beam2 == null) {
                throw new Exception("选择的不是梁对象");
            }

            var centerLine1 = beam1.GetCenterLine(false);
            var centerLine2 = beam2.GetCenterLine(false);
            var s1 = centerLine1[0] as Point;
            var e1 = centerLine1[centerLine1.Count - 1] as Point;
            var s2 = centerLine2[0] as Point;
            var e2 = centerLine2[centerLine2.Count - 1] as Point;

            var dir1 = new Vector(e1 - s1);
            var dir2 = new Vector(e2 - s2);

            if (!Parallel.VectorToVector(dir1, dir2)) {
                throw new Exception("选择的两根梁不平行");
            }

            if (dir1.Dot(dir2) < 0) {
                (e2, s2) = (s2, e2);
            }

            var totalDist = Distance.PointToLine(s2, new Line(s1, e1));
            var numberOfSpacing = (int)(totalDist / Spacing);
            var remainder = totalDist - numberOfSpacing * Spacing;

            if (remainder < MinSpacing) {
                numberOfSpacing -= 1;
                remainder += Spacing;
            }

            var startOffset = remainder * 0.5;

            var numberOfBeam = startOffset <= GeometryConstants.DISTANCE_EPSILON
                ? numberOfSpacing - 1
                : numberOfSpacing + 1;
            if (startOffset <= GeometryConstants.DISTANCE_EPSILON) {
                startOffset = Spacing;
            }

            var offsetDir = dir1.Cross(new Vector(s2 - s1)).Cross(dir1).GetNormal();

            var ssLine = new Line(s1, s2);
            var eeLine = new Line(e1, e2);
            var beamLine = new Line(s1 + offsetDir * startOffset, dir1);

            for (int i = 0; i < numberOfBeam; i++) {
                var beam = new Beam {
                    Name = Name,
                    Profile = { ProfileString = Profile },
                    Material = { MaterialString = Material },
                    Class = Class.ToString(),
                    Finish = Finish,
                    PartNumber = { Prefix = PartPrefix, StartNumber = PartStartNumber },
                    AssemblyNumber = { Prefix = AssemblyPrefix, StartNumber = AssemblyStartNumber },
                    StartPoint = Intersection.LineToLine(beamLine, ssLine).StartPoint,
                    EndPoint = Intersection.LineToLine(beamLine, eeLine).StartPoint
                };

                beam.Insert();

                beamLine.Origin += offsetDir * Spacing;
            }

            _model.CommitChanges();

            Operation.DisplayPrompt($"成功创建 {numberOfBeam} 根次梁");
        } catch (Exception e) {
            _messageBoxService.ShowError(e.Message + e.StackTrace);
        }
    }
}
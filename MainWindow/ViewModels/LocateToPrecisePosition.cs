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
 *  LocateToPrecisePosition.cs: view model for the locate to precise position operation.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels;

public partial class LocateToPrecisePositionViewModel : ViewModelBase {
    private const int Correction = 10;
    private static readonly string[] ColumnNames = ["column", "柱"];

    private readonly Model model = new Model();
    private readonly TSMUI.Picker picker = new TSMUI.Picker();
    private readonly TSMUI.ModelObjectSelector uiSelector = new TSMUI.ModelObjectSelector();

    private readonly IMessageBoxService messageBoxService;

    public double Significance {
        get;
        set {
            if (value == 0.0) value = 5.0;
            SetProperty(ref field, value);
        }
    }

    public LocateToPrecisePositionViewModel(IMessageBoxService messageBoxService) {
        this.messageBoxService = messageBoxService;

        Significance = 5.0;
    }

    [RelayCommand]
    private void LocateToPrecisePosition() {
        List<Beam> beams = [], columns = [];
        try {
            if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

            var parts = uiSelector.GetSelectedObjects();
            if (parts == null || parts.GetSize() == 0) {
                parts = picker.PickObjects(TSMUI.Picker.PickObjectsEnum.PICK_N_PARTS);
                if (parts == null || parts.GetSize() == 0) {
                    throw new Exception("No parts were selected.");
                }
            }

            foreach (var part in parts) {
                if (part is not Beam beam) continue;
                var isColumn = beam.Type is Beam.BeamTypeEnum.COLUMN ||
                               ColumnNames.Any(n => beam.Name.Contains(n, StringComparison.InvariantCultureIgnoreCase));

                beam.StartPoint = Move(beam.StartPoint, Significance, isColumn);
                beam.EndPoint = Move(beam.EndPoint, Significance, isColumn);
                beam.Modify();

                if (isColumn) columns.Add(beam); 
                else beams.Add(beam);
            }

            model.CommitChanges();

            foreach (var beam in beams) {
                beam.StartPoint = MoveBack(beam.StartPoint, false);
                beam.EndPoint = MoveBack(beam.EndPoint, false);
                beam.Modify();
            }

            foreach (var column in columns) {
                column.StartPoint = MoveBack(column.StartPoint, true);
                column.EndPoint = MoveBack(column.EndPoint, true);
            }
            
            model.CommitChanges();
        } catch (Exception e) when (e.Message == "User interrupt") {
            // ignore
        } catch (Exception e) {
            messageBoxService.ShowError(e.ToString());
        }
    }

    //  微小的位移会被忽略，需要先移动较大的距离，再移回去
    private static Point Move(Point point, double significance, bool isColumn) {
        return new Point(
            Math.Round(point.X / significance, 0) * significance + Correction,
            Math.Round(point.Y / significance, 0) * significance + Correction,
            isColumn ? point.Z : Math.Round(point.Z / significance, 0) * significance + Correction);
    }

    private static Point MoveBack(Point point, bool isColumn) {
        return new Point(point.X - Correction, point.Y - Correction, isColumn ? point.Z : point.Z - Correction);
    }
}
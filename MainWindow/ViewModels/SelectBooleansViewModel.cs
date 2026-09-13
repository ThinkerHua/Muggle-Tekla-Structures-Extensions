/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2025 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  SelectBooleansViewModel.cs: view model for the SelectBooleans tool.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using TSM = Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class SelectBooleansViewModel : ViewModelBase {
        [Flags]
        public enum BooleanFilters {
            None = 0,
            Add = 1 << 0,
            Cut = 1 << 1,
            WeldPrep = 1 << 2,
            CutPlane = 1 << 3,
            EdgeChamfer = 1 << 4,
            Fitting = 1 << 5,
            All = -1
        }

        private readonly Model model = new Model();
        private readonly Picker picker = new Picker();
        private readonly TSMUI.ModelObjectSelector uiSelector = new TSMUI.ModelObjectSelector();

        private readonly IMessageBoxService messageBoxService;

        [ObservableProperty]
        private bool matchBooleanAdd = false;

        [ObservableProperty]
        private bool matchBooleanCut = true;

        [ObservableProperty]
        private bool matchWeldPrep = false;

        [ObservableProperty]
        private bool matchCutPlane = false;

        [ObservableProperty]
        private bool matchEdgeChamfer = false;

        [ObservableProperty]
        private bool matchFitting = false;

        public BooleanFilters Filter => BooleanFilters.None |
                (MatchBooleanAdd ? BooleanFilters.Add : BooleanFilters.None) |
                (MatchBooleanCut ? BooleanFilters.Cut : BooleanFilters.None) |
                (MatchWeldPrep ? BooleanFilters.WeldPrep : BooleanFilters.None) |
                (MatchCutPlane ? BooleanFilters.CutPlane : BooleanFilters.None) |
                (MatchEdgeChamfer ? BooleanFilters.EdgeChamfer : BooleanFilters.None) |
                (MatchFitting ? BooleanFilters.Fitting : BooleanFilters.None);

        public SelectBooleansViewModel(IMessageBoxService messageBoxService) {
            this.messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void SelectBooleans() {

            Part part;
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

                part = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART) as Part;
            } catch (Exception e) when (e.Message == App.UserInterrupt) {
                return;
            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
                return;
            }

            if (part is null) return;

            var objects = new ArrayList();
            var objectEnumerator = part.GetBooleans();
            BooleanFilters type;
            foreach (TSM.Boolean obj in objectEnumerator) {
                if (obj is BooleanPart booleanPart) {
                    type = booleanPart.Type switch {
                        BooleanPart.BooleanTypeEnum.BOOLEAN_ADD => BooleanFilters.Add,
                        BooleanPart.BooleanTypeEnum.BOOLEAN_CUT => BooleanFilters.Cut,
                        BooleanPart.BooleanTypeEnum.BOOLEAN_WELDPREP => BooleanFilters.WeldPrep,
                        _ => BooleanFilters.None,
                    };
                } else if (obj is CutPlane) {
                    type = BooleanFilters.CutPlane;
                } else if (obj is EdgeChamfer) {
                    type = BooleanFilters.EdgeChamfer;
                } else if (obj is Fitting) {
                    type = BooleanFilters.Fitting;
                } else {
                    type = BooleanFilters.None;
                }

                if (Filter.HasFlag(type)) {
                    objects.Add(obj);
                }
            }

            if (objects.Count == 0) return;
            uiSelector.Select(objects, false);
            model.CommitChanges();
        }
    }
}

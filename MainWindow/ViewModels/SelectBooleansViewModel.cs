/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
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
        public enum BooleanTypeEnum {
            None = 0,
            Add = 1 << 0,
            Cut = 1 << 1,
            WELDPREP = 1 << 2,
            CUTPLANE = 1 << 3,
            EDGECHAMFER = 1 << 4,
            FITTING = 1 << 5,
            ALL = -1
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

        public BooleanTypeEnum BooleanType => BooleanTypeEnum.None |
                (MatchBooleanAdd ? BooleanTypeEnum.Add : BooleanTypeEnum.None) |
                (MatchBooleanCut ? BooleanTypeEnum.Cut : BooleanTypeEnum.None) |
                (MatchWeldPrep ? BooleanTypeEnum.WELDPREP : BooleanTypeEnum.None) |
                (MatchCutPlane ? BooleanTypeEnum.CUTPLANE : BooleanTypeEnum.None) |
                (MatchEdgeChamfer ? BooleanTypeEnum.EDGECHAMFER : BooleanTypeEnum.None) |
                (MatchFitting ? BooleanTypeEnum.FITTING : BooleanTypeEnum.None);

        public SelectBooleansViewModel(IMessageBoxService messageBoxService) {
            this.messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void SelectBooleans() {

            Part part;
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NOT_CONNECTED);

                part = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART) as Part;
            } catch (Exception e) when (e.Message == App.USER_INTERRUPT) {
                return;
            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
                return;
            }

            if (part is null) return;

            var objects = new ArrayList();
            var objectEnumerator = part.GetBooleans();
            BooleanTypeEnum type;
            foreach (TSM.Boolean obj in objectEnumerator) {
                if (obj is BooleanPart booleanPart) {
                    type = booleanPart.Type switch {
                        BooleanPart.BooleanTypeEnum.BOOLEAN_ADD => BooleanTypeEnum.Add,
                        BooleanPart.BooleanTypeEnum.BOOLEAN_CUT => BooleanTypeEnum.Cut,
                        BooleanPart.BooleanTypeEnum.BOOLEAN_WELDPREP => BooleanTypeEnum.WELDPREP,
                        _ => BooleanTypeEnum.None,
                    };
                } else if (obj is CutPlane) {
                    type = BooleanTypeEnum.CUTPLANE;
                } else if (obj is EdgeChamfer) {
                    type = BooleanTypeEnum.EDGECHAMFER;
                } else if (obj is Fitting) {
                    type = BooleanTypeEnum.FITTING;
                } else {
                    type = BooleanTypeEnum.None;
                }

                if (BooleanType.HasFlag(type)) {
                    objects.Add(obj);
                }
            }

            if (objects.Count == 0) return;
            uiSelector.Select(objects, false);
            model.CommitChanges();
        }
    }
}

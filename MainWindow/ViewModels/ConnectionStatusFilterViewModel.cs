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
 *  ConnectionStatusFilterViewModel.cs: view model for the ConnectionStatusFilter tool.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using TSM = Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class ConnectionStatusFilterViewModel : ViewModelBase {
        [Flags]
        public enum ConnectionStatusFilterFlag {
            GreenSymbol = 1,
            YellowSymbol = 2,
            RedSymbol = 4,
            NotPassDesignCheck = 8
        }

        private readonly Model model;
        private readonly TSM.ModelObjectSelector selector;
        private readonly TSMUI.ModelObjectSelector uiSelector;
        private readonly Picker picker;
        private readonly IMessageBoxService messageBoxService;

        [ObservableProperty]
        private bool filterGreenSymbol = false;

        [ObservableProperty]
        private bool filterYellowSymbol = false;

        [ObservableProperty]
        private bool filterRedSymbol = true;

        [ObservableProperty]
        private bool filterNotPassDesignCheck = true;

        [ObservableProperty]
        private bool manualSelection = false;

        private ConnectionStatusFilterFlag Filter =>
            (FilterGreenSymbol ? ConnectionStatusFilterFlag.GreenSymbol : 0) |
            (FilterYellowSymbol ? ConnectionStatusFilterFlag.YellowSymbol : 0) |
            (FilterRedSymbol ? ConnectionStatusFilterFlag.RedSymbol : 0) |
            (FilterNotPassDesignCheck ? ConnectionStatusFilterFlag.NotPassDesignCheck : 0);

        public ConnectionStatusFilterViewModel(IMessageBoxService messageBoxService) {
            model = new Model();
            selector = model.GetModelObjectSelector();
            uiSelector = new TSMUI.ModelObjectSelector();
            picker = new Picker();

            this.messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void ApplyFilter() {
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

                var objectEnumerator = uiSelector.GetSelectedObjects();
                if (objectEnumerator.GetSize() == 0) {
                    if (ManualSelection) {
                        objectEnumerator = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_OBJECTS);
                    } else {
                        objectEnumerator = selector.GetAllObjectsWithType([typeof(Connection), typeof(Detail), typeof(Seam)]);
                    }
                }

                var objects = new ArrayList();

                if (objectEnumerator.GetSize() == 0) {
                    uiSelector.Select(objects);
                    return;
                }

                foreach (var obj in objectEnumerator) {
                    if (obj is Connection connection && IsMatchStatus(connection.Status) ||
                        obj is Detail detail && IsMatchStatus(detail.Status) ||
                        obj is Seam seam && IsMatchStatus(seam.Status)) {
                        objects.Add(obj);
                    }
                }

                uiSelector.Select(objects);
            } catch (Exception e) when (e.Message == App.UserInterrupt) {

            } catch (Exception e) {
                messageBoxService.ShowError(e.Message);
            }
        }

        private bool IsMatchStatus(ConnectionStatusEnum status) {
            return status == ConnectionStatusEnum.STATUS_OK && Filter.HasFlag(ConnectionStatusFilterFlag.GreenSymbol) ||
                   status == ConnectionStatusEnum.STATUS_WARNING && Filter.HasFlag(ConnectionStatusFilterFlag.YellowSymbol) ||
                   status == ConnectionStatusEnum.STATUS_ERROR && Filter.HasFlag(ConnectionStatusFilterFlag.RedSymbol) ||
                   status == ConnectionStatusEnum.STATUS_UNKNOWN && Filter.HasFlag(ConnectionStatusFilterFlag.NotPassDesignCheck);
        }
    }
}

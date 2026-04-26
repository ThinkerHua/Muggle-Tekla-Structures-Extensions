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
 *  PluginsViewModel.cs: view model for the plugins.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures;
using Tekla.Structures.Dialog;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Assembly = System.Reflection.Assembly;
using TSMUI = Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public class PluginCard {
        public string PluginName { get; set; }
        public BitmapImage Icon { get; set; }
    }

    public partial class PluginsViewModel : ViewModelBase {
        private readonly Model model = new Model();
        private readonly Picker picker = new Picker();
        private readonly TSMUI.ModelObjectSelector uiSelector = new TSMUI.ModelObjectSelector();
        /// <summary>
        /// key - plugin name, value - plugin assembly
        /// </summary>
        private readonly Dictionary<string, Assembly> plugins = new Dictionary<string, Assembly>();

        private readonly IMessageBoxService messageBoxService;

        private readonly Localization localization = App.Current.Localization;

        public PluginsViewModel(IMessageBoxService messageBoxService) {
            this.messageBoxService = messageBoxService;
        }

        private void ReloadPlugins() {
            plugins.Clear();
            var extensionsPath = AppDomain.CurrentDomain.BaseDirectory;
            var dlls = Directory.GetFiles(extensionsPath, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var dll in dlls) {
                var assembly = Assembly.LoadFile(dll);
                var types = assembly.GetTypes();
                foreach (var type in types) {
                    var attribute = type.GetCustomAttribute<PluginAttribute>();
                    if (attribute == null) continue;

                    plugins[attribute.Name] = assembly;
                }
            }
        }

        [RelayCommand]
        private void RepeatRunPlugin(string pluginName) {
            try {
                if (!plugins.ContainsKey(pluginName)) ReloadPlugins();
                if (!plugins.ContainsKey(pluginName))
                    throw new Exception(string.Format("Failed to run plugin \"{0}\", which may not exist.", pluginName));

                while (true) {
                    RunPlugin(pluginName);
                }
            } catch (Exception e) when (e.Message == App.USER_INTERRUPT) {

            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
            }
        }

        private void RunPlugin(string pluginName) {

            if (pluginName == "WK1001") {
                RunWK1001();
                return;
            }

            BaseComponent baseComponent = null;
            try {
                var assembly = plugins[pluginName];
                var pluginType = assembly.GetTypes().First(type => {
                    var attr = type.GetCustomAttribute<PluginAttribute>();
                    return attr != null && attr.Name == pluginName;
                });

                Attribute attribute;

                var secondaryType = ConnectionBase.SecondaryType.SECONDARYTYPE_ONE;
                var autoDirectionType = AutoDirectionTypeEnum.AUTODIR_DETAIL;
                var positionType = PositionTypeEnum.COLLISION_PLANE;
                var detailType = DetailTypeEnum.END;
                var seamInputType = ConnectionBase.SeamInputType.INPUT_POLYGON;
                var customPartInputType = CustomPartBase.CustomPartInputType.INPUT_1_POINT;
                //var customPartPositioningType = CustomPartBase.CustomPartPositioningType.POSITIONING_BY_CENTER_OF_BOUNDING_BOX;
                //var inputObjectDependency = PluginBase.InputObjectDependency.NOT_DEPENDENT;
                //var pluginCoordinateSystem = PluginBase.CoordinateSystemType.FROM_FIRST_POINT_AND_GLOBAL;
                //var pluginSymbolVisibility = PluginBase.SymbolVisibility.DRAW_SYMBOL;
                var baseType = pluginType.BaseType;
                if (baseType.Equals(typeof(ConnectionBase))) {
                    attribute = pluginType.GetCustomAttribute<SecondaryTypeAttribute>();
                    if (attribute != null) secondaryType = ((SecondaryTypeAttribute)attribute).Type;

                    attribute = pluginType.GetCustomAttribute<AutoDirectionTypeAttribute>();
                    if (attribute != null) autoDirectionType = ((AutoDirectionTypeAttribute)attribute).Type;

                    attribute = pluginType.GetCustomAttribute<PositionTypeAttribute>();
                    if (attribute != null) positionType = ((PositionTypeAttribute)attribute).Type;

                    if (secondaryType == ConnectionBase.SecondaryType.SECONDARYTYPE_ZERO) {
                        //  Detail
                        attribute = pluginType.GetCustomAttribute<DetailTypeAttribute>();
                        if (attribute != null) detailType = ((DetailTypeAttribute)attribute).Type;

                        baseComponent = CreatDetail(pluginName, autoDirectionType, detailType);
                    } else {
                        attribute = pluginType.GetCustomAttribute<SeamInputTypeAttribute>();
                        if (attribute != null) {
                            //  Seam
                            seamInputType = ((SeamInputTypeAttribute)attribute).Type;

                            baseComponent = CreatSeam(pluginName, secondaryType, autoDirectionType, seamInputType);
                        } else {
                            //  Connection

                            baseComponent = CreatConnection(pluginName, secondaryType, autoDirectionType, positionType);
                        }
                    }
                } else if (baseType.Equals(typeof(CustomPartBase))) {
                    //  CustomPart
                    attribute = pluginType.GetCustomAttribute<CustomPartInputTypeAttribute>();
                    if (attribute != null) customPartInputType = ((CustomPartInputTypeAttribute)attribute).Type;

                    //attribute = pluginType.GetCustomAttribute<CustomPartPositioningTypeAttribute>();
                    //if (attribute != null) customPartPositioningType = ((CustomPartPositioningTypeAttribute) attribute).Type;

                    baseComponent = CreatCustomPart(pluginName, customPartInputType);
                } else if (baseType.Equals(typeof(PluginBase))) {
                    //  Component

                    //Assembly.CreatInstance 不适用于没有无参数构造函数的类
                    //var instance = pluginType.Assembly.CreateInstance(pluginType.FullName) as PluginBase;
                    var instance = FormatterServices.GetUninitializedObject(pluginType) as PluginBase;
                    var inputDefinitions = instance.DefineInput();

                    baseComponent = CreatComponent(pluginName, inputDefinitions);
                } else {
                    throw new Exception(string.Format("Unsupported type: \"{0}\"", baseType));
                }

            } catch {
                throw;
            } finally {
                if (baseComponent != null) {
                    uiSelector.Select(new ArrayList { baseComponent });
                    model.CommitChanges();
                }
            }
        }

        private Detail CreatDetail(string pluginName, AutoDirectionTypeEnum autoDirectionType, DetailTypeEnum detailType) {
            var mo = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, localization.GetText("prompt_Pick_main_part"));
            var p = picker.PickPoint(localization.GetText("prompt_Pick_position"));
            var detail = new Detail {
                Name = pluginName,
                Number = BaseComponent.PLUGIN_OBJECT_NUMBER,
                AutoDirectionType = autoDirectionType,
                DetailType = detailType,
                Class = -1
            };
            detail.LoadAttributesFromFile("standard");
            detail.SetPrimaryObject(mo);
            detail.SetReferencePoint(p);

            try {
                if (detail.Insert()) return detail;
                return null;
            } catch {
                throw;
            }
        }

        private Seam CreatSeam(string pluginName, ConnectionBase.SecondaryType secondaryType, AutoDirectionTypeEnum autoDirectionType, ConnectionBase.SeamInputType seamInputType) {

            var primPart = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, localization.GetText("prompt_Pick_main_part"));

            ModelObject secondaryPart = null;
            ArrayList secondaryParts = new ArrayList();
            Point p1 = null, p2 = null;
            ArrayList points = null;
            switch (secondaryType) {
            case ConnectionBase.SecondaryType.SECONDARYTYPE_ONE:
                secondaryPart = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, localization.GetText("prompt_Pick_secondary_part"));
                break;
            case ConnectionBase.SecondaryType.SECONDARYTYPE_TWO:
                for (int i = 0; i < 2; i++) {
                    secondaryPart = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, localization.GetText("prompt_Pick_secondary_parts"));
                    secondaryParts.Add(secondaryPart);
                }
                break;
            case ConnectionBase.SecondaryType.SECONDARYTYPE_MULTIPLE:
                var enumerator = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, localization.GetText("prompt_Pick_secondary_parts"));
                foreach (ModelObject obj in enumerator) {
                    secondaryParts.Add(obj);
                }
                break;
            default:
                break;
            }

            switch (seamInputType) {
            case ConnectionBase.SeamInputType.INPUT_2_POINTS:
                p1 = picker.PickPoint(localization.GetText("prompt_Pick_first_position"));
                p2 = picker.PickPoint(localization.GetText("prompt_Pick_second_position"), p1);
                break;
            case ConnectionBase.SeamInputType.INPUT_POLYGON:
                points = picker.PickPoints(Picker.PickPointEnum.PICK_POLYGON, localization.GetText("prompt_Pick_polygon"));
                break;
            default:
                break;
            }

            var seam = new Seam {
                Name = pluginName,
                Number = BaseComponent.PLUGIN_OBJECT_NUMBER,
                AutoDirectionType = autoDirectionType,
                AutoPosition = true,
                Class = -1
            };
            seam.LoadAttributesFromFile("standard");

            seam.SetPrimaryObject(primPart);
            switch (secondaryType) {
            case ConnectionBase.SecondaryType.SECONDARYTYPE_ONE:
                seam.SetSecondaryObject(secondaryPart);
                break;
            default:
                seam.SetSecondaryObjects(secondaryParts);
                break;
            }

            switch (seamInputType) {
            case ConnectionBase.SeamInputType.INPUT_2_POINTS:
                seam.SetInputPositions(p1, p2);
                break;
            default:
                seam.SetInputPolygon(new Polygon { Points = points });
                break;
            }

            try {
                if (seam.Insert()) return seam;
                return null;
            } catch {
                throw;
            }
        }

        private Connection CreatConnection(string pluginName, ConnectionBase.SecondaryType secondaryType, AutoDirectionTypeEnum autoDirectionType, PositionTypeEnum positionType) {

            var primPart = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, localization.GetText("prompt_Pick_main_part"));

            ModelObject secondaryPart = null;
            var secondaryParts = new ArrayList();
            switch (secondaryType) {
            case ConnectionBase.SecondaryType.SECONDARYTYPE_ONE:
                secondaryPart = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, localization.GetText("prompt_Pick_secondary_part"));
                break;
            case ConnectionBase.SecondaryType.SECONDARYTYPE_TWO:
                for (int i = 0; i < 2; i++) {
                    secondaryPart = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, localization.GetText("prompt_Pick_secondary_parts"));
                    secondaryParts.Add(secondaryPart);
                }
                break;
            case ConnectionBase.SecondaryType.SECONDARYTYPE_MULTIPLE:
            default:
                var enumerator = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, localization.GetText("prompt_Pick_secondary_parts"));
                foreach (ModelObject obj in enumerator) {
                    secondaryParts.Add(obj);
                }
                break;
            }

            var connection = new Connection {
                Name = pluginName,
                Number = BaseComponent.PLUGIN_OBJECT_NUMBER,
                AutoDirectionType = autoDirectionType,
                PositionType = positionType,
                Class = -1
            };
            connection.LoadAttributesFromFile("standard");

            connection.SetPrimaryObject(primPart);
            switch (secondaryType) {
            case ConnectionBase.SecondaryType.SECONDARYTYPE_ONE:
                connection.SetSecondaryObject(secondaryPart);
                break;
            default:
                connection.SetSecondaryObjects(secondaryParts);
                break;
            }

            try {
                if (connection.Insert()) return connection;
                return null;
            } catch {
                throw;
            }
        }

        private CustomPart CreatCustomPart(string pluginName, CustomPartBase.CustomPartInputType customPartInputType) {
            Point p1 = null, p2 = null;
            switch (customPartInputType) {
            case CustomPartBase.CustomPartInputType.INPUT_2_POINTS:
                p1 = picker.PickPoint(localization.GetText("prompt_Pick_first_position"));
                p2 = picker.PickPoint(localization.GetText("prompt_Pick_second_position"), p1);
                break;
            case CustomPartBase.CustomPartInputType.INPUT_1_POINT:
                p1 = picker.PickPoint(localization.GetText("prompt_Pick_position"));
                break;
            default:
                break;
            }

            var customPart = new CustomPart {
                Name = pluginName,
                Number = BaseComponent.PLUGIN_OBJECT_NUMBER
            };
            customPart.LoadAttributesFromFile("standard");

            switch (customPartInputType) {
            case CustomPartBase.CustomPartInputType.INPUT_1_POINT:
                customPart.SetInputPositions(p1, p1);
                break;
            default:
                customPart.SetInputPositions(p1, p2);
                break;
            }

            try {
                if (customPart.Insert()) return customPart;
                return null;
            } catch {
                throw;
            }
        }

        /// <summary>
        /// Creates and inserts a new component with the specified name and input definitions.
        /// </summary>
        /// <remarks>This method processes the provided input definitions to configure the component's
        /// inputs. Supported input types include points, polygons, and objects. The method ensures that the component
        /// is properly initialized and inserted into the model.</remarks>
        /// <param name="name">The name of the component to be created.</param>
        /// <param name="inputDefinitions">A list of input definitions that specify the inputs required for the component. Each input definition
        /// determines the type and structure of the input data.</param>
        /// <returns>The newly created <see cref="Component"/> instance.</returns>
        private Component CreatComponent(string name, List<PluginBase.InputDefinition> inputDefinitions) {
            var input = new ComponentInput();
            foreach (var inputDefinition in inputDefinitions) {
                switch (inputDefinition.GetInputType()) {
                case PluginBase.InputDefinition.InputTypeEnum.INPUT_ONE_POINT:
                    input.AddOneInputPosition(inputDefinition.GetInput() as Point);
                    break;
                case PluginBase.InputDefinition.InputTypeEnum.INPUT_TWO_POINTS:
                    var twoPoints = inputDefinition.GetInput() as ArrayList;
                    input.AddTwoInputPositions(twoPoints[0] as Point, twoPoints[1] as Point);
                    break;
                case PluginBase.InputDefinition.InputTypeEnum.INPUT_POLYGON:
                    var polygon = inputDefinition.GetInput() as ArrayList;
                    input.AddInputPolygon(new Polygon { Points = polygon });
                    break;
                case PluginBase.InputDefinition.InputTypeEnum.INPUT_ONE_OBJECT:
                    var identifer = inputDefinition.GetInput() as Identifier;
                    input.AddInputObject(model.SelectModelObject(identifer));
                    break;
                case PluginBase.InputDefinition.InputTypeEnum.INPUT_N_OBJECTS:
                    var identifers = inputDefinition.GetInput() as ArrayList;
                    var objects = new ArrayList();
                    foreach (Identifier id in identifers) {
                        objects.Add(model.SelectModelObject(id));
                    }
                    input.AddInputObjects(objects);
                    break;
                default:
                    break;
                }
            }
            var component = new Component {
                Name = name,
                Number = BaseComponent.PLUGIN_OBJECT_NUMBER
            };
            component.LoadAttributesFromFile("standard");
            component.SetComponentInput(input);

            try {
                if (component.Insert()) return component;
                return null;
            } catch {
                throw;
            }
        }
    }
}

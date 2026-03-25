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
 *  ViewModelPropertiesGenerator.cs: help to generate properties (with "StructuresDialogAttribute")
 *  for view model which used by plugin WPF UI.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Muggle.TsExtensions.CodingHelper.Diagnosers;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    [Generator]
    internal class ViewModelPropertiesGenerator : IIncrementalGenerator {
        private static readonly string[] ConcernedAttributes = [
            "Muggle.TsExtensions.CodingHelper.Generators.PartPropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PlatePropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.WeldPropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltPropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltCirclePropertiesAttribute"
        ];

        #region Initial files

        private const string NotificationObject =
            """
            using System.ComponentModel;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// A base class that can send notifications when its properties change.
                /// </summary>
                public abstract class NotificationObject : INotifyPropertyChanged {
                    
                    public event PropertyChangedEventHandler PropertyChanged;
                    
                    protected void OnPropertyChanged(string name) {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                    }
                }
                
            }
            """;

        private const string ConnectionViewModel =
            """
            using Tekla.Structures.Dialog;
            using TD = Tekla.Structures.Datatype;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// A view model base suitable for connection plugin of Tekla Structures,
                /// include several commonly used properties.
                /// Inherits from <see cref="NotificationObject"/>, 
                /// so it can send notifications when its properties changed.
                /// </summary>
                public abstract class ConnectionViewModel : NotificationObject {
                    
                    private int upDirection = 7;
                    [StructuresDialog("zsuunta", typeof(TD.Integer))]
                    public int UpDirection {
                        get { return upDirection; }
                        set {
                            upDirection = value <= 0 || value > 7 ? 7 : value;
                            OnPropertyChanged("UpDirection");
                        }
                    }
                    
                    private double rotationAngleY = 0.0;
                    [StructuresDialog("zang1", typeof(TD.Double))]
                    public double RotationAngleY {
                        get { return rotationAngleY; }
                        set {
                            rotationAngleY = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("RotationAngleY");
                        }
                    }
                    
                    private double rotationAngleX = 0.0;
                    [StructuresDialog("zang2", typeof(TD.Double))]
                    public double RotationAngleX {
                        get { return rotationAngleX; }
                        set {
                            rotationAngleX = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("RotationAngleX");
                        }
                    }
                    
                    private int locked = 0;
                    [StructuresDialog("OBJECT_LOCKED", typeof(TD.Integer))]
                    public int Locked {
                        get { return locked; }
                        set {
                            locked = value == 1 ? 1 : 0;
                            OnPropertyChanged("Locked");
                        }
                    }
                    
                    private int @class = -1;
                    [StructuresDialog("group_no", typeof(TD.Integer))]
                    public int Class {
                        get { return @class; }
                        set {
                            @class = value == int.MinValue ? 0 : value;
                            OnPropertyChanged("Class");
                        }
                    }
                    
                    private string connectionCode = string.Empty;
                    [StructuresDialog("joint_code", typeof(TD.String))]
                    public string ConnectionCode {
                        get { return connectionCode; }
                        set {
                            connectionCode = value ?? string.Empty;
                            OnPropertyChanged("ConnectionCode");
                        }
                    }
                    
                    private string autoDefaults = string.Empty;
                    [StructuresDialog("ad_root", typeof(TD.String))]
                    public string AutoDefaults {
                        get { return autoDefaults; }
                        set {
                            autoDefaults = value ?? string.Empty;
                            OnPropertyChanged("AutoDefaults");
                        }
                    }
                    
                    private string autoConnection = string.Empty;
                    [StructuresDialog("ac_root", typeof(TD.String))]
                    public string AutoConnection {
                        get { return autoConnection; }
                        set {
                            autoConnection = value ?? string.Empty;
                            OnPropertyChanged("AutoConnection");
                        }
                    }
                }
                
            }
            """;

        private const string DetailViewModel =
            """
            using Tekla.Structures.Dialog;
            using TD = Tekla.Structures.Datatype;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// A view model base suitable for detail plugin of Tekla Structures,
                /// include several commonly used properties.
                /// Inherits from <see cref="NotificationObject"/>, 
                /// so it can send notifications when its properties changed.
                /// </summary>
                public abstract class DetailViewModel : NotificationObject {
                    
                    private int upDirection = 7;
                    [StructuresDialog("zsuunta", typeof(TD.Integer))]
                    public int UpDirection {
                        get { return upDirection; }
                        set {
                            upDirection = value <= 0 || value > 7 ? 7 : value;
                            OnPropertyChanged("UpDirection");
                        }
                    }
                    
                    private double rotationAngleY = 0.0;
                    [StructuresDialog("zang1", typeof(TD.Double))]
                    public double RotationAngleY {
                        get { return rotationAngleY; }
                        set {
                            rotationAngleY = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("RotationAngleY");
                        }
                    }
                    
                    private double rotationAngleX = 0.0;
                    [StructuresDialog("zang2", typeof(TD.Double))]
                    public double RotationAngleX {
                        get { return rotationAngleX; }
                        set {
                            rotationAngleX = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("RotationAngleX");
                        }
                    }
                    
                    private int vertical_position = 0;
                    [StructuresDialog("vertical_position", typeof(TD.Integer))]
                    public int VerticalPosition {
                        get { return vertical_position; }
                        set {
                            vertical_position = value < -1 || value > 2 ? 0 : value;
                            OnPropertyChanged("VerticalPosition");
                        }
                    }
                    
                    private int horizontal_position = 0;
                    [StructuresDialog("horizontal_position", typeof(TD.Integer))]
                    public int HorizontalPosition {
                        get { return horizontal_position; }
                        set {
                            horizontal_position = value < -1 || value > 2 ? 0 : value;
                            OnPropertyChanged("HorizontalPosition");
                        }
                    }
                    
                    private double vertical_offset = 0.0;
                    [StructuresDialog("vertical_offset", typeof(TD.Double))]
                    public double VerticalOffset {
                        get { return vertical_offset; }
                        set {
                            vertical_offset = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("VerticalOffset");
                        }
                    }
                    
                    private double horizontal_offset = 0.0;
                    [StructuresDialog("horizontal_offset", typeof(TD.Double))]
                    public double HorizontalOffset {
                        get { return horizontal_offset; }
                        set {
                            horizontal_offset = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("HorizontalOffset");
                        }
                    }
                    
                    private int upMiddleLeft = 0;
                    [StructuresDialog("UpMiddleLeft", typeof(TD.Integer))]
                    public int UpMiddleLeft {
                        get { return upMiddleLeft; }
                        set {
                            upMiddleLeft = value == 1 ? 1 : 0;
                            OnPropertyChanged("UpMiddleLeft");

                            if (value == 1) {
                                UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 2; HorizontalPosition = 1;
                            }
                        }
                    }
                    
                    private int upMiddleMiddle = 0;
                    [StructuresDialog("UpMiddleMiddle", typeof(TD.Integer))]
                    public int UpMiddleMiddle {
                        get { return upMiddleMiddle; }
                        set {
                            upMiddleMiddle = value == 1 ? 1 : 0;
                            OnPropertyChanged("UpMiddleMiddle");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 2; HorizontalPosition = 2;
                            }
                        }
                    }
                    
                    private int upMiddleRight = 0;
                    [StructuresDialog("UpMiddleRight", typeof(TD.Integer))]
                    public int UpMiddleRight {
                        get { return upMiddleRight; }
                        set {
                            upMiddleRight = value == 1 ? 1 : 0;
                            OnPropertyChanged("UpMiddleRight");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 2; HorizontalPosition = -1;
                            }
                        }
                    }
                    
                    private int topLeft = 0;
                    [StructuresDialog("TopLeft", typeof(TD.Integer))]
                    public int TopLeft {
                        get { return topLeft; }
                        set {
                            topLeft = value == 1 ? 1 : 0;
                            OnPropertyChanged("TopLeft");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 1; HorizontalPosition = 1;
                            }
                        }
                    }
                    
                    private int topMiddle = 0;
                    [StructuresDialog("TopMiddle", typeof(TD.Integer))]
                    public int TopMiddle {
                        get { return topMiddle; }
                        set {
                            topMiddle = value == 1 ? 1 : 0;
                            OnPropertyChanged("TopMiddle");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 1; HorizontalPosition = 0;
                            }
                        }
                    }
                    
                    private int topRight = 0;
                    [StructuresDialog("TopRight", typeof(TD.Integer))]
                    public int TopRight {
                        get { return topRight; }
                        set {
                            topRight = value == 1 ? 1 : 0;
                            OnPropertyChanged("TopRight");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 1; HorizontalPosition = -1;
                            }
                        }
                    }
                    
                    private int middleLeft = 0;
                    [StructuresDialog("MiddleLeft", typeof(TD.Integer))]
                    public int MiddleLeft {
                        get { return middleLeft; }
                        set {
                            middleLeft = value == 1 ? 1 : 0;
                            OnPropertyChanged("MiddleLeft");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 0; HorizontalPosition = 1;
                            }
                        }
                    }
                    
                    private int middleMiddle = 1;
                    [StructuresDialog("MiddleMiddle", typeof(TD.Integer))]
                    public int MiddleMiddle {
                        get { return middleMiddle; }
                        set {
                            middleMiddle = value == 1 ? 1 : 0;
                            OnPropertyChanged("MiddleMiddle");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 0; HorizontalPosition = 0;
                            }
                        }
                    }
                    
                    private int middleRight = 0;
                    [StructuresDialog("MiddleRight", typeof(TD.Integer))]
                    public int MiddleRight {
                        get { return middleRight; }
                        set {
                            middleRight = value == 1 ? 1 : 0;
                            OnPropertyChanged("MiddleRight");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0;
                                BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = 0; HorizontalPosition = -1;
                            }
                        }
                    }
                    
                    private int bottomLeft = 0;
                    [StructuresDialog("BottomLeft", typeof(TD.Integer))]
                    public int BottomLeft {
                        get { return bottomLeft; }
                        set {
                            bottomLeft = value == 1 ? 1 : 0;
                            OnPropertyChanged("BottomLeft");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomMiddle = 0; BottomRight = 0;

                                VerticalPosition = -1; HorizontalPosition = 1;
                            }
                        }
                    }
                    
                    private int bottomMiddle = 0;
                    [StructuresDialog("BottomMiddle", typeof(TD.Integer))]
                    public int BottomMiddle {
                        get { return bottomMiddle; }
                        set {
                            bottomMiddle = value == 1 ? 1 : 0;
                            OnPropertyChanged("BottomMiddle");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomRight = 0;

                                VerticalPosition = -1; HorizontalPosition = 0;
                            }
                        }
                    }
                    
                    private int bottomRight = 0;
                    [StructuresDialog("BottomRight", typeof(TD.Integer))]
                    public int BottomRight {
                        get { return bottomRight; }
                        set {
                            bottomRight = value == 1 ? 1 : 0;
                            OnPropertyChanged("BottomRight");

                            if (value == 1) {
                                UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                                TopLeft = 0; TopMiddle = 0; TopRight = 0;
                                MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                                BottomLeft = 0; BottomMiddle = 0;

                                VerticalPosition = -1; HorizontalPosition = -1;
                            }
                        }
                    }
                    
                    private int detail_type = 0;
                    [StructuresDialog("detail_type", typeof(TD.Integer))]
                    public int DetailType {
                        get { return detail_type; }
                        set {
                            detail_type = value < 0 || value > 2 ? 0 : value;
                            OnPropertyChanged("DetailType");
                        }
                    }
                    
                    private int locked = 0;
                    [StructuresDialog("OBJECT_LOCKED", typeof(TD.Integer))]
                    public int Locked {
                        get { return locked; }
                        set {
                            locked = value == 1 ? 1 : 0;
                            OnPropertyChanged("Locked");
                        }
                    }
                    
                    private int @class = -1;
                    [StructuresDialog("group_no", typeof(TD.Integer))]
                    public int Class {
                        get { return @class; }
                        set {
                            @class = value == int.MinValue ? 0 : value;
                            OnPropertyChanged("Class");
                        }
                    }
                    
                    private string connectionCode = string.Empty;
                    [StructuresDialog("joint_code", typeof(TD.String))]
                    public string ConnectionCode {
                        get { return connectionCode; }
                        set {
                            connectionCode = value ?? string.Empty;
                            OnPropertyChanged("ConnectionCode");
                        }
                    }
                    
                    private string autoDefaults = string.Empty;
                    [StructuresDialog("ad_root", typeof(TD.String))]
                    public string AutoDefaults {
                        get { return autoDefaults; }
                        set {
                            autoDefaults = value ?? string.Empty;
                            OnPropertyChanged("AutoDefaults");
                        }
                    }
                }
                
            }
            """;

        private const string PartPropertiesAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the part(s) properties that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.ViewModelPropertiesGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class PartPropertiesAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the part(s) properties using the given number(s).
                    /// </summary>
                    public PartPropertiesAttribute(params uint[] numbers) { }
                    
                    /// <summary>
                    /// Register the part(s) properties using the given name(s).
                    /// </summary>
                    public PartPropertiesAttribute(params string[] names) { }
                    
                }
                
            }
            """;

        private const string PlatePropertiesAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the plate(s) properties that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.ViewModelPropertiesGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class PlatePropertiesAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the plate(s) properties using the given number(s).
                    /// </summary>
                    public PlatePropertiesAttribute(params uint[] numbers)  { }
                    
                    /// <summary>
                    /// Register the plate(s) properties using the given name(s).
                    /// </summary>
                    public PlatePropertiesAttribute(params string[] names)  { }
                    
                }
                
            }
            """;

        private const string WeldPropertiesAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the weld(s) properties that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.ViewModelPropertiesGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class WeldPropertiesAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the weld(s) properties using the given number(s).
                    /// </summary>
                    public WeldPropertiesAttribute(params uint[] numbers) { }
                    
                    /// <summary>
                    /// Register the weld(s) properties using the given name(s).
                    /// </summary>
                    public WeldPropertiesAttribute(params string[] names) { }
                    
                }
                
            }
            """;

        private const string BoltPropertiesAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the bolt(s) properties that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.ViewModelPropertiesGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class BoltPropertiesAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the bolt(s) properties using the given number(s).
                    /// </summary>
                    public BoltPropertiesAttribute(params uint[] numbers) { }
                    
                    /// <summary>
                    /// Register the bolt(s) properties using the given name(s).
                    /// </summary>
                    public BoltPropertiesAttribute(params string[] names) { }
                    
                }
                
            }
            """;

        private const string BoltCirclePropertiesAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the bolt circle(s) properties that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.ViewModelPropertiesGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class BoltCirclePropertiesAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the bolt circle(s) properties using the given number(s).
                    /// </summary>
                    public BoltCirclePropertiesAttribute(params uint[] numbers)  { }
                    
                    /// <summary>
                    /// Register the bolt circle(s) properties using the given name(s).
                    /// </summary>
                    public BoltCirclePropertiesAttribute(params string[] names)  { }
                    
                }
                
            }
            """;

        #endregion

        #region Templates

        private const string ViewModelClassTemplate =
            """
            // <auto-generated/>{{generatedAt}}
            using Tekla.Structures.Dialog;
            using TD = Tekla.Structures.Datatype;

            namespace {{namespace}} {
                
                {{accessibility}} partial {{typeKind}}class {{className}} {
            {{properties}}        
                }
                
            }
            """;

        private const string PartPropertiesTemplate =
            """
                    
                    private string part{{nameOrNumber}}Name;
                    [StructuresDialog("PT{{nameOrNumber}}NAME", typeof(TD.String))]
                    public string Part{{nameOrNumber}}Name {
                        get { return part{{nameOrNumber}}Name; }
                        set {
                            part{{nameOrNumber}}Name = value;
                            OnPropertyChanged("Part{{nameOrNumber}}Name");
                        }
                    }
                    
                    private string part{{nameOrNumber}}Profile;
                    [StructuresDialog("PT{{nameOrNumber}}PRF", typeof(TD.String))]
                    public string Part{{nameOrNumber}}Profile {
                        get { return part{{nameOrNumber}}Profile; }
                        set {
                            part{{nameOrNumber}}Profile = value;
                            OnPropertyChanged("Part{{nameOrNumber}}Profile");
                        }
                    }
                    
                    private string part{{nameOrNumber}}Material;
                    [StructuresDialog("PT{{nameOrNumber}}MATL", typeof(TD.String))]
                    public string Part{{nameOrNumber}}Material {
                        get { return part{{nameOrNumber}}Material; }
                        set {
                            part{{nameOrNumber}}Material = value;
                            OnPropertyChanged("Part{{nameOrNumber}}Material");
                        }
                    }
                    
                    private string part{{nameOrNumber}}Finish;
                    [StructuresDialog("PT{{nameOrNumber}}FNSH", typeof(TD.String))]
                    public string Part{{nameOrNumber}}Finish {
                        get { return part{{nameOrNumber}}Finish; }
                        set {
                            part{{nameOrNumber}}Finish = value;
                            OnPropertyChanged("Part{{nameOrNumber}}Finish");
                        }
                    }
                    
                    private int part{{nameOrNumber}}Class;
                    [StructuresDialog("PT{{nameOrNumber}}CLS", typeof(TD.Integer))]
                    public int Part{{nameOrNumber}}Class {
                        get { return part{{nameOrNumber}}Class; }
                        set {
                            part{{nameOrNumber}}Class = value == int.MinValue ? 99 : value;
                            OnPropertyChanged("Part{{nameOrNumber}}Class");
                        }
                    }
                    
                    private string part{{nameOrNumber}}AssemblyPrefix;
                    [StructuresDialog("PT{{nameOrNumber}}ASMP", typeof(TD.String))]
                    public string Part{{nameOrNumber}}AssemblyPrefix {
                        get { return part{{nameOrNumber}}AssemblyPrefix; }
                        set {
                            part{{nameOrNumber}}AssemblyPrefix = value;
                            OnPropertyChanged("Part{{nameOrNumber}}AssemblyPrefix");
                        }
                    }
                    
                    private int part{{nameOrNumber}}AssemblyStartNumber;
                    [StructuresDialog("PT{{nameOrNumber}}ASMN", typeof(TD.Integer))]
                    public int Part{{nameOrNumber}}AssemblyStartNumber {
                        get { return part{{nameOrNumber}}AssemblyStartNumber; }
                        set {
                            part{{nameOrNumber}}AssemblyStartNumber = value == int.MinValue ? 1 : value;
                            OnPropertyChanged("Part{{nameOrNumber}}AssemblyStartNumber");
                        }
                    }
                    
                    private string part{{nameOrNumber}}PartPrefix;
                    [StructuresDialog("PT{{nameOrNumber}}PTP", typeof(TD.String))]
                    public string Part{{nameOrNumber}}PartPrefix {
                        get { return part{{nameOrNumber}}PartPrefix; }
                        set {
                            part{{nameOrNumber}}PartPrefix = value;
                            OnPropertyChanged("Part{{nameOrNumber}}PartPrefix");
                        }
                    }
                    
                    private int part{{nameOrNumber}}PartStartNumber;
                    [StructuresDialog("PT{{nameOrNumber}}PTN", typeof(TD.Integer))]
                    public int Part{{nameOrNumber}}PartStartNumber {
                        get { return part{{nameOrNumber}}PartStartNumber; }
                        set {
                            part{{nameOrNumber}}PartStartNumber = value == int.MinValue ? 1 : value;
                            OnPropertyChanged("Part{{nameOrNumber}}PartStartNumber");
                        }
                    }
            """;

        private const string PlatePropertiesTemplate =
            """
                    
                    private string plate{{nameOrNumber}}Name;
                    [StructuresDialog("PL{{nameOrNumber}}NAME", typeof(TD.String))]
                    public string Plate{{nameOrNumber}}Name {
                        get { return plate{{nameOrNumber}}Name; }
                        set {
                            plate{{nameOrNumber}}Name = value;
                            OnPropertyChanged("Plate{{nameOrNumber}}Name");
                        }
                    }
                    
                    private double plate{{nameOrNumber}}Thickness;
                    [StructuresDialog("PL{{nameOrNumber}}T", typeof(TD.Double))]
                    public double Plate{{nameOrNumber}}Thickness {
                        get { return plate{{nameOrNumber}}Thickness; }
                        set {
                            plate{{nameOrNumber}}Thickness = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Plate{{nameOrNumber}}Thickness");
                        }
                    }
                    
                    private double plate{{nameOrNumber}}Breadth;
                    [StructuresDialog("PL{{nameOrNumber}}B", typeof(TD.Double))]
                    public double Plate{{nameOrNumber}}Breadth {
                        get { return plate{{nameOrNumber}}Breadth; }
                        set {
                            plate{{nameOrNumber}}Breadth = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Plate{{nameOrNumber}}Breadth");
                        }
                    }
                    
                    private double plate{{nameOrNumber}}Height;
                    [StructuresDialog("PL{{nameOrNumber}}H", typeof(TD.Double))]
                    public double Plate{{nameOrNumber}}Height {
                        get { return plate{{nameOrNumber}}Height; }
                        set {
                            plate{{nameOrNumber}}Height = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Plate{{nameOrNumber}}Height");
                        }
                    }
                    
                    private string plate{{nameOrNumber}}Material;
                    [StructuresDialog("PL{{nameOrNumber}}MATL", typeof(TD.String))]
                    public string Plate{{nameOrNumber}}Material {
                        get { return plate{{nameOrNumber}}Material; }
                        set {
                            plate{{nameOrNumber}}Material = value;
                            OnPropertyChanged("Plate{{nameOrNumber}}Material");
                        }
                    }
                    
                    private string plate{{nameOrNumber}}Finish;
                    [StructuresDialog("PL{{nameOrNumber}}FNSH", typeof(TD.String))]
                    public string Plate{{nameOrNumber}}Finish {
                        get { return plate{{nameOrNumber}}Finish; }
                        set {
                            plate{{nameOrNumber}}Finish = value;
                            OnPropertyChanged("Plate{{nameOrNumber}}Finish");
                        }
                    }
                    
                    private int plate{{nameOrNumber}}Class;
                    [StructuresDialog("PL{{nameOrNumber}}CLS", typeof(TD.Integer))]
                    public int Plate{{nameOrNumber}}Class {
                        get { return plate{{nameOrNumber}}Class; }
                        set {
                            plate{{nameOrNumber}}Class = value == int.MinValue ? 99 : value;
                            OnPropertyChanged("Plate{{nameOrNumber}}Class");
                        }
                    }
                    
                    private string plate{{nameOrNumber}}AssemblyPrefix;
                    [StructuresDialog("PL{{nameOrNumber}}ASMP", typeof(TD.String))]
                    public string Plate{{nameOrNumber}}AssemblyPrefix {
                        get { return plate{{nameOrNumber}}AssemblyPrefix; }
                        set {
                            plate{{nameOrNumber}}AssemblyPrefix = value;
                            OnPropertyChanged("Plate{{nameOrNumber}}AssemblyPrefix");
                        }
                    }
                    
                    private int plate{{nameOrNumber}}AssemblyStartNumber;
                    [StructuresDialog("PL{{nameOrNumber}}ASMN", typeof(TD.Integer))]
                    public int Plate{{nameOrNumber}}AssemblyStartNumber {
                        get { return plate{{nameOrNumber}}AssemblyStartNumber; }
                        set {
                            plate{{nameOrNumber}}AssemblyStartNumber = value == int.MinValue ? 1 : value;
                            OnPropertyChanged("Plate{{nameOrNumber}}AssemblyStartNumber");
                        }
                    }
                    
                    private string plate{{nameOrNumber}}PartPrefix;
                    [StructuresDialog("PL{{nameOrNumber}}PTP", typeof(TD.String))]
                    public string Plate{{nameOrNumber}}PartPrefix {
                        get { return plate{{nameOrNumber}}PartPrefix; }
                        set {
                            plate{{nameOrNumber}}PartPrefix = value;
                            OnPropertyChanged("Plate{{nameOrNumber}}PartPrefix");
                        }
                    }
                    
                    private int plate{{nameOrNumber}}PartStartNumber;
                    [StructuresDialog("PL{{nameOrNumber}}PTN", typeof(TD.Integer))]
                    public int Plate{{nameOrNumber}}PartStartNumber {
                        get { return plate{{nameOrNumber}}PartStartNumber; }
                        set {
                            plate{{nameOrNumber}}PartStartNumber = value == int.MinValue ? 1 : value;
                            OnPropertyChanged("Plate{{nameOrNumber}}PartStartNumber");
                        }
                    }
            """;

        private const string WeldPropertiesTemplate =
            """
                    
                    private double weld{{nameOrNumber}}SizeAbove;
                    [StructuresDialog("W{{nameOrNumber}}SIZEA", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}SizeAbove {
                        get { return weld{{nameOrNumber}}SizeAbove; }
                        set {
                            weld{{nameOrNumber}}SizeAbove = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}SizeAbove");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}SizeBelow;
                    [StructuresDialog("W{{nameOrNumber}}SIZEB", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}SizeBelow {
                        get { return weld{{nameOrNumber}}SizeBelow; }
                        set {
                            weld{{nameOrNumber}}SizeBelow = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}SizeBelow");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}TypeAbove;
                    [StructuresDialog("W{{nameOrNumber}}TYPEA", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}TypeAbove {
                        get { return weld{{nameOrNumber}}TypeAbove; }
                        set {
                            weld{{nameOrNumber}}TypeAbove = (value < 0 || value > 26) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}TypeAbove");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}TypeBelow;
                    [StructuresDialog("W{{nameOrNumber}}TYPEB", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}TypeBelow {
                        get { return weld{{nameOrNumber}}TypeBelow; }
                        set {
                            weld{{nameOrNumber}}TypeBelow = (value < 0 || value > 26) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}TypeBelow");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}AngleAbove;
                    [StructuresDialog("W{{nameOrNumber}}ANGA", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}AngleAbove {
                        get { return weld{{nameOrNumber}}AngleAbove; }
                        set {
                            weld{{nameOrNumber}}AngleAbove = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}AngleAbove");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}AngleBelow;
                    [StructuresDialog("W{{nameOrNumber}}ANGB", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}AngleBelow {
                        get { return weld{{nameOrNumber}}AngleBelow; }
                        set {
                            weld{{nameOrNumber}}AngleBelow = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}AngleBelow");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}ContourAbove;
                    [StructuresDialog("W{{nameOrNumber}}CTRA", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}ContourAbove {
                        get { return weld{{nameOrNumber}}ContourAbove; }
                        set {
                            weld{{nameOrNumber}}ContourAbove = (value < 0 || value > 3) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}ContourAbove");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}ContourBelow;
                    [StructuresDialog("W{{nameOrNumber}}CTRB", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}ContourBelow {
                        get { return weld{{nameOrNumber}}ContourBelow; }
                        set {
                            weld{{nameOrNumber}}ContourBelow = (value < 0 || value > 3) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}ContourBelow");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}FinishAbove;
                    [StructuresDialog("W{{nameOrNumber}}FNSHA", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}FinishAbove {
                        get { return weld{{nameOrNumber}}FinishAbove; }
                        set {
                            weld{{nameOrNumber}}FinishAbove = (value < 0 || value > 5) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}FinishAbove");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}FinishBelow;
                    [StructuresDialog("W{{nameOrNumber}}FNSHB", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}FinishBelow {
                        get { return weld{{nameOrNumber}}FinishBelow; }
                        set {
                            weld{{nameOrNumber}}FinishBelow = (value < 0 || value > 5) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}FinishBelow");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}RootFaceAbove;
                    [StructuresDialog("W{{nameOrNumber}}FACEA", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}RootFaceAbove {
                        get { return weld{{nameOrNumber}}RootFaceAbove; }
                        set {
                            weld{{nameOrNumber}}RootFaceAbove = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}RootFaceAbove");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}RootFaceBelow;
                    [StructuresDialog("W{{nameOrNumber}}FACEB", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}RootFaceBelow {
                        get { return weld{{nameOrNumber}}RootFaceBelow; }
                        set {
                            weld{{nameOrNumber}}RootFaceBelow = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}RootFaceBelow");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}EffectiveThroatAbove;
                    [StructuresDialog("W{{nameOrNumber}}THROA", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}EffectiveThroatAbove {
                        get { return weld{{nameOrNumber}}EffectiveThroatAbove; }
                        set {
                            weld{{nameOrNumber}}EffectiveThroatAbove = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}EffectiveThroatAbove");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}EffectiveThroatBelow;
                    [StructuresDialog("W{{nameOrNumber}}THROB", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}EffectiveThroatBelow {
                        get { return weld{{nameOrNumber}}EffectiveThroatBelow; }
                        set {
                            weld{{nameOrNumber}}EffectiveThroatBelow = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}EffectiveThroatBelow");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}RootOpeningAbove;
                    [StructuresDialog("W{{nameOrNumber}}OPNGA", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}RootOpeningAbove {
                        get { return weld{{nameOrNumber}}RootOpeningAbove; }
                        set {
                            weld{{nameOrNumber}}RootOpeningAbove = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}RootOpeningAbove");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}RootOpeningBelow;
                    [StructuresDialog("W{{nameOrNumber}}OPNGB", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}RootOpeningBelow {
                        get { return weld{{nameOrNumber}}RootOpeningBelow; }
                        set {
                            weld{{nameOrNumber}}RootOpeningBelow = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}RootOpeningBelow");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}IncrementAmountAbove;
                    [StructuresDialog("W{{nameOrNumber}}INCRA", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}IncrementAmountAbove {
                        get { return weld{{nameOrNumber}}IncrementAmountAbove; }
                        set {
                            weld{{nameOrNumber}}IncrementAmountAbove = value == int.MinValue ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}IncrementAmountAbove");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}IncrementAmountBelow;
                    [StructuresDialog("W{{nameOrNumber}}INCRB", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}IncrementAmountBelow {
                        get { return weld{{nameOrNumber}}IncrementAmountBelow; }
                        set {
                            weld{{nameOrNumber}}IncrementAmountBelow = value == int.MinValue ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}IncrementAmountBelow");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}LengthAbove;
                    [StructuresDialog("W{{nameOrNumber}}LENA", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}LengthAbove {
                        get { return weld{{nameOrNumber}}LengthAbove; }
                        set {
                            weld{{nameOrNumber}}LengthAbove = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}LengthAbove");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}LengthBelow;
                    [StructuresDialog("W{{nameOrNumber}}LENB", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}LengthBelow {
                        get { return weld{{nameOrNumber}}LengthBelow; }
                        set {
                            weld{{nameOrNumber}}LengthBelow = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}LengthBelow");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}PitchAbove;
                    [StructuresDialog("W{{nameOrNumber}}PITA", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}PitchAbove {
                        get { return weld{{nameOrNumber}}PitchAbove; }
                        set {
                            weld{{nameOrNumber}}PitchAbove = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}PitchAbove");
                        }
                    }
                    
                    private double weld{{nameOrNumber}}PitchBelow;
                    [StructuresDialog("W{{nameOrNumber}}PITB", typeof(TD.Double))]
                    public double Weld{{nameOrNumber}}PitchBelow {
                        get { return weld{{nameOrNumber}}PitchBelow; }
                        set {
                            weld{{nameOrNumber}}PitchBelow = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}PitchBelow");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}Around;
                    [StructuresDialog("W{{nameOrNumber}}ARND", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}Around {
                        get { return weld{{nameOrNumber}}Around; }
                        set {
                            weld{{nameOrNumber}}Around = (value < 0 || value > 1) ? 1 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}Around");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}Shop;
                    [StructuresDialog("W{{nameOrNumber}}SHOP", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}Shop {
                        get { return weld{{nameOrNumber}}Shop; }
                        set {
                            weld{{nameOrNumber}}Shop = (value < 0 || value > 1) ? 1 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}Shop");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}Placement;
                    [StructuresDialog("W{{nameOrNumber}}PLACE", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}Placement {
                        get { return weld{{nameOrNumber}}Placement; }
                        set {
                            weld{{nameOrNumber}}Placement = (value < 0 || value > 2) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}Placement");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}Preparation;
                    [StructuresDialog("W{{nameOrNumber}}PREP", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}Preparation {
                        get { return weld{{nameOrNumber}}Preparation; }
                        set {
                            weld{{nameOrNumber}}Preparation = (value < 0 || value > 3) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}Preparation");
                        }
                    }
                    
                    private int weld{{nameOrNumber}}Intermittent;
                    [StructuresDialog("W{{nameOrNumber}}INTMI", typeof(TD.Integer))]
                    public int Weld{{nameOrNumber}}Intermittent {
                        get { return weld{{nameOrNumber}}Intermittent; }
                        set {
                            weld{{nameOrNumber}}Intermittent = (value < 0 || value > 2) ? 0 : value;
                            OnPropertyChanged("Weld{{nameOrNumber}}Intermittent");
                        }
                    }
                    
                    private string weld{{nameOrNumber}}ReferenceText;
                    [StructuresDialog("W{{nameOrNumber}}TEXT", typeof(TD.String))]
                    public string Weld{{nameOrNumber}}ReferenceText {
                        get { return weld{{nameOrNumber}}ReferenceText; }
                        set {
                            weld{{nameOrNumber}}ReferenceText = value;
                            OnPropertyChanged("Weld{{nameOrNumber}}ReferenceText");
                        }
                    }
            """;

        private const string BoltPropertiesTemplate =
            """
                    
                    private TD.Distance bolt{{nameOrNumber}}Size;
                    [StructuresDialog("B{{nameOrNumber}}SIZE", typeof(TD.Distance))]
                    public TD.Distance Bolt{{nameOrNumber}}Size {
                        get { return bolt{{nameOrNumber}}Size; }
                        set {
                            bolt{{nameOrNumber}}Size = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Size");
                        }
                    }
                    
                    private string bolt{{nameOrNumber}}Standard;
                    [StructuresDialog("B{{nameOrNumber}}STD", typeof(TD.String))]
                    public string Bolt{{nameOrNumber}}Standard {
                        get { return bolt{{nameOrNumber}}Standard; }
                        set {
                            bolt{{nameOrNumber}}Standard = string.IsNullOrEmpty(value) ? "HS10.9" : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Standard");
                        }
                    }
                    
                    private TD.DistanceList bolt{{nameOrNumber}}DistX;
                    [StructuresDialog("B{{nameOrNumber}}DISTX", typeof(TD.DistanceList))]
                    public TD.DistanceList Bolt{{nameOrNumber}}DistX {
                        get { return bolt{{nameOrNumber}}DistX; }
                        set {
                            bolt{{nameOrNumber}}DistX = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}DistX");
                        }
                    }
                    
                    private TD.DistanceList bolt{{nameOrNumber}}DistY;
                    [StructuresDialog("B{{nameOrNumber}}DISTY", typeof(TD.DistanceList))]
                    public TD.DistanceList Bolt{{nameOrNumber}}DistY {
                        get { return bolt{{nameOrNumber}}DistY; }
                        set {
                            bolt{{nameOrNumber}}DistY = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}DistY");
                        }
                    }
                    
                    private int bolt{{nameOrNumber}}Type;
                    [StructuresDialog("B{{nameOrNumber}}TYPE", typeof(TD.Integer))]
                    public int Bolt{{nameOrNumber}}Type {
                        get { return bolt{{nameOrNumber}}Type; }
                        set {
                            bolt{{nameOrNumber}}Type = (value < 0 || value > 1) ? 0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Type");
                        }
                    }
                    
                    private int bolt{{nameOrNumber}}ThreadInMaterial;
                    [StructuresDialog("B{{nameOrNumber}}THRD", typeof(TD.Integer))]
                    public int Bolt{{nameOrNumber}}ThreadInMaterial {
                        get { return bolt{{nameOrNumber}}ThreadInMaterial; }
                        set {
                            bolt{{nameOrNumber}}ThreadInMaterial = (value < 0 || value > 1) ? 1 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}ThreadInMaterial");
                        }
                    }
                    
                    private double bolt{{nameOrNumber}}CutLength;
                    [StructuresDialog("B{{nameOrNumber}}CLEN", typeof(TD.Double))]
                    public double Bolt{{nameOrNumber}}CutLength {
                        get { return bolt{{nameOrNumber}}CutLength; }
                        set {
                            bolt{{nameOrNumber}}CutLength = value == int.MinValue ? 100.0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}CutLength");
                        }
                    }
                    
                    private double bolt{{nameOrNumber}}ExtraLength;
                    [StructuresDialog("B{{nameOrNumber}}XLEN", typeof(TD.Double))]
                    public double Bolt{{nameOrNumber}}ExtraLength {
                        get { return bolt{{nameOrNumber}}ExtraLength; }
                        set {
                            bolt{{nameOrNumber}}ExtraLength = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}ExtraLength");
                        }
                    }
                    
                    private double bolt{{nameOrNumber}}Tolerance;
                    [StructuresDialog("B{{nameOrNumber}}TOL", typeof(TD.Double))]
                    public double Bolt{{nameOrNumber}}Tolerance {
                        get { return bolt{{nameOrNumber}}Tolerance; }
                        set {
                            bolt{{nameOrNumber}}Tolerance = value == int.MinValue ? 2.0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Tolerance");
                        }
                    }
                    
                    private int bolt{{nameOrNumber}}PlainType;
                    [StructuresDialog("B{{nameOrNumber}}PLAIN", typeof(TD.Integer))]
                    public int Bolt{{nameOrNumber}}PlainType {
                        get { return bolt{{nameOrNumber}}PlainType; }
                        set {
                            bolt{{nameOrNumber}}PlainType = (value < 0 || value > 1) ? 0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}PlainType");
                        }
                    }
                    
                    private double bolt{{nameOrNumber}}BlindHoleDepth;
                    [StructuresDialog("B{{nameOrNumber}}DEPTH", typeof(TD.Double))]
                    public double Bolt{{nameOrNumber}}BlindHoleDepth {
                        get { return bolt{{nameOrNumber}}BlindHoleDepth; }
                        set {
                            bolt{{nameOrNumber}}BlindHoleDepth = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}BlindHoleDepth");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}Hole1 = true;
                    [StructuresDialog("B{{nameOrNumber}}HOLE1", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}Hole1 {
                        get { return bolt{{nameOrNumber}}Hole1; }
                        set {
                            bolt{{nameOrNumber}}Hole1 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Hole1");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}Hole2 = true;
                    [StructuresDialog("B{{nameOrNumber}}HOLE2", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}Hole2 {
                        get { return bolt{{nameOrNumber}}Hole2; }
                        set {
                            bolt{{nameOrNumber}}Hole2 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Hole2");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}Hole3;
                    [StructuresDialog("B{{nameOrNumber}}HOLE3", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}Hole3 {
                        get { return bolt{{nameOrNumber}}Hole3; }
                        set {
                            bolt{{nameOrNumber}}Hole3 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Hole3");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}Hole4;
                    [StructuresDialog("B{{nameOrNumber}}HOLE4", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}Hole4 {
                        get { return bolt{{nameOrNumber}}Hole4; }
                        set {
                            bolt{{nameOrNumber}}Hole4 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Hole4");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}Hole5;
                    [StructuresDialog("B{{nameOrNumber}}HOLE5", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}Hole5 {
                        get { return bolt{{nameOrNumber}}Hole5; }
                        set {
                            bolt{{nameOrNumber}}Hole5 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}Hole5");
                        }
                    }
                    
                    private int bolt{{nameOrNumber}}HoleType;
                    [StructuresDialog("B{{nameOrNumber}}HOLTY", typeof(TD.Integer))]
                    public int Bolt{{nameOrNumber}}HoleType {
                        get { return bolt{{nameOrNumber}}HoleType; }
                        set {
                            bolt{{nameOrNumber}}HoleType = (value < 0 || value > 2) ? 0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}HoleType");
                        }
                    }
                    
                    private double bolt{{nameOrNumber}}SlottedHoleX;
                    [StructuresDialog("B{{nameOrNumber}}SLOTX", typeof(TD.Double))]
                    public double Bolt{{nameOrNumber}}SlottedHoleX {
                        get { return bolt{{nameOrNumber}}SlottedHoleX; }
                        set {
                            bolt{{nameOrNumber}}SlottedHoleX = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}SlottedHoleX");
                        }
                    }
                    
                    private double bolt{{nameOrNumber}}SlottedHoleY;
                    [StructuresDialog("B{{nameOrNumber}}SLOTY", typeof(TD.Double))]
                    public double Bolt{{nameOrNumber}}SlottedHoleY {
                        get { return bolt{{nameOrNumber}}SlottedHoleY; }
                        set {
                            bolt{{nameOrNumber}}SlottedHoleY = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}SlottedHoleY");
                        }
                    }
                    
                    private int bolt{{nameOrNumber}}RotateSlots;
                    [StructuresDialog("B{{nameOrNumber}}RSLOT", typeof(TD.Integer))]
                    public int Bolt{{nameOrNumber}}RotateSlots {
                        get { return bolt{{nameOrNumber}}RotateSlots; }
                        set {
                            bolt{{nameOrNumber}}RotateSlots = (value < 0 || value > 2) ? 2 : value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}RotateSlots");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}IsBolt = true;
                    [StructuresDialog("B{{nameOrNumber}}ISBOT", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}IsBolt {
                        get { return bolt{{nameOrNumber}}IsBolt; }
                        set {
                            bolt{{nameOrNumber}}IsBolt = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}IsBolt");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}UseNut1 = true;
                    [StructuresDialog("B{{nameOrNumber}}NUT1", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}UseNut1 {
                        get { return bolt{{nameOrNumber}}UseNut1; }
                        set {
                            bolt{{nameOrNumber}}UseNut1 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}UseNut1");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}UseNut2;
                    [StructuresDialog("B{{nameOrNumber}}NUT2", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}UseNut2 {
                        get { return bolt{{nameOrNumber}}UseNut2; }
                        set {
                            bolt{{nameOrNumber}}UseNut2 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}UseNut2");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}UseWasher1;
                    [StructuresDialog("B{{nameOrNumber}}WSHR1", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}UseWasher1 {
                        get { return bolt{{nameOrNumber}}UseWasher1; }
                        set {
                            bolt{{nameOrNumber}}UseWasher1 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}UseWasher1");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}UseWasher2;
                    [StructuresDialog("B{{nameOrNumber}}WSHR2", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}UseWasher2 {
                        get { return bolt{{nameOrNumber}}UseWasher2; }
                        set {
                            bolt{{nameOrNumber}}UseWasher2 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}UseWasher2");
                        }
                    }
                    
                    private bool bolt{{nameOrNumber}}UseWasher3 = true;
                    [StructuresDialog("B{{nameOrNumber}}WSHR3", typeof(TD.Boolean))]
                    public bool Bolt{{nameOrNumber}}UseWasher3 {
                        get { return bolt{{nameOrNumber}}UseWasher3; }
                        set {
                            bolt{{nameOrNumber}}UseWasher3 = value;
                            OnPropertyChanged("Bolt{{nameOrNumber}}UseWasher3");
                        }
                    }
            """;

        private const string BoltCirclePropertiesTemplate =
            """
                    
                    private TD.Distance boltCircle{{nameOrNumber}}Size;
                    [StructuresDialog("BC{{nameOrNumber}}SIZE", typeof(TD.Distance))]
                    public TD.Distance BoltCircle{{nameOrNumber}}Size {
                        get { return boltCircle{{nameOrNumber}}Size; }
                        set {
                            boltCircle{{nameOrNumber}}Size = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Size");
                        }
                    }
                    
                    private string boltCircle{{nameOrNumber}}Standard;
                    [StructuresDialog("BC{{nameOrNumber}}STD", typeof(TD.String))]
                    public string BoltCircle{{nameOrNumber}}Standard {
                        get { return boltCircle{{nameOrNumber}}Standard; }
                        set {
                            boltCircle{{nameOrNumber}}Standard = string.IsNullOrEmpty(value) ? "HS10.9" : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Standard");
                        }
                    }
                    
                    private int boltCircle{{nameOrNumber}}NumberOfBolts;
                    [StructuresDialog("BC{{nameOrNumber}}NUM", typeof(TD.Integer))]
                    public int BoltCircle{{nameOrNumber}}NumberOfBolts {
                        get { return boltCircle{{nameOrNumber}}NumberOfBolts; }
                        set {
                            boltCircle{{nameOrNumber}}NumberOfBolts = value == int.MinValue ? 0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}NumberOfBolts");
                        }
                    }
                    
                    private double boltCircle{{nameOrNumber}}Diameter;
                    [StructuresDialog("BC{{nameOrNumber}}DIAM", typeof(TD.Double))]
                    public double BoltCircle{{nameOrNumber}}Diameter {
                        get { return boltCircle{{nameOrNumber}}Diameter; }
                        set {
                            boltCircle{{nameOrNumber}}Diameter = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Diameter");
                        }
                    }
                    
                    private int boltCircle{{nameOrNumber}}Type;
                    [StructuresDialog("BC{{nameOrNumber}}TYPE", typeof(TD.Integer))]
                    public int BoltCircle{{nameOrNumber}}Type {
                        get { return boltCircle{{nameOrNumber}}Type; }
                        set {
                            boltCircle{{nameOrNumber}}Type = (value < 0 || value > 1) ? 0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Type");
                        }
                    }
                    
                    private int boltCircle{{nameOrNumber}}ThreadInMaterial;
                    [StructuresDialog("BC{{nameOrNumber}}THRD", typeof(TD.Integer))]
                    public int BoltCircle{{nameOrNumber}}ThreadInMaterial {
                        get { return boltCircle{{nameOrNumber}}ThreadInMaterial; }
                        set {
                            boltCircle{{nameOrNumber}}ThreadInMaterial = (value < 0 || value > 1) ? 1 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}ThreadInMaterial");
                        }
                    }
                    
                    private double boltCircle{{nameOrNumber}}CutLength;
                    [StructuresDialog("BC{{nameOrNumber}}CLEN", typeof(TD.Double))]
                    public double BoltCircle{{nameOrNumber}}CutLength {
                        get { return boltCircle{{nameOrNumber}}CutLength; }
                        set {
                            boltCircle{{nameOrNumber}}CutLength = value == int.MinValue ? 100.0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}CutLength");
                        }
                    }
                    
                    private double boltCircle{{nameOrNumber}}ExtraLength;
                    [StructuresDialog("BC{{nameOrNumber}}XLEN", typeof(TD.Double))]
                    public double BoltCircle{{nameOrNumber}}ExtraLength {
                        get { return boltCircle{{nameOrNumber}}ExtraLength; }
                        set {
                            boltCircle{{nameOrNumber}}ExtraLength = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}ExtraLength");
                        }
                    }
                    
                    private double boltCircle{{nameOrNumber}}Tolerance;
                    [StructuresDialog("BC{{nameOrNumber}}TOL", typeof(TD.Double))]
                    public double BoltCircle{{nameOrNumber}}Tolerance {
                        get { return boltCircle{{nameOrNumber}}Tolerance; }
                        set {
                            boltCircle{{nameOrNumber}}Tolerance = value == int.MinValue ? 2.0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Tolerance");
                        }
                    }
                    
                    private int boltCircle{{nameOrNumber}}PlainType;
                    [StructuresDialog("BC{{nameOrNumber}}PLAIN", typeof(TD.Integer))]
                    public int BoltCircle{{nameOrNumber}}PlainType {
                        get { return boltCircle{{nameOrNumber}}PlainType; }
                        set {
                            boltCircle{{nameOrNumber}}PlainType = (value < 0 || value > 1) ? 0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}PlainType");
                        }
                    }
                    
                    private double boltCircle{{nameOrNumber}}BlindHoleDepth;
                    [StructuresDialog("BC{{nameOrNumber}}DEPTH", typeof(TD.Double))]
                    public double BoltCircle{{nameOrNumber}}BlindHoleDepth {
                        get { return boltCircle{{nameOrNumber}}BlindHoleDepth; }
                        set {
                            boltCircle{{nameOrNumber}}BlindHoleDepth = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}BlindHoleDepth");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}Hole1 = true;
                    [StructuresDialog("BC{{nameOrNumber}}HOLE1", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}Hole1 {
                        get { return boltCircle{{nameOrNumber}}Hole1; }
                        set {
                            boltCircle{{nameOrNumber}}Hole1 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Hole1");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}Hole2 = true;
                    [StructuresDialog("BC{{nameOrNumber}}HOLE2", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}Hole2 {
                        get { return boltCircle{{nameOrNumber}}Hole2; }
                        set {
                            boltCircle{{nameOrNumber}}Hole2 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Hole2");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}Hole3;
                    [StructuresDialog("BC{{nameOrNumber}}HOLE3", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}Hole3 {
                        get { return boltCircle{{nameOrNumber}}Hole3; }
                        set {
                            boltCircle{{nameOrNumber}}Hole3 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Hole3");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}Hole4;
                    [StructuresDialog("BC{{nameOrNumber}}HOLE4", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}Hole4 {
                        get { return boltCircle{{nameOrNumber}}Hole4; }
                        set {
                            boltCircle{{nameOrNumber}}Hole4 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Hole4");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}Hole5;
                    [StructuresDialog("BC{{nameOrNumber}}HOLE5", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}Hole5 {
                        get { return boltCircle{{nameOrNumber}}Hole5; }
                        set {
                            boltCircle{{nameOrNumber}}Hole5 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}Hole5");
                        }
                    }
                    
                    private int boltCircle{{nameOrNumber}}HoleType;
                    [StructuresDialog("BC{{nameOrNumber}}HOLTY", typeof(TD.Integer))]
                    public int BoltCircle{{nameOrNumber}}HoleType {
                        get { return boltCircle{{nameOrNumber}}HoleType; }
                        set {
                            boltCircle{{nameOrNumber}}HoleType = (value < 0 || value > 2) ? 0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}HoleType");
                        }
                    }
                    
                    private double boltCircle{{nameOrNumber}}SlottedHoleX;
                    [StructuresDialog("BC{{nameOrNumber}}SLOTX", typeof(TD.Double))]
                    public double BoltCircle{{nameOrNumber}}SlottedHoleX {
                        get { return boltCircle{{nameOrNumber}}SlottedHoleX; }
                        set {
                            boltCircle{{nameOrNumber}}SlottedHoleX = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}SlottedHoleX");
                        }
                    }
                    
                    private double boltCircle{{nameOrNumber}}SlottedHoleY;
                    [StructuresDialog("BC{{nameOrNumber}}SLOTY", typeof(TD.Double))]
                    public double BoltCircle{{nameOrNumber}}SlottedHoleY {
                        get { return boltCircle{{nameOrNumber}}SlottedHoleY; }
                        set {
                            boltCircle{{nameOrNumber}}SlottedHoleY = value == int.MinValue ? 0.0 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}SlottedHoleY");
                        }
                    }
                    
                    private int boltCircle{{nameOrNumber}}RotateSlots;
                    [StructuresDialog("BC{{nameOrNumber}}RSLOT", typeof(TD.Integer))]
                    public int BoltCircle{{nameOrNumber}}RotateSlots {
                        get { return boltCircle{{nameOrNumber}}RotateSlots; }
                        set {
                            boltCircle{{nameOrNumber}}RotateSlots = (value < 0 || value > 2) ? 2 : value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}RotateSlots");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}IsBolt = true;
                    [StructuresDialog("BC{{nameOrNumber}}ISBOT", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}IsBolt {
                        get { return boltCircle{{nameOrNumber}}IsBolt; }
                        set {
                            boltCircle{{nameOrNumber}}IsBolt = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}IsBolt");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}UseNut1 = true;
                    [StructuresDialog("BC{{nameOrNumber}}NUT1", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}UseNut1 {
                        get { return boltCircle{{nameOrNumber}}UseNut1; }
                        set {
                            boltCircle{{nameOrNumber}}UseNut1 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}UseNut1");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}UseNut2;
                    [StructuresDialog("BC{{nameOrNumber}}NUT2", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}UseNut2 {
                        get { return boltCircle{{nameOrNumber}}UseNut2; }
                        set {
                            boltCircle{{nameOrNumber}}UseNut2 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}UseNut2");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}UseWasher1;
                    [StructuresDialog("BC{{nameOrNumber}}WSHR1", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}UseWasher1 {
                        get { return boltCircle{{nameOrNumber}}UseWasher1; }
                        set {
                            boltCircle{{nameOrNumber}}UseWasher1 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}UseWasher1");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}UseWasher2;
                    [StructuresDialog("BC{{nameOrNumber}}WSHR2", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}UseWasher2 {
                        get { return boltCircle{{nameOrNumber}}UseWasher2; }
                        set {
                            boltCircle{{nameOrNumber}}UseWasher2 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}UseWasher2");
                        }
                    }
                    
                    private bool boltCircle{{nameOrNumber}}UseWasher3 = true;
                    [StructuresDialog("BC{{nameOrNumber}}WSHR3", typeof(TD.Boolean))]
                    public bool BoltCircle{{nameOrNumber}}UseWasher3 {
                        get { return boltCircle{{nameOrNumber}}UseWasher3; }
                        set {
                            boltCircle{{nameOrNumber}}UseWasher3 = value;
                            OnPropertyChanged("BoltCircle{{nameOrNumber}}UseWasher3");
                        }
                    }
            """;

        #endregion

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            context.RegisterPostInitializationOutput(ctx => {
                ctx.AddSource("NotificationObject.g.cs", SourceText.From(NotificationObject, Encoding.UTF8));
                ctx.AddSource("ConnectionViewModel.g.cs", SourceText.From(ConnectionViewModel, Encoding.UTF8));
                ctx.AddSource("DetailViewModel.g.cs", SourceText.From(DetailViewModel, Encoding.UTF8));
                ctx.AddSource("PartPropertiesAttribute.g.cs", SourceText.From(PartPropertiesAttribute, Encoding.UTF8));
                ctx.AddSource("PlatePropertiesAttribute.g.cs",
                    SourceText.From(PlatePropertiesAttribute, Encoding.UTF8));
                ctx.AddSource("WeldPropertiesAttribute.g.cs", SourceText.From(WeldPropertiesAttribute, Encoding.UTF8));
                ctx.AddSource("BoltPropertiesAttribute.g.cs", SourceText.From(BoltPropertiesAttribute, Encoding.UTF8));
                ctx.AddSource("BoltCirclePropertiesAttribute.g.cs",
                    SourceText.From(BoltCirclePropertiesAttribute, Encoding.UTF8));
            });

            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(Predicate, Transform)
                .Where(x => x != null);

            context.RegisterSourceOutput(provider, Generate);
        }

        private void Generate(SourceProductionContext context, AppliedClassInfo? classInfo) {
            if (!classInfo.HasValue) return;
            var info = classInfo.Value;

            var builder = new StringBuilder();
            foreach (var kvp in info.AttributesInfo) {
                var attributeName = kvp.Key;
                var propertiesTemplate = attributeName switch {
                    "PartPropertiesAttribute" => PartPropertiesTemplate,
                    "PlatePropertiesAttribute" => PlatePropertiesTemplate,
                    "WeldPropertiesAttribute" => WeldPropertiesTemplate,
                    "BoltPropertiesAttribute" => BoltPropertiesTemplate,
                    "BoltCirclePropertiesAttribute" => BoltCirclePropertiesTemplate,
                    _ => throw new NotSupportedException()
                };

                foreach (var nameOrNumber in kvp.Value) {
                    var match = Regex.Match(nameOrNumber, InternalAttributesDiagnoser.SpecialCharacterPattern);
                    if (match.Success) continue;

                    builder.AppendLine(propertiesTemplate.Replace("{{nameOrNumber}}", nameOrNumber));
                }
            }

            var output = ViewModelClassTemplate
#if DEBUG
                .Replace("{{generatedAt}}", $" at {DateTime.Now}")
#else
                .Replace("{{generatedAt}}", string.Empty)
#endif
                .Replace("{{namespace}}", info.NameSpace)
                .Replace("{{accessibility}}", info.Accessibility.ToString().ToLower())
                .Replace("{{typeKind}}", info.IsRecord ? "record " : string.Empty)
                .Replace("{{className}}", info.Name)
                .Replace("{{properties}}", builder.ToString());

            context.AddSource($"{info.Name}.g.cs", SourceText.From(output, Encoding.UTF8));
        }

        private bool Predicate(SyntaxNode syntaxNode, CancellationToken token) {
            if (token.IsCancellationRequested) return false;

            if (syntaxNode is not ClassDeclarationSyntax classDeclaration ||
                classDeclaration.AttributeLists.Count == 0)
                return false;

            return true;
        }

        private AppliedClassInfo? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token) {
            var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
            if (!classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) return null;

            var semanticModel = syntaxContext.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);

            if (classSymbol == null || !classSymbol.AllInterfaces.Any(i =>
                    i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged"))
                return null;

            return GeneratorHelper.GetClassInfo(syntaxContext, token, ConcernedAttributes);
        }
    }
}
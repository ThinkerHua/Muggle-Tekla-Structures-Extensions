# Muggle.TsExtensions.CodingHelper

Help to generate fields (with StructuresFieldAttribute) for plugin data and properties (with StructuresDialogAttribute) for view model which used by plugin WPF UI.

## Attributes

Apply attributes to partial class to auto generate fields or properties, currently available attributes are:

- PartFieldsAttribute
- PlateFieldsAttribute
- WeldFieldsAttribute
- BoltFieldsAttribute
- BoltCircleFieldsAttribute
- PartPropertiesAttribute
- PlatePropertiesAttribute
- WeldPropertiesAttribute
- BoltPropertiesAttribute
- BoltCirclePropertiesAttribute

The attributes end with "FieldsAttribute" used for plugin data fields, and the attributes end with "PropertiesAttribute" used for view model properties.

Mapping relationship between model object properties and attribute name [see here](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md).

## View model bases

Inherit view model from preset view model base to make it has some general properties and ability to notify when its property changed, currently avaliable view model base are:

- NotificationObject
A simple abstract class which implement INotifyPropertyChanged, inherit from it so you can use OnPropertyChanged method directly.
- ConnectionViewModel
An abstract class inherit from NotificationObject, and has a several general properties for connection plugin. The use of properties is consistent with the Tekla Structures system connection component general tab.
- DetailViewModel
An abstract class inherit from NotificationObject, and has a several general properties for detail plugin. The use of properties is consistent with the Tekla Structures system detail component general tab.

## Example

For example, the source codes by hand:

~~~CSharp
// in PluginDemo.cs
using Muggle.TsExtensions.CodingHelper.Generators;

[PlateFields("EndPlate")]
public partial class PluginData { }

[Plugin("PluginDemo")]
[PluginUserInterface("PluginDemo.Views.MainWindow")]
[SecondaryType(SecondaryType.SECONDARYTYPE_ONE)]
public class PluginDemo : ConnectionBase {
    // ...
    public PluginDemo(PluginData data) {
        // ...
    }
    // ...
}
~~~

~~~CSharp
// in MainWindowViewModel.cs
using Muggle.TsExtensions.CodingHelper.Generators;

[PlateProperties("EndPlate")]
public partial class MainWindowViewModel : ConnectionViewModel { 
    // ...
}
~~~

Then it will auto generate codes behind like this:

~~~CSharp
//  in PluginData.g.cs

public partial class PluginData {
        
        [StructuresField("PLEndPlateNAME")]
        public string PlateEndPlateName;
        
        [StructuresField("PLEndPlateT")]
        public double PlateEndPlateThickness;
        
        [StructuresField("PLEndPlateB")]
        public double PlateEndPlateBreadth;
        
        [StructuresField("PLEndPlateH")]
        public double PlateEndPlateHeight;
        
        [StructuresField("PLEndPlateMATL")]
        public string PlateEndPlateMaterial;
        
        [StructuresField("PLEndPlateFNSH")]
        public string PlateEndPlateFinish;
        
        [StructuresField("PLEndPlateCLS")]
        public int PlateEndPlateClass;
        
        [StructuresField("PLEndPlateASMP")]
        public string PlateEndPlateAssemblyPrefix;
        
        [StructuresField("PLEndPlateASMN")]
        public int PlateEndPlateAssemblyStartNumber;
        
        [StructuresField("PLEndPlatePTP")]
        public string PlateEndPlatePartPrefix;
        
        [StructuresField("PLEndPlatePTN")]
        public int PlateEndPlatePartStartNumber;
}
~~~

~~~CSharp
// in MainWindowViewModel.g.cs

public partial class MainWindowViewModel {
        
        private string plateEndPlateName;
        [StructuresDialog("PLEndPlateNAME", typeof(TD.String))]
        public string PlateEndPlateName {
            get { return plateEndPlateName; }
            set {
                plateEndPlateName = value;
                OnPropertyChanged("PlateEndPlateName");
            }
        }
        
        private double plateEndPlateThickness;
        [StructuresDialog("PLEndPlateT", typeof(TD.Double))]
        public double PlateEndPlateThickness {
            get { return plateEndPlateThickness; }
            set {
                plateEndPlateThickness = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged("PlateEndPlateThickness");
            }
        }
        
        private double plateEndPlateBreadth;
        [StructuresDialog("PLEndPlateB", typeof(TD.Double))]
        public double PlateEndPlateBreadth {
            get { return plateEndPlateBreadth; }
            set {
                plateEndPlateBreadth = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged("PlateEndPlateBreadth");
            }
        }
        
        private double plateEndPlateHeight;
        [StructuresDialog("PLEndPlateH", typeof(TD.Double))]
        public double PlateEndPlateHeight {
            get { return plateEndPlateHeight; }
            set {
                plateEndPlateHeight = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged("PlateEndPlateHeight");
            }
        }
        
        private string plateEndPlateMaterial;
        [StructuresDialog("PLEndPlateMATL", typeof(TD.String))]
        public string PlateEndPlateMaterial {
            get { return plateEndPlateMaterial; }
            set {
                plateEndPlateMaterial = value;
                OnPropertyChanged("PlateEndPlateMaterial");
            }
        }
        
        private string plateEndPlateFinish;
        [StructuresDialog("PLEndPlateFNSH", typeof(TD.String))]
        public string PlateEndPlateFinish {
            get { return plateEndPlateFinish; }
            set {
                plateEndPlateFinish = value;
                OnPropertyChanged("PlateEndPlateFinish");
            }
        }
        
        private int plateEndPlateClass;
        [StructuresDialog("PLEndPlateCLS", typeof(TD.Integer))]
        public int PlateEndPlateClass {
            get { return plateEndPlateClass; }
            set {
                plateEndPlateClass = value == int.MinValue ? 99 : value;
                OnPropertyChanged("PlateEndPlateClass");
            }
        }
        
        private string plateEndPlateAssemblyPrefix;
        [StructuresDialog("PLEndPlateASMP", typeof(TD.String))]
        public string PlateEndPlateAssemblyPrefix {
            get { return plateEndPlateAssemblyPrefix; }
            set {
                plateEndPlateAssemblyPrefix = value;
                OnPropertyChanged("PlateEndPlateAssemblyPrefix");
            }
        }
        
        private int plateEndPlateAssemblyStartNumber;
        [StructuresDialog("PLEndPlateASMN", typeof(TD.Integer))]
        public int PlateEndPlateAssemblyStartNumber {
            get { return plateEndPlateAssemblyStartNumber; }
            set {
                plateEndPlateAssemblyStartNumber = value == int.MinValue ? 1 : value;
                OnPropertyChanged("PlateEndPlateAssemblyStartNumber");
            }
        }
        
        private string plateEndPlatePartPrefix;
        [StructuresDialog("PLEndPlatePTP", typeof(TD.String))]
        public string PlateEndPlatePartPrefix {
            get { return plateEndPlatePartPrefix; }
            set {
                plateEndPlatePartPrefix = value;
                OnPropertyChanged("PlateEndPlatePartPrefix");
            }
        }
        
        private int plateEndPlatePartStartNumber;
        [StructuresDialog("PLEndPlatePTN", typeof(TD.Integer))]
        public int PlateEndPlatePartStartNumber {
            get { return plateEndPlatePartStartNumber; }
            set {
                plateEndPlatePartStartNumber = value == int.MinValue ? 1 : value;
                OnPropertyChanged("PlateEndPlatePartStartNumber");
            }
        }
}
~~~
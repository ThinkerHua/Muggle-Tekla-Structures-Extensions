# Muggle.TsExtensions.CodingHelper

---
## Contents

- [Generate fields for plugin data class](#generate-fields-for-plugin-data-class)
    - [Example](#example)
- [Generate fields for plugin class](#generate-fields-for-plugin-class)
  - [Fields and get field values method](#fields-and-get-field-values-method)
    - [Example](#example-1)
  - [Set field default values method](#set-field-default-values-method)
    - [Example](#example-2)
- [Generate properties for view model class](#generate-properties-for-view-model-class)
  - [General properties](#general-properties)
  - [Specific properties](#specific-properties)
  - [Example](#example-3)
- [Demo](#demo)
---
## Generate fields for plugin data class

Apply these attributes to plugin data class to auto generate fields (with StructuresFieldAttribute):

- PartFieldsAttribute
- PlateFieldsAttribute
- WeldFieldsAttribute
- BoltFieldsAttribute
- BoltCircleFieldsAttribute

> ***Note***: Mapping relationship between model object properties and plugin attribute name [see here](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md).

### Example

The source codes by hand:

~~~CSharp
// in PluginData.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[PlateFields("EndPlate")]
public partial class PluginData { }
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

---
## Generate fields for plugin class

### Fields and get field values method

Apply "**FieldsFromAttribute**" on the plugin class and pass the plugin data type, then it will auto generate private fields one-to-one corresponds with each public fields (including manually written fields and generated fields) in data type. And it will also generate a **"GetFieldValuesFrom"** method, you need to manually call this method at an appropriate location.

#### Example

The source codes by hand:

~~~CSharp
// in PluginData.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[PlateFields("End")]
public partial class PluginData { 

    [StructuresField("UselessAttribute")]
    public int UselessAttribute;
}


// in Plugin.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[Plugin("Plugin")]
[PluginUserInterface("Plugin.View")]
[FieldsFrom(typeof(PluginData))]
public partial class Plugin : PluginBase {

    public Plugin(PluginData data) {
        GetFieldValuesFrom(data);
    }
}
~~~

Then it will auto generate codes behind like this:

~~~CSharp
// in Plugin.g.cs

public partial class Plugin {
    
    private int _uselessAttribute;
    
    private string _plateEndPlateName;
    
    private double _plateEndPlateThickness;
    
    private double _plateEndPlateBreadth;
    
    private double _plateEndPlateHeight;
    
    private string _plateEndPlateMaterial;
    
    private string _plateEndPlateFinish;
    
    private int _plateEndPlateClass;
    
    private string _plateEndPlateAssemblyPrefix;
    
    private int _plateEndPlateAssemblyStartNumber;
    
    private string _plateEndPlatePartPrefix;
    
    private int _plateEndPlatePartStartNumber;

    private void GetFieldValuesFrom(PluginData data) {
        _uselessAttribute = data.UselessAttribute;

        _plateEndPlateName = data.PlateEndPlateName;
        _plateEndPlateThickness = data.PlateEndPlateThickness;
        _plateEndPlateBreadth = data.PlateEndPlateBreadth;
        _plateEndPlateHeight = data.PlateEndPlateHeight;
        _plateEndPlateMaterial = data.PlateEndPlateMaterial;
        _plateEndPlateFinish = data.PlateEndPlateFinish;
        _plateEndPlateClass = data.PlateEndPlateClass;
        _plateEndPlateAssemblyPrefix = data.PlateEndPlateAssemblyPrefix;
        _plateEndPlateAssemblyStartNumber = data.PlateEndPlateAssemblyStartNumber;
        _plateEndPlatePartPrefix = data.PlateEndPlatePartPrefix;
        _plateEndPlatePartStartNumber = data.PlateEndPlatePartStartNumber;
    }
}
~~~

### Set field default values method

Apply these attribute on the plugin class or its data field or property, then it will auto generate a **"SetDataToDefaultIfUnset"** method to register default value:

- PartFieldDefaultValuesAttribute
- PlateFieldDefaultValuesAttribute
- WeldFieldDefaultValuesAttribute
- BoltFieldDefaultValuesAttribute
- BoltCircleFieldDefaultValuesAttribute

> ***Note***: When applied on class, you need to apply "FieldsFromAttribute" also.

> ***Note***: You need to pay attention to the order of calling the "SetDataToDefaultIfUnset" method and the "GetFieldValuesFrom" method. Otherwise, you might not get the correct values.
> | If applied "FieldsFromAttribute" | Place "***FieldDefaultValuesAttribute" applied on | Calling order | Way to access data |
> | --- | --- | --- | --- |
> | Yes | Class | GetFieldValuesFrom(data);<br>SetDataToDefaultIfUnset(); | Only fields |
> | Yes | Data field or property | SetDataToDefaultIfUnset();<br>GetFieldValuesFrom(data); | Both fields and data field or property |
> | No | Data field or property | SetDataToDefaultIfUnset();<br>~~//GetFieldValuesFrom(data);~~ | Only data field or property |

#### Example

The source codes by hand:

~~~CSharp
// in Plugin.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[Plugin("Plugin")]
[PluginUserInterface("Plugin.View")]
[FieldsFrom(typeof(PluginData))]
public partial class Plugin : PluginBase {

    [PlateFieldDefaultValues("EndPlate", breadth: 300, thickness: 14, material: "Q235")]
    public PluginData Data { get; set; }

    public Plugin(PluginData data) {
        Data = data;
        SetDataToDefaultIfUnset();
        GetFieldValuesFrom(Data);
    }
}
~~~

Then it will auto generate codes behind like this:

~~~CSharp
// in Plugin.g.cs

public partial class Plugin {

    private void SetDataToDefaultIfUnset() {
        
        if (Data.PlateEndPlateThickness <= 0)
            Data.PlateEndPlateThickness = 14;
        if (Data.PlateEndPlateBreadth <= 0)
            Data.PlateEndPlateBreadth = 300;
        if (Data.PlateEndPlateHeight <= 0)
            Data.PlateEndPlateHeight = 0;
        if (IsDefaultValue(Data.PlateEndPlateMaterial))
            Data.PlateEndPlateMaterial = "Q235";
        if (IsDefaultValue(Data.PlateEndPlateName))
            Data.PlateEndPlateName = "";
        if (IsDefaultValue(Data.PlateEndPlateFinish))
            Data.PlateEndPlateFinish = "";
        if (IsDefaultValue(Data.PlateEndPlateClass))
            Data.PlateEndPlateClass = 99;
        if (IsDefaultValue(Data.PlateEndPlateAssemblyPrefix))
            Data.PlateEndPlateAssemblyPrefix = "A";
        if (IsDefaultValue(Data.PlateEndPlateAssemblyStartNumber))
            Data.PlateEndPlateAssemblyStartNumber = 1;
        if (IsDefaultValue(Data.PlateEndPlatePartPrefix))
            Data.PlateEndPlatePartPrefix = "P";
        if (IsDefaultValue(Data.PlateEndPlatePartStartNumber))
            Data.PlateEndPlatePartStartNumber = 1;
    }
}
~~~


---
## Generate properties for view model class

### General properties

Inherit view model from preset view model base to make it has ability to notify when its property changed and has some **general properties** (with StructuresDialogAttribute).

- NotificationObject
  
  A simple abstract class which implement INotifyPropertyChanged, inherit from it so you can use OnPropertyChanged method directly.

- ConnectionViewModel
  
  An abstract class inherit from NotificationObject, and has a several general properties for connection type plugin. 
  The use of properties is consistent with the Tekla Structures system connection component general tab.

- DetailViewModel
  
  An abstract class inherit from NotificationObject, and has a several general properties for detail type plugin. 
  The use of properties is consistent with the Tekla Structures system detail component general tab.

### Specific properties

Apply these attributes to view model class to auto generate properties (with StructuresDialogAttribute):

- PartPropertiesAttribute
- PlatePropertiesAttribute
- WeldPropertiesAttribute
- BoltPropertiesAttribute
- BoltCirclePropertiesAttribute

> ***Note***: Mapping relationship between model object properties and plugin attribute name 
[see here](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md).

### Example

The source codes by hand:

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

---
## Demo

You can get a complete demo project from [here](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/Demo1).

![Preview0](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/Resources/Introduction_Demo1_00.png)

![Preview1](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/Resources/Introduction_Demo1_01.png)

![Preview2](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/Resources/Introduction_Demo1_02.png)

![Preview3](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/Resources/Introduction_Demo1_03.png)

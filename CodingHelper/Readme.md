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

- GeneralFieldsAttribute
- PartFieldsAttribute
- PlateFieldsAttribute
- WeldFieldsAttribute
- BoltFieldsAttribute
- BoltCircleFieldsAttribute

> ***Note***: Mapping relationship between model object properties and plugin attribute names [see here](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md).

### Example

The source codes by hand:

~~~CSharp
// in PluginData.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[PlateFields("Stiffener")]
public partial class PluginData { }
~~~

Then it will auto generate codes behind like this:

~~~CSharp
//  in PluginData.g.cs

public partial class PluginData {
        
        [StructuresField("PLStiffenerNAME")]
        public string PlateStiffenerName;
        
        [StructuresField("PLStiffenerT")]
        public double PlateStiffenerThickness;
        
        [StructuresField("PLStiffenerB")]
        public double PlateStiffenerBreadth;
        
        [StructuresField("PLStiffenerH")]
        public double PlateStiffenerHeight;
        
        [StructuresField("PLStiffenerMATL")]
        public string PlateStiffenerMaterial;
        
        [StructuresField("PLStiffenerFNSH")]
        public string PlateStiffenerFinish;
        
        [StructuresField("PLStiffenerCLS")]
        public int PlateStiffenerClass;
        
        [StructuresField("PLStiffenerASMP")]
        public string PlateStiffenerAssemblyPrefix;
        
        [StructuresField("PLStiffenerASMN")]
        public int PlateStiffenerAssemblyStartNumber;
        
        [StructuresField("PLStiffenerPTP")]
        public string PlateStiffenerPartPrefix;
        
        [StructuresField("PLStiffenerPTN")]
        public int PlateStiffenerPartStartNumber;
}
~~~

---

## Generate fields for plugin class

### Fields and get field values method

Apply "**FieldsFromAttribute**" on the plugin class and input the plugin data type, then it will auto generate private fields one-to-one corresponds with each public fields (including manually written fields and generated fields) in data type. And it will also generate a **"GetFieldValuesFrom"** method, you need to manually call this method at an appropriate location.

#### Example

The source codes by hand:

~~~CSharp
// in PluginData.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[PlateFields("Stiffener")]
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
    
    private string _plateStiffenerName;
    private double _plateStiffenerThickness;
    private double _plateStiffenerBreadth;
    private double _plateStiffenerHeight;
    private string _plateStiffenerMaterial;
    private string _plateStiffenerFinish;
    private int _plateStiffenerClass;
    private string _plateStiffenerAssemblyPrefix;
    private int _plateStiffenerAssemblyStartNumber;
    private string _plateStiffenerPartPrefix;
    private int _plateStiffenerPartStartNumber;

    private void GetFieldValuesFrom(PluginData data) {
        _uselessAttribute = data.UselessAttribute;

        _plateStiffenerName = data.PlateStiffenerName;
        _plateStiffenerThickness = data.PlateStiffenerThickness;
        _plateStiffenerBreadth = data.PlateStiffenerBreadth;
        _plateStiffenerHeight = data.PlateStiffenerHeight;
        _plateStiffenerMaterial = data.PlateStiffenerMaterial;
        _plateStiffenerFinish = data.PlateStiffenerFinish;
        _plateStiffenerClass = data.PlateStiffenerClass;
        _plateStiffenerAssemblyPrefix = data.PlateStiffenerAssemblyPrefix;
        _plateStiffenerAssemblyStartNumber = data.PlateStiffenerAssemblyStartNumber;
        _plateStiffenerPartPrefix = data.PlateStiffenerPartPrefix;
        _plateStiffenerPartStartNumber = data.PlateStiffenerPartStartNumber;
    }
}
~~~

### Set field default values method

Apply these attributes on the plugin class or its data field or property, then it will auto generate a **"SetDataToDefaultIfUnset"** method to register default value:

- GeneralFieldDefaultValuesAttribute
- PartFieldDefaultValuesAttribute
- PlateFieldDefaultValuesAttribute
- WeldFieldDefaultValuesAttribute
- BoltFieldDefaultValuesAttribute
- BoltCircleFieldDefaultValuesAttribute

> ***Note***: When applied on class, you need to apply "FieldsFromAttribute" also.

> ***Note***: You need to pay attention to the order of calling the "SetDataToDefaultIfUnset" method and the "GetFieldValuesFrom" method. Otherwise, you might not get the correct values.
>
> | If applied "FieldsFromAttribute" | Place "***FieldDefaultValuesAttribute" applied on | Calling order                                                  | Way to access value                    |
> |----------------------------------|---------------------------------------------------|----------------------------------------------------------------|----------------------------------------|
> | Yes                              | Class                                             | GetFieldValuesFrom(data);<br/>SetDataToDefaultIfUnset();       | Only fields                            |
> | Yes                              | Data field or property                            | SetDataToDefaultIfUnset();<br/>GetFieldValuesFrom(data);       | Both fields and data field or property |
> | No                               | Data field or property                            | SetDataToDefaultIfUnset();<br/>~~//GetFieldValuesFrom(data);~~ | Only data field or property            |

#### Example

The source codes by hand:

~~~CSharp
// in Plugin.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[Plugin("Plugin")]
[PluginUserInterface("Plugin.View")]
[FieldsFrom(typeof(PluginData))]
public partial class Plugin : PluginBase {

    [PlateFieldDefaultValues("Stiffener", breadth: 300, thickness: 14, material: "Q235")]
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
        
        if (Data.PlateStiffenerThickness <= 0)
            Data.PlateStiffenerThickness = 14;
        if (Data.PlateStiffenerBreadth <= 0)
            Data.PlateStiffenerBreadth = 300;
        if (Data.PlateStiffenerHeight <= 0)
            Data.PlateStiffenerHeight = 0;
        if (IsDefaultValue(Data.PlateStiffenerMaterial))
            Data.PlateStiffenerMaterial = "Q235";
        if (IsDefaultValue(Data.PlateStiffenerName))
            Data.PlateStiffenerName = "";
        if (IsDefaultValue(Data.PlateStiffenerFinish))
            Data.PlateStiffenerFinish = "";
        if (IsDefaultValue(Data.PlateStiffenerClass))
            Data.PlateStiffenerClass = 99;
        if (IsDefaultValue(Data.PlateStiffenerAssemblyPrefix))
            Data.PlateStiffenerAssemblyPrefix = "A";
        if (IsDefaultValue(Data.PlateStiffenerAssemblyStartNumber))
            Data.PlateStiffenerAssemblyStartNumber = 1;
        if (IsDefaultValue(Data.PlateStiffenerPartPrefix))
            Data.PlateStiffenerPartPrefix = "P";
        if (IsDefaultValue(Data.PlateStiffenerPartStartNumber))
            Data.PlateStiffenerPartStartNumber = 1;
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

  An abstract class inherit from NotificationObject, and has a several general properties for connection type plugin. The usage of properties is consistent with the options in Tekla Structures system *connection* component general tab.

- DetailViewModel

  An abstract class inherit from NotificationObject, and has a several general properties for detail type plugin. The usage of properties is consistent with the options in Tekla Structures system *detail* component general tab.

### Specific properties

Apply these attributes to view model class to auto generate properties (with StructuresDialogAttribute) and set default values at the same time:

- GeneralPropertiesWithDefaultValuesAttribute
- PartPropertiesWithDefaultValuesAttribute
- PlatePropertiesWithDefaultValuesAttribute
- WeldPropertiesWithDefaultValuesAttribute
- BoltPropertiesWithDefaultValuesAttribute
- BoltCirclePropertiesWithDefaultValuesAttribute

> ***Note***: Currently, the attributes from the previous version are retained (the same names of the above but do not include "WithDefaultValues"), but they are not recommended for use and might be removed in later versions, because the default values in the old version are hard-coded, whereas the new version allows manual specification.

> ***Note***: Mapping relationship between model object properties and plugin attribute names [see here](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md).

#### Example

The source codes by hand:

~~~CSharp
// in MainWindowViewModel.cs

using Muggle.TsExtensions.CodingHelper.Generators;

[PlatePropertiesWithDefaultValues("Stiffener", 10, 200.0, 300.00,)]
public partial class MainWindowViewModel : ConnectionViewModel { 
    // ...
}
~~~

Then it will auto generate codes behind like this:

~~~CSharp
// in MainWindowViewModel.g.cs

public partial class MainWindowViewModel {

        private Tekla.Structures.Datatype.Double _plateStiffenerThickness;
        private Tekla.Structures.Datatype.Double _plateStiffenerBreadth;
        private Tekla.Structures.Datatype.Double _plateStiffenerHeight;
        private Tekla.Structures.Datatype.String _plateStiffenerMaterial;
        private Tekla.Structures.Datatype.String _plateStiffenerName;
        private Tekla.Structures.Datatype.String _plateStiffenerFinish;
        private Tekla.Structures.Datatype.Integer _plateStiffenerClass;
        private Tekla.Structures.Datatype.String _plateStiffenerAssemblyPrefix;
        private Tekla.Structures.Datatype.Integer _plateStiffenerAssemblyStartNumber;
        private Tekla.Structures.Datatype.String _plateStiffenerPartPrefix;
        private Tekla.Structures.Datatype.Integer _plateStiffenerPartStartNumber;
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerT", typeof(Tekla.Structures.Datatype.Double))]
        public Tekla.Structures.Datatype.Double PlateStiffenerThickness {
            get {
                return _plateStiffenerThickness;
            }
            set {
                _plateStiffenerThickness = value == int.MinValue ? 10 : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerB", typeof(Tekla.Structures.Datatype.Double))]
        public Tekla.Structures.Datatype.Double PlateStiffenerBreadth {
            get {
                return _plateStiffenerBreadth;
            }
            set {
                _plateStiffenerBreadth = value == int.MinValue ? 200 : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerH", typeof(Tekla.Structures.Datatype.Double))]
        public Tekla.Structures.Datatype.Double PlateStiffenerHeight {
            get {
                return _plateStiffenerHeight;
            }
            set {
                _plateStiffenerHeight = value == int.MinValue ? 300 : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerMATL", typeof(Tekla.Structures.Datatype.String))]
        public Tekla.Structures.Datatype.String PlateStiffenerMaterial {
            get {
                return _plateStiffenerMaterial;
            }
            set {
                _plateStiffenerMaterial = string.IsNullOrEmpty(value) ? "" : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerNAME", typeof(Tekla.Structures.Datatype.String))]
        public Tekla.Structures.Datatype.String PlateStiffenerName {
            get {
                return _plateStiffenerName;
            }
            set {
                _plateStiffenerName = string.IsNullOrEmpty(value) ? "" : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerFNSH", typeof(Tekla.Structures.Datatype.String))]
        public Tekla.Structures.Datatype.String PlateStiffenerFinish {
            get {
                return _plateStiffenerFinish;
            }
            set {
                _plateStiffenerFinish = string.IsNullOrEmpty(value) ? "" : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerCLS", typeof(Tekla.Structures.Datatype.Integer))]
        public Tekla.Structures.Datatype.Integer PlateStiffenerClass {
            get {
                return _plateStiffenerClass;
            }
            set {
                _plateStiffenerClass = value == int.MinValue ? 99 : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerASMP", typeof(Tekla.Structures.Datatype.String))]
        public Tekla.Structures.Datatype.String PlateStiffenerAssemblyPrefix {
            get {
                return _plateStiffenerAssemblyPrefix;
            }
            set {
                _plateStiffenerAssemblyPrefix = string.IsNullOrEmpty(value) ? "A-" : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerASMN", typeof(Tekla.Structures.Datatype.Integer))]
        public Tekla.Structures.Datatype.Integer PlateStiffenerAssemblyStartNumber {
            get {
                return _plateStiffenerAssemblyStartNumber;
            }
            set {
                _plateStiffenerAssemblyStartNumber = value == int.MinValue ? 1 : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerPTP", typeof(Tekla.Structures.Datatype.String))]
        public Tekla.Structures.Datatype.String PlateStiffenerPartPrefix {
            get {
                return _plateStiffenerPartPrefix;
            }
            set {
                _plateStiffenerPartPrefix = string.IsNullOrEmpty(value) ? "P" : value;
                OnPropertyChanged();
            }
        }
        
        [Tekla.Structures.Dialog.StructuresDialog("PLStiffenerPTN", typeof(Tekla.Structures.Datatype.Integer))]
        public Tekla.Structures.Datatype.Integer PlateStiffenerPartStartNumber {
            get {
                return _plateStiffenerPartStartNumber;
            }
            set {
                _plateStiffenerPartStartNumber = value == int.MinValue ? 1 : value;
                OnPropertyChanged();
            }
        }
}
~~~

---

## Demo

You can get a complete demo project from [here](https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/Demo1).

![Preview0](https://raw.githubusercontent.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/master/Resources/Introduction_Demo1_00.png)

![Preview1](https://raw.githubusercontent.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/master/Resources/Introduction_Demo1_01.png)

![Preview2](https://raw.githubusercontent.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/master/Resources/Introduction_Demo1_02.png)

![Preview3](https://raw.githubusercontent.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/master/Resources/Introduction_Demo1_03.png)

![Preview4](https://raw.githubusercontent.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/master/Resources/Introduction_Demo1_04.png)

![Preview5](https://raw.githubusercontent.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/master/Resources/Introduction_Demo1_05.png)

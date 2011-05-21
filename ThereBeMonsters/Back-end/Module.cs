using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTKGUI;
using ThereBeMonsters.Front_end;

namespace ThereBeMonsters.Back_end
{
  /// <summary>
  /// Base class for all (internal) procedural content generator modules.
  /// Please make sure a default (no-argument) construtor is accessible,
  /// otherwise the Generator will not be able to dynamically instantiate the module.
  /// </summary>
  public abstract class Module
  {
    /// <summary>
    /// Override and put any computation code in this method (see remarks).
    /// </summary>
    /// <remarks>
    /// Some notes on the behavior of the generator:
    /// 
    /// By default, an instance of a Module can be reused, in which case this method can
    /// be called multiple times in an instance's lifetime (Use [Module(Reusable = false)]
    /// above the class declaration to prevent this behavior).
    /// 
    /// Input parameters are set before Run() is called. All input parameters will have their
    /// set accessor called once, unless the parameter is marked optional, in which case it
    /// may or may not be called.
    /// 
    /// After Run() returns, any output parameters referenced by other modules will have their
    /// get accessors called once and only once. Thus, it is possible to move some computation
    /// out of the Run() method and into the get methods if that computation is only necessary
    /// if that computation only needs to run if the output parameter is used. Note there's no
    /// garentee for what order output parameters are accessed.
    /// </remarks>
    public abstract void Run();

    #region Properties

    public static Random rng = new Random();

    /// <summary>
    /// Retrieves the description of the module (from the [Module] attribute), if any.
    /// </summary>
    public string Description
    {
      get
      {
        ModuleAttribute attrib = Attribute.GetCustomAttribute(this.GetType(), typeof(ModuleAttribute)) as ModuleAttribute;
        return attrib != null ? attrib.Description : string.Empty;
      }
    }

    /// <summary>
    /// Retrieves whether this module is reusable (from the [Module] attribute).
    /// By default the same instance can be re-run multiple times with different
    /// parameters. If you want to prevent this behavior, please put above your
    /// class defintion: [Module(Reusable = false)]
    /// </summary>
    public bool IsReusable
    {
      get
      {
        ModuleAttribute attrib = Attribute.GetCustomAttribute(this.GetType(), typeof(ModuleAttribute)) as ModuleAttribute;
        return attrib != null ? attrib.Reusable : true;
      }
    }

    /// <summary>
    /// Indicates whether this class overrides the indexer and accepts parameters not present in
    /// the Parameters property. Defaults to false.
    /// </summary>
    public bool DynamicParameters
    {
      get
      {
        ModuleAttribute attrib = Attribute.GetCustomAttribute(this.GetType(), typeof(ModuleAttribute)) as ModuleAttribute;
        return attrib != null ? attrib.DynamicParameters : false;
      }
    }

    /// <summary>
    /// A dictionary of this module's parameters, by name.
    /// Do not change the contents of this dictionary,
    /// it's instance is shared with all existing and future Modules of this type.
    /// </summary>
    public Dictionary<string, Parameter> Parameters { get; protected set; }

    /// <summary>
    /// Gets or sets a module parameter by name.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <returns></returns>
    public object this[string name]
    {
      get
      {
        Parameter param = this.Parameters[name];
        if (param == null)
        {
          throw new KeyNotFoundException("Parameter does not exist.");
        }

        return param.Property.GetValue(this, null); // TODO: more friendly error handling?
      }
      set
      {
        Parameter param = this.Parameters[name];
        if (param == null)
        {
          throw new KeyNotFoundException("Parameter does not exist.");
        }

        param.Property.SetValue(this, value, null);
      }
    }

    #endregion

    #region Constructors and static helpers

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected Module()
    {
      this.Parameters = this.GetModuleParameters();
    }

    /// <summary>
    /// Returns a Dictionary of module parameters for this instance.
    /// </summary>
    /// <returns></returns>
    public virtual Dictionary<string, Parameter> GetModuleParameters()
    {
      return Module.GetModuleParameters(GetType());
    }

    private static Dictionary<Type, Dictionary<string, Parameter>> parametersCache = new Dictionary<Type, Dictionary<string, Parameter>>();

    /// <summary>
    /// Scans the specified type's properties and constructs the parameter list.
    /// </summary>
    public static Dictionary<string, Parameter> GetModuleParameters(Type instanceType)
    {
      if (parametersCache.ContainsKey(instanceType))
      {
        return parametersCache[instanceType];
      }

      Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
      Parameter param;
      Parameter.IODirection iodir;
      foreach (PropertyInfo property in instanceType.GetProperties())
      {
        if (property.DeclaringType == typeof(Module)
            || Attribute.GetCustomAttribute(property, typeof(NonParameterAttribute)) != null)
        {
          continue;
        }

        param = Attribute.GetCustomAttribute(property, typeof(Parameter)) as Parameter;
        if (param != null)
        {
          param.Property = property;
          parameters[param.Name] = param;
          if (param.Direction != Parameter.IODirection.AUTO)
          {
            continue;
          }
        }

        // Auto-detection
        iodir = Parameter.IODirection.NONE;
        if (property.GetGetMethod() != null)
        {
          iodir |= Parameter.IODirection.OUTPUT;
        }
          
        if (property.GetSetMethod() != null)
        {
          iodir |= Parameter.IODirection.INPUT;
        }

        if (param != null)
        {
          param.Direction = iodir;
        }
        else
        {
          parameters[property.Name] = new Parameter(iodir, property);
        }
      }

      parametersCache[instanceType] = parameters;
      return parameters;
    }

    /// <summary>
    /// Indicates whether the specified type uses dynamic parameters.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the [Module] tag specifies DynamicParameters = true, false otherwise.</returns>
    public static bool UsesDynamicParameters(Type type)
    {
      ModuleAttribute attrib = Attribute.GetCustomAttribute(type, typeof(ModuleAttribute)) as ModuleAttribute;
      return attrib != null ? attrib.DynamicParameters : false;
    }

    #endregion

    #region Nested Classes

    /// <summary>
    /// Use this attribute on a property that should not be treated as a module parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NonParameterAttribute : Attribute { }

    /// <summary>
    /// Optional attribute for adding metadata to a parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Parameter : Attribute
    {
      [Flags]
      public enum IODirection
      {
        /// <summary>
        /// Default; the Module class will automatically replace the type with appropriate value
        /// based on the public accessors for the property.
        /// </summary>
        AUTO = 0x4,

        /// <summary>
        /// Used when a Parameter is generated on a property (without a Parameter attribute
        /// or a NonParameter attribute), but the property does not have a public getter or setter.
        /// Please use [NonParameter] on such properties. so they do not become parameters.
        /// </summary>
        NONE = 0x0,

        INPUT = 0x1,
        OUTPUT = 0x2,
        INOUT = INPUT | OUTPUT
      }

      public delegate EditorControl EditorFactoryDelegate(
        ModuleNodeControl nodeControl, string paramName);

      /// <summary>
      /// The name of this parameter (the name of its underlying property).
      /// </summary>
      public string Name
      {
        get
        {
          return Property != null ? Property.Name : string.Empty;
        }
      }

      /// <summary>
      /// Indicates the input/output disposition of this parameter.
      /// </summary>
      public IODirection Direction { get; set; }

      /// <summary>
      /// A description for the user about this parameter.
      /// </summary>
      public string Description { get; set; }

      /// <summary>
      /// The type of the parameter.
      /// </summary>
      public Type Type
      {
        get
        {
          return this.Property.PropertyType;
        }
      }

      /// <summary>
      /// Indicates whether this parameter needs to have a value specified.
      /// </summary>
      public bool Optional { get; set; }

      /// <summary>
      /// Indicates whether this parameter should be hidden from the editor (e.g. a custom
      /// editor for another parameter also changes this parameter).
      /// </summary>
      public bool Hidden { get; set; }

      /// <summary>
      /// Specifies a custom editor GUI control for this parameter.
      /// </summary>
      public Type Editor { get; set; }

      // TODO: the factory pattern doesn't seem like it would be useful?
      public EditorFactoryDelegate EditorFactory { get; set; }

      /// <summary>
      /// The PropertyInfo associated with this parameter. Set by the Module base class.
      /// </summary>
      public PropertyInfo Property { get; set; }

      public Parameter()
      {
        this.Direction = IODirection.AUTO;
      }

      /// <summary>
      /// Specifies a description for this parameter. The type of parameter will be auto-detected
      /// based on the publically accessible accessors.
      /// </summary>
      /// <param name="description">The description.</param>
      public Parameter(string description)
      {
        this.Description = description;
        this.Direction = IODirection.AUTO;
      }

      /// <summary>
      /// Used by the Module base class when a property does not have a Parameter attribute associated with it.
      /// </summary>
      /// <param name="dir">Input/output disposition.</param>
      /// <param name="prop">The property.</param>
      public Parameter(IODirection dir, PropertyInfo prop)
      {
        this.Direction = dir;
        this.Property = prop;
      }

      public EditorControl GetEditorInstance(ModuleNodeControl control)
      {
        if (this.EditorFactory != null)
        {
          return this.EditorFactory(control, this.Name);
        }
        
        if (this.Editor != null)
        {
          return (EditorControl)Activator.CreateInstance(this.Editor, control, this.Name);
        }

        return EditorControl.CreateDefaultEditorInstanceFor(this.Type, control, this.Name);
      }
    }

    /// <summary>
    /// Optional attribute for adding metadata to a module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleAttribute : Attribute
    {
      /// <summary>
      /// Description of the module.
      /// </summary>
      public string Description { get; set; }

      /// <summary>
      /// Indicates whether the same instance of this Module can be invoked multiple times with different parameters.
      /// Defaults to true. If you want a new instance to be constructed each time this module is used, set this to false.
      /// </summary>
      public bool Reusable { get; set; }

      /// <summary>
      /// Indicates whether this class overrides the indexer and accepts parameters not present in
      /// the Parameters property. Defaults to false.
      /// </summary>
      public bool DynamicParameters { get; set; }

      public ModuleAttribute(string description = "")
      {
        this.Description = description;
        this.Reusable = true;
        this.DynamicParameters = false;
      }
    }

    #endregion
  }
}

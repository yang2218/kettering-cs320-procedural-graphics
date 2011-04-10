using System;
using System.Collections.Generic;
using System.Reflection;

// TODO: support fields instead of just properties?

// other ideas:
// add optional methods, etc. for supporting a node graph editing UI?
//   (e.g. functions for user feedback, attributes for realtime previewing, etc.)

namespace ThereBeMonsters.Back_end
{
  /// <summary>
  /// Base class for all (internal) procedural content generator modules.
  /// </summary>
  public abstract class Module
  {
    /// <summary>
    /// Use this attribute on a property that should not be treated as a module parameter.
    /// </summary>
    public class NonParameter : Attribute { }

    /// <summary>
    /// Attribute for adding a description, and optionally overriding the type of module parameter.
    /// </summary>
    public class Parameter : Attribute
    {
      public enum IODirection
      {
        /// <summary>
        /// Default; the Module class will automatically replace the type with appropriate value
        /// based on the public accessors for the property.
        /// </summary>
        AUTO,
        NONE,
        INPUT,
        OUTPUT,
        INOUT
      }

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
      public string Description { get; private set; }

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
      public bool Optional { get; private set; }

      /// <summary>
      /// The PropertyInfo associated with this parameter. Set by the Module base class.
      /// </summary>
      public PropertyInfo Property { get; set; }

      /// <summary>
      /// Specifies a description for this parameter. The type of parameter will be auto-detected
      /// based on the publically accessible accessors.
      /// </summary>
      /// <param name="desc">The description.</param>
      /// <param name="optional">If false (or left to default), the module will not run
      /// without a value specified for this parameter.</param>
      /// <param name="direction">Input/output disposition (defaults to Auto).</param>
      public Parameter(string desc, bool optional = false, IODirection direction = IODirection.AUTO)
      {
        this.Description = desc;
        this.Optional = optional;
        this.Direction = direction;
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
    }

    public class ModuleAttribute : Attribute
    {
      /// <summary>
      /// Description of the module.
      /// </summary>
      public string Description { get; private set; }

      public ModuleAttribute(string desc)
      {
        this.Description = desc;
      }
    }

    /// <summary>
    /// Default constructor; runs Setup() to detect what runtime properties are module parameters.
    /// </summary>
    protected Module()
    {
      Setup();
    }

    /// <summary>
    /// A dictionary of this module's parameters, by name.
    /// </summary>
    public Dictionary<string, Parameter> Parameters { get; protected set; }

    /// <summary>
    /// Retrieves the description of the module, if any.
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

        return param.Property.GetValue(this, null);
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

    /// <summary>
    /// Scans this instance's properties and constructs the parameter list.
    /// </summary>
    private void Setup()
    {
      this.Parameters = new Dictionary<string, Parameter>();

      Type instanceType = GetType();
      Parameter param;
      Parameter.IODirection iodir;
      foreach (PropertyInfo property in instanceType.GetProperties())
      {
        if (property.DeclaringType == typeof(Module)
            || Attribute.GetCustomAttribute(property, typeof(NonParameter)) != null)
        {
          continue;
        }

        param = Attribute.GetCustomAttribute(property, typeof(Parameter)) as Parameter;
        if (param != null)
        {
          param.Property = property;
          this.Parameters[param.Name] = param;
          if (param.Direction != Parameter.IODirection.AUTO)
          {
            continue;
          }
        }

        // Auto-detection
        iodir = Parameter.IODirection.NONE;
        switch (property.GetAccessors().Length)
        {
          case 1:
            if (property.GetGetMethod() != null)
            {
              iodir = Parameter.IODirection.OUTPUT;
            }
            else
            {
              iodir = Parameter.IODirection.INPUT;
            }
            break;
          case 2:
            iodir = Parameter.IODirection.INOUT;
            break;
        }

        if (param != null)
        {
          param.Direction = iodir;
        }
        else
        {
          this.Parameters[property.Name] = new Parameter(iodir, property);
        }
      }
    }

    /// <summary>
    /// Called after input parameters are initialized.
    /// </summary>
    public abstract void Run();
  }
}

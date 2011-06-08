using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end.Controls
{
  public abstract class EditorControl : Control
  {
    public static Dictionary<Type, Type> _editorRegistry =
      new Dictionary<Type, Type> {
        {typeof(bool), typeof(BoolEditor)},
        {typeof(Enum), typeof(EnumControl)},
        {typeof(float), typeof(FloatControl)},
        {typeof(int), typeof(IntControl)},
        {typeof(string), typeof(StringControl)}
      };
    
    public ModuleNodeControl NodeControl { get; protected set; }
    public string ParameterName { get; protected set; }

    public abstract double PreferredHeight { get; }

    public Control Client { get; protected set; }

    public EditorControl(ModuleNodeControl parentNode, string paramName)
    {
      this.NodeControl = parentNode;
      this.ParameterName = paramName;
      parentNode.Node.ParameterUpdated += OnValueChangedPrefiltered;
      // TODO: do we have to worry about unsubscribing? ModuleNode will probably
      // be GC'd anyway...
    }

    public static EditorControl CreateDefaultEditorInstanceFor(
      Type type, ModuleNodeControl control, string parameterName)
    {
      Type editorType;
      // Enum classes are always subclassed -- maybe dictionary isn't the way to go...
      if (type.BaseType == typeof(Enum))
      {
        editorType = _editorRegistry[typeof(Enum)];
      }
      else if (_editorRegistry.TryGetValue(type, out editorType) == false)
      {
        return null;
      }

      return (EditorControl)Activator.CreateInstance(editorType,
        new object[]{ control, parameterName });
    }

    public virtual void OnValueChangedPrefiltered(object sender, ModuleParameterEventArgs e)
    {
      if (e.ParameterName == this.ParameterName)
      {
        OnValueChanged(sender, e);
      }
    }

    // WARNING: this will be called after you call SetModuleParameterValue
    // DO NOT invoke SetModuleParameterValue or otherwise change the ModuleNode's
    // parameter from this method (infinite feedback loop)
    public abstract void OnValueChanged(object sender, ModuleParameterEventArgs e);

    /// <summary>
    /// Gets or sets the value of the module parameter this editor is associated with.
    /// </summary>
    protected object ModuleParameterValue
    {
      get
      {
        object value = this.NodeControl.Node[ParameterName];
        if (value != null && value.GetType() != typeof(ParameterWireup))
        {
          return value;
        }

        return null;
      }
      set
      {
        this.NodeControl.Node[ParameterName] = value;
      }
    }

    protected object GetModuleParameterValue(string otherParamName)
    {
      return this.NodeControl.Node[otherParamName];
    }

    protected void SetModuleParameterValue(string otherParamName, object value)
    {
      this.NodeControl.Node[otherParamName] = value;
    }

    public override void Render(GUIRenderContext Context)
    {
      Client.Render(Context);
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      Client.Update(Context.CreateChildContext(Client, new Point(0.0, 0.0)), Time);
    }

    protected override void OnResize(Point Size)
    {
      this.ResizeChild(Client, Size);
    }

    protected override void OnDispose()
    {
      Client.Dispose();
    }
  }
}

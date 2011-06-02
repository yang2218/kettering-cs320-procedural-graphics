using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Drawing;
using OpenTK;
using System.Xml.Schema;
using System.IO;

namespace ThereBeMonsters.Back_end
{
  #region Helper classes

  public struct ParameterWireup
  {
    [XmlAttribute]
    public string srcId;
    [XmlAttribute]
    public string srcParam;

    public ParameterWireup(string id, string param)
    {
      this.srcId = id;
      this.srcParam = param;
    }
  }

  public class ModuleEventArgs : EventArgs
  {
    public string ModuleId { get; set; }
    public ModuleEventArgs(string id)
    {
      this.ModuleId = id;
    }
  }

  public class ModuleParameterEventArgs : EventArgs
  {
    public string ParameterName { get; set; }

    public ModuleParameterEventArgs(string name)
    {
      this.ParameterName = name;
    }
  }

  public class ModuleMovedEventArgs : ModuleEventArgs
  {
    public float X { get; set; }
    public float Y { get; set; }

    public ModuleMovedEventArgs(string id, float x, float y)
      : base(id)
    {
      this.X = x;
      this.Y = y;
    }
  }

  public static class XmlHelper
  {
    private static XmlSerializerNamespaces _ns;
    public static XmlSerializerNamespaces Ns
    {
      get
      {
        if (_ns == null)
        {
          _ns = new XmlSerializerNamespaces();
          _ns.Add("", "");
        }

        return _ns;
      }
    }
  }

  public class Wireups : Dictionary<string, object>, IXmlSerializable
  {
    public XmlSchema GetSchema()
    {
      return null;
    }

    private static Dictionary<string, XmlSerializer> _serializers
      = new Dictionary<string, XmlSerializer>();

    public void ReadXml(System.Xml.XmlReader reader)
    {
      bool wasEmpty = reader.IsEmptyElement;
      reader.Read();

      if (wasEmpty)
      {
        return;
      }

      string key, type;
      XmlSerializer xs;
      while (reader.NodeType != XmlNodeType.EndElement)
      {
        key = reader.GetAttribute("name");
        type = reader.GetAttribute("type");
        reader.ReadStartElement("Wireup");
        if (_serializers.TryGetValue(type, out xs) == false)
        {
          _serializers[type] = xs = new XmlSerializer(Type.GetType(type));
        }

        Add(key, xs.Deserialize(reader));

        reader.ReadEndElement();
        reader.MoveToContent();
      }

      reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
      foreach (KeyValuePair<string, object> kvp in this)
      {
        try
        {
          XmlSerializer xs = new XmlSerializer(kvp.Value.GetType());
          // if a value is not serializable by xml, skip it (the constructor should
          // error if there's a problem, otherwise xs.Serialize shoudl work)

          writer.WriteStartElement("Wireup");
          writer.WriteAttributeString("name", kvp.Key);
          writer.WriteAttributeString("type", kvp.Value.GetType().FullName);
          xs.Serialize(writer, kvp.Value, XmlHelper.Ns);
          writer.WriteEndElement();
        }
        catch (Exception)
        {
        }
      }
    }
  }

  #endregion

  [XmlRoot("Module")]
  public class ModuleNode
  {
    [XmlAttribute("id")]
    public string ModuleId { get; set; }
    [XmlIgnore]
    public Type ModuleType { get; set; }
    [XmlAttribute("description")]
    public string Description { get; set; }

    [XmlAttribute("type")]
    public string ModuleTypeName
    { // Type is not serializable, so serialize the name instead
      get
      {
        return ModuleType.FullName;
        // TODO: if ModuleType not from the current assembly, use assembly-qualified name
      }
      set
      {
        ModuleType = Type.GetType(value);
      }
    }

    public Wireups Wireups { get; set; }

    private Vector2 _position;
    public Vector2 Position
    {
      get
      {
        return _position;
      }
      set
      {
        _position = value;
        FireMovedEvents();
      }
    }

    [XmlIgnore]
    public float X
    {
      get
      {
        return _position.X;
      }
      set
      {
        _position.X = value;
        FireMovedEvents();
      }
    }
    [XmlIgnore]
    public float Y
    {
      get
      {
        return _position.Y;
      }
      set
      {
        _position.Y = value;
        FireMovedEvents();
      }
    }

    public event EventHandler<ModuleParameterEventArgs> ParameterUpdated;
    public event EventHandler<ModuleMovedEventArgs> Moved;

    public ModuleNode()
    {
      this.Wireups = new Wireups();
    }

    public void Add(string parameterName, object value)
    {
      Wireups.Add(parameterName, value);

      if (ParameterUpdated != null)
      {
        ParameterUpdated(this, new ModuleParameterEventArgs(parameterName));
      }
    }

    public void Add(string parameterName, string srcId, string srcParam)
    {
      Wireups.Add(parameterName, new ParameterWireup {
        srcId = srcId,
        srcParam = srcParam
      });

      ParameterUpdated(this, new ModuleParameterEventArgs(parameterName));
    }

    public object this[string parameterName]
    {
      get
      {
        object temp;
        if (Wireups.TryGetValue(parameterName, out temp))
        {
          return temp;
        }

        return Module.GetModuleParameters(ModuleType)[parameterName].Default;
      }
      set
      {
        Wireups[parameterName] = value;

        if (ParameterUpdated != null)
        {
          ParameterUpdated(this, new ModuleParameterEventArgs(parameterName));
        }
      }
    }

    public IEnumerable<ParameterWireup> ParameterWireups
    {
      get
      {
        foreach (object o in Wireups.Values)
        {
          if (o != null && o.GetType() == typeof(ParameterWireup))
          {
            yield return (ParameterWireup)o;
          }
        }
        yield break;
      }
    }

    private void FireMovedEvents()
    {
      if (Moved != null)
      {
        Moved(this, new ModuleMovedEventArgs(
          this.ModuleId,
          _position.X,
          _position.Y
        ));
      }
    }
  }

  // hack for serializing/deserializing the ModuleNode dictionary in ModuleGraph
  public class Nodes : List<ModuleNode>
  {
    private Dictionary<string, ModuleNode> _nodes;
    public Nodes(Dictionary<string, ModuleNode> d)
    {
      this._nodes = d;
      foreach (ModuleNode n in d.Values)
      {
        base.Add(n);
      }
    }

    public new void Add(ModuleNode n)
    {
      _nodes[n.ModuleId] = n;
    }
  }

  public class ModuleGraph
  {
    public event EventHandler<ModuleEventArgs> ModuleAdded;
    public event EventHandler<ModuleEventArgs> ModuleRemoved;
    public event EventHandler<ModuleMovedEventArgs> ModuleMoved;

    [XmlIgnore] // can't serialize dictionaries, so NodeValues was created
    public Dictionary<string, ModuleNode> Nodes { get; private set; }

    [XmlElement("Module")]
    public Nodes NodeValues
    {
      get
      {
        return new Nodes(this.Nodes);
      }
    }

    [XmlAttribute("name")]
    public string Name { get; set; }

    private static XmlSerializer _moduleGraphSerializer;
    private static XmlSerializer ModuleGraphSerializer
    {
      get
      {
        return _moduleGraphSerializer
          ?? (_moduleGraphSerializer = new XmlSerializer(typeof(ModuleGraph)));
      }
    }

    public static ModuleGraph LoadFromXml(string filePath)
    {
      XmlReaderSettings settings = new XmlReaderSettings();
      settings.IgnoreWhitespace = true;
      using (XmlReader reader = XmlReader.Create(filePath, settings))
      {
        return (ModuleGraph)ModuleGraphSerializer.Deserialize(reader);
      }
    }

    public ModuleGraph()
    {
      Nodes = new Dictionary<string, ModuleNode>();
    }

    public void SaveToXml(string filePath)
    {
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.IndentChars = "  ";
      using (XmlWriter writer = XmlWriter.Create(filePath, settings))
      {
        ModuleGraphSerializer.Serialize(writer, this, XmlHelper.Ns);
      }
    }

    public void Add(string moduleId, Type moduleType, string description = "")
    {
      ModuleNode node = new ModuleNode
      {
        ModuleType = moduleType,
        Description = description
      };

      // Add will throw an exception in the case of a duplicate key (desired)
      Nodes.Add(moduleId, node);
      node.Moved += OnModuleMoved;

      if (ModuleAdded != null)
      {
        ModuleAdded(this, new ModuleEventArgs(moduleId));
      }
    }

    public void Remove(string moduleId)
    {
      ModuleNode node;
      if (Nodes.TryGetValue(moduleId, out node) == false)
      {
        return;
      }

      foreach (ModuleNode n in Nodes.Values)
      {
        foreach (KeyValuePair<string, object> kvp in n.Wireups)
        {
          if (kvp.Value != null && kvp.Value.GetType() == typeof(ParameterWireup)
            && ((ParameterWireup)kvp.Value).srcId == moduleId)
          {
            n[kvp.Key] = null;
          }
        }
      }

      node.Moved -= OnModuleMoved;
      Nodes.Remove(moduleId);
      if (ModuleRemoved != null)
      {
        ModuleRemoved(this, new ModuleEventArgs(moduleId));
      }
    }

    private void OnModuleMoved(object sender, ModuleMovedEventArgs e)
    {
      if (ModuleMoved != null)
      {
        ModuleMoved(sender, e);
      }
    }

    /// <summary>
    /// Gets or sets the ModuleNode with the specified ID.
    /// </summary>
    /// <param name="moduleId">The unique string identifier for the module node.</param>
    /// <returns>The module node.</returns>
    public ModuleNode this[string moduleId]
    {
      get
      {
        return Nodes[moduleId];
      }
      set
      {
        Nodes[moduleId] = value;
      }
    }

    /// <summary>
    /// Gets or sets the specified parameter of the specified module.
    /// </summary>
    /// <param name="moduleId">The unique string identifier for the module node.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>Either a ParameterWireup struct, any object value, or null</returns>
    public object this[string moduleId, string parameterName]
    {
      get
      {
        return this[moduleId][parameterName];
      }
      set
      {
        this[moduleId][parameterName] = value;
      }
    }

  }
}

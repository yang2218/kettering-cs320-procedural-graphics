using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace ThereBeMonsters.Back_end
{
  public struct ParameterWireup
  {
    public string srcId;
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
  }

  public class ModuleParameterEventArgs : EventArgs
  {
    public string ParameterName { get; set; }
  }

  public delegate void ModuleAddedHandler(object sender, ModuleEventArgs e);
  public delegate void ModuleRemovedHandler(object sender, ModuleEventArgs e);
  public delegate void ModuleParameterChangedHandler(object sender, ModuleParameterEventArgs e);

  public class ModuleNode
  {
    public string ModuleId { get; set; }
    public Type ModuleType { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> Wireups { get; private set; }
    public float X { get; set; }
    public float Y { get; set; }

    public event ModuleParameterChangedHandler ParameterUpdated;

    public ModuleNode()
    {
      Wireups = new Dictionary<string, object>();
    }

    public void Add(string parameterName, object value)
    {
      Wireups.Add(parameterName, value);

      ParameterUpdated(this, new ModuleParameterEventArgs {
        ParameterName = parameterName
      });
    }

    public void Add(string parameterName, string srcId, string srcParam)
    {
      Wireups.Add(parameterName, new ParameterWireup {
        srcId = srcId,
        srcParam = srcParam
      });

      ParameterUpdated(this, new ModuleParameterEventArgs {
        ParameterName = parameterName
      });
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

        ParameterUpdated(this, new ModuleParameterEventArgs {
          ParameterName = parameterName
        });
      }
    }

    public IEnumerable<ParameterWireup> ParameterWireups
    {
      get
      {
        foreach (object o in Wireups.Values)
        {
          if (o.GetType() == typeof(ParameterWireup))
          {
            yield return (ParameterWireup)o;
          }
        }
      }
    }
  }

  public class ModuleGraph
  {
    public event ModuleAddedHandler ModuleAdded;
    public event ModuleRemovedHandler ModuleRemoved;

    // TODO: refactor how the module graph is stored:
    //  create a ModuleModel class, each module keeps its incoming wireups and values
    //  (using dictionary or maybe HybridDictionary) so duplicate wireups are more preventable
    public Dictionary<string, ModuleNode> Nodes { get; private set; }
    
    public ModuleGraph()
    {
      Nodes = new Dictionary<string, ModuleNode>();
    }

    public ModuleGraph(string filePath)
      : this()
    {
      LoadFromXml(filePath);
    }

    #region Loading from XML methods

    public void LoadFromXml(string filePath)
    {
      LoadFromXml(new XPathDocument(filePath).CreateNavigator());
    }

    private void LoadFromXml(XPathNavigator nav)
    {
      XPathNodeIterator it = nav.Select("/ModuleGraph/Module");
      Queue<XPathNavigator> wireupLoadQueue = new Queue<XPathNavigator>();
      Type typeObject;
      string id, type, description;
      while (it.MoveNext())
      {
        if (GetAttribute(it, "id", out id, "Missing attribute 'id'") == false)
        {
          continue;
        }

        if (Nodes.ContainsKey(id))
        {
          LogError("Duplicate id", it);
          continue;
        }

        if (GetAttribute(it, "type", out type, "Missing attribute 'type'") == false)
        {
          continue;
        }

        if (type.Contains(".") == false)
        {
          type = "ThereBeMonsters.Back_end.Modules." + type;
        }

        typeObject = Type.GetType(type, false, true);
        if (typeObject == null || typeObject.IsSubclassOf(typeof(Module)) == false)
        {
          LogError("Invalid type", it);
          continue;
        }

        GetAttribute(it, "description", out description, null);
        Nodes[id] = new ModuleNode {
          ModuleId = id,
          ModuleType = typeObject,
          Description = description
        };

        wireupLoadQueue.Enqueue(it.Current.Clone());
      }

      while (wireupLoadQueue.Count > 0)
      {
        nav = wireupLoadQueue.Dequeue();
        id = nav.GetAttribute("id", string.Empty);
        LoadWireupsFromXml(nav.SelectChildren("Wireup", string.Empty), Nodes[id]);
      }
    }

    private void LoadWireupsFromXml(XPathNodeIterator it, ModuleNode node)
    {
      string param, value, srcId, srcParam;
      object valueObject = null;
      Dictionary<string, Module.Parameter> validParameters = null;
      TypeConverter converter;
      if (Module.UsesDynamicParameters(node.ModuleType) == false)
      {
        validParameters = Module.GetModuleParameters(node.ModuleType);
      }

      while (it.MoveNext())
      {
        if (GetAttribute(it, "name", out param, "Missing parameter name") == false)
        {
          continue;
        }

        if (validParameters != null && validParameters.ContainsKey(param) == false)
        {
          LogError("Invalid parameter", it);
          continue;
        }

        if (GetAttribute(it, "value", out value, null))
        {
          if (validParameters != null) // implies param is a key
          {
            converter = TypeDescriptor.GetConverter(validParameters[param].Type);
            if (converter.CanConvertFrom(typeof(string)))
            {
              valueObject = converter.ConvertFromString(value);
            }
            else
            {
              LogError("Warning: cannot convert value to parameter type (a string will be passed in)", it);
            }
          }
          // TODO: support a type attribute so the user can specify a type for
          // inputs to modules using dynamic parameters

          node[param] = valueObject ?? value;
        }
        else if (GetAttribute(it, "srcId", out srcId, null)
          && GetAttribute(it, "srcParam", out srcParam, null))
        {
          if (Nodes.ContainsKey(srcId) == false)
          {
            LogError("Invalid id", it);
            continue;
          }

          if (Module.UsesDynamicParameters(Nodes[srcId].ModuleType) == false
            && Module.GetModuleParameters(Nodes[srcId].ModuleType).ContainsKey(srcParam) == false)
          {
            LogError("Invalid srcParam", it);
            continue;
          }

          node[param] = new ParameterWireup {
            srcId = srcId,
            srcParam = srcParam
          };
        }
        else
        {
          LogError("Either value or srcId and srcParam attributes missing", it);
        }
      }
    }

    private bool GetAttribute(XPathNodeIterator it, string name, out string value, string errorMsg)
    {
      value = it.Current.GetAttribute(name, string.Empty);
      if (string.IsNullOrEmpty(value))
      {
        if (errorMsg != null)
        {
          LogError(errorMsg, it);
        }

        return false;
      }

      return true;
    }

    private void LogError(string errorMsg, XPathNodeIterator it)
    {
      LogError(errorMsg, it.Current as IXmlLineInfo);
    }

    private void LogError(string errorMsg, IXmlLineInfo lineInfo)
    {
      if (lineInfo != null)
      {
        errorMsg = string.Format(
          "Line {0} char {1}: {2}",
          lineInfo.LineNumber,
          lineInfo.LinePosition,
          errorMsg);
      }

      System.Console.Error.WriteLine(errorMsg);
    }

    #endregion
  
    public void Add(string moduleId, Type moduleType, string description = "")
    {
      Nodes.Add(moduleId, new ModuleNode {
        ModuleType = moduleType,
        Description = description
      }); // Add will throw an exception in the case of a duplicate key (desired)

      ModuleAdded(this, new ModuleEventArgs { ModuleId = moduleId });
    }

    public void Remove(string moduleId)
    {
      if (Nodes.Remove(moduleId))
      {
        ModuleRemoved(this, new ModuleEventArgs { ModuleId = moduleId });
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

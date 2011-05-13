using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace ThereBeMonsters.Back_end
{
  public struct ModuleNode
  {
    public Type moduleType;
    public string description;
  }

  public struct ParameterWireup
  {
    public string parameterName;
    public string srcIdDotParam;
  }

  public struct ValueWireup
  {
    public string parameterName;
    public object value;
  }

  public class ModuleGraph
  {
    public Dictionary<string, ModuleNode> moduleNodes { get; private set; }
    public Dictionary<string, List<ParameterWireup>> parameterWireups { get; private set; }
    public Dictionary<string, List<ValueWireup>> valueWireups { get; private set; }

    public ModuleGraph()
    {
      moduleNodes = new Dictionary<string, ModuleNode>();
      parameterWireups = new Dictionary<string, List<ParameterWireup>>();
      valueWireups = new Dictionary<string, List<ValueWireup>>();
    }

    public ModuleGraph(string filePath)
      : this()
    {
      LoadFromXml(filePath);
    }

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

        if (id.Contains("."))
        {
          LogError("id is not allowd to contain '.'", it);
          continue;
        }

        if (moduleNodes.ContainsKey(id))
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
        moduleNodes[id] = new ModuleNode {
          moduleType = typeObject,
          description = description
        };

        wireupLoadQueue.Enqueue(it.Current.Clone());
      }

      while (wireupLoadQueue.Count > 0)
      {
        nav = wireupLoadQueue.Dequeue();
        id = nav.GetAttribute("id", string.Empty);
        LoadWireupsFromXml(nav.SelectChildren("Wireup", string.Empty), id);
      }
    }

    private void LoadWireupsFromXml(XPathNodeIterator it, string id)
    {
      string param, value, srcId, srcParam;
      object valueObject = null;
      Dictionary<string, Module.Parameter> validParameters = null;
      TypeConverter converter;
      if (Module.UsesDynamicParameters(moduleNodes[id].moduleType) == false)
      {
        validParameters = Module.GetModuleParameters(moduleNodes[id].moduleType);
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
          if (valueWireups.ContainsKey(id) == false)
          {
            valueWireups[id] = new List<ValueWireup>();
          }

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

          valueWireups[id].Add(new ValueWireup {
            parameterName = param,
            value = valueObject ?? value
          });
        }
        else if (GetAttribute(it, "srcId", out srcId, null)
          && GetAttribute(it, "srcParam", out srcParam, null))
        {
          if (parameterWireups.ContainsKey(id) == false)
          {
            parameterWireups[id] = new List<ParameterWireup>();
          }

          if (moduleNodes.ContainsKey(srcId) == false)
          {
            LogError("Invalid id", it);
            continue;
          }

          if (Module.UsesDynamicParameters(moduleNodes[srcId].moduleType) == false
            && Module.GetModuleParameters(moduleNodes[srcId].moduleType).ContainsKey(srcParam) == false)
          {
            LogError("Invalid srcParam", it);
            continue;
          }

          parameterWireups[id].Add(new ParameterWireup {
            parameterName = param,
            srcIdDotParam = string.Format("{0}.{1}", srcId, srcParam)
          });
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
  }
}

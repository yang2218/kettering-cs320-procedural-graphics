using System;
using System.Collections.Generic;

namespace ThereBeMonsters.Back_end
{
  public class Generator
  {
    public ModuleGraph Graph { get; private set; }
    private Dictionary<Type, Module> moduleInstanceCache;

    #region Constructors

    public Generator()
    {
      moduleInstanceCache = new Dictionary<Type, Module>();
    }

    public Generator(string filePath)
      : this()
    {
      this.Graph = ModuleGraph.LoadFromXml(filePath);
    }

    public Generator(ModuleGraph graph)
      : this()
    {
      this.Graph = graph;
    }

    #endregion

    public void RunGraph()
    {
      Queue<ModuleNode> runQueue;
      Dictionary<ParameterWireup, int> paramReferences;
      Dictionary<ParameterWireup, object> outputCache = new Dictionary<ParameterWireup, object>();
      
      AnalyzeGraph(out runQueue, out paramReferences);

      Module module;
      ParameterWireup pw;
      object value;
      foreach (ModuleNode node in runQueue)
      {
        module = GetModuleInstance(node.ModuleType);

        foreach (Module.Parameter param in module.Parameters.Values)
        {
          if (node.Wireups.TryGetValue(param.Name, out value))
          {
            if (value != null && value.GetType() == typeof(ParameterWireup))
            {
              pw = (ParameterWireup)value;
              module[param.Name] = outputCache[pw];
              paramReferences[pw]--;
              if (paramReferences[pw] == 0)
              { // free the output result as soon as we know we don't need it anymore.
                outputCache.Remove(pw);
                paramReferences.Remove(pw);
              }
            }
            else
            {
              module[param.Name] = value;
            }

            continue;
          }

          if (param.Optional == false)
          {
            module[param.Name] = param.Default;
          }
        }

        module.Run();

        foreach (Module.Parameter param in module.Parameters.Values)
        {
          if ((param.Direction & Module.Parameter.IODirection.OUTPUT) == 0)
          {
            continue;
          }

          pw = new ParameterWireup(node.ModuleId, param.Name);
          if (paramReferences.ContainsKey(pw))
          { // only read the parameter if we have a need for it in the future
            outputCache[pw] = module[param.Name];
          }
        }
      }
    }

    private Module GetModuleInstance(Type type)
    {
      if (moduleInstanceCache.ContainsKey(type))
      {
        return moduleInstanceCache[type];
      }

      Module module = (Module)Activator.CreateInstance(type);
      if (module.IsReusable)
      {
        moduleInstanceCache[type] = module;
      }

      return module;
    }

    private void AnalyzeGraph(out Queue<ModuleNode> runQueue,
      out Dictionary<ParameterWireup, int> paramReferences)
    {
      runQueue = new Queue<ModuleNode>();
      paramReferences = new Dictionary<ParameterWireup, int>();

      // first stage: construct a DAG of dependancies (adjacency list)
      int numNodes = this.Graph.Nodes.Count;
      Dictionary<ModuleNode, ModuleNode[]> dependancies
        = new Dictionary<ModuleNode, ModuleNode[]>(numNodes);

      HashSet<ModuleNode> tempSet = new HashSet<ModuleNode>();
      ModuleNode[] tempArray;
      foreach (ModuleNode node in this.Graph.Nodes.Values)
      {
        foreach (ParameterWireup pw in node.ParameterWireups)
        {
          if (paramReferences.ContainsKey(pw) == false)
          {
            paramReferences[pw] = 0;
          }

          paramReferences[pw]++;
          tempSet.Add(this.Graph.Nodes[pw.srcId]);
        }

        if (tempSet.Count > 0)
        {
          tempArray = new ModuleNode[tempSet.Count];
          tempSet.CopyTo(tempArray);
          dependancies[node] = tempArray;
          tempSet.Clear();
        }
      }

      // stage two: transverse the graph DFS to build the queue
      // TODO: add a check to make sure there are no cycles
      HashSet<ModuleNode> visited = new HashSet<ModuleNode>();
      foreach (ModuleNode node in dependancies.Keys)
      {
        if (visited.Contains(node) == false)
        {
          DFSVisit(node, visited, dependancies, runQueue);
        }
      }
    }

    private void DFSVisit(ModuleNode node, HashSet<ModuleNode> visited,
      Dictionary<ModuleNode, ModuleNode[]> dependancies, Queue<ModuleNode> runQueue)
    {
      visited.Add(node);
      ModuleNode[] dependeeList;
      if (dependancies.TryGetValue(node, out dependeeList))
      {
        foreach (ModuleNode dependee in dependeeList)
        {
          if (visited.Contains(dependee) == false)
          {
            DFSVisit(dependee, visited, dependancies, runQueue);
          }
        }
      }

      runQueue.Enqueue(node);
    }

  }
}

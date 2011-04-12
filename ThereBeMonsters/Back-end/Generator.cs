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
      this.Graph = new ModuleGraph(filePath);
    }

    public Generator(ModuleGraph graph)
      : this()
    {
      this.Graph = graph;
    }

    #endregion

    public void RunGraph()
    {
      Queue<string> runQueue;
      Dictionary<string, int> paramReferences;
      Dictionary<string, object> outputCache = new Dictionary<string,object>();
      
      AnalyzeGraph(out runQueue, out paramReferences);

      Module module;
      string idDotParam;
      foreach (string id in runQueue)
      {
        module = GetModuleInstance(this.Graph.moduleNodes[id].moduleType);

        foreach (ValueWireup vw in this.Graph.valueWireups[id])
        {
          module[vw.parameterName] = vw.value;
        }

        foreach (ParameterWireup pw in this.Graph.parameterWireups[id])
        {
          module[pw.parameterName] = outputCache[pw.srcIdDotParam];
          paramReferences[pw.srcIdDotParam]--;
          if (paramReferences[pw.srcIdDotParam] == 0)
          { // free the output result as soon as we know we don't need it anymore.
            outputCache.Remove(pw.srcIdDotParam);
            paramReferences.Remove(pw.srcIdDotParam);
          }
        }

        module.Run();

        foreach (Module.Parameter param in module.Parameters.Values)
        {
          if ((param.Direction & Module.Parameter.IODirection.OUTPUT) == 0)
          {
            continue;
          }

          idDotParam = string.Format("{0}.{1}", id, param.Name);
          if (paramReferences.ContainsKey(idDotParam))
          { // only read the parameter if we have a need for it in the future
            outputCache[idDotParam] = module[param.Name];
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

    private void AnalyzeGraph(out Queue<string> runQueue, out Dictionary<string, int> paramReferences)
    {
      runQueue = new Queue<string>();
      paramReferences = new Dictionary<string, int>();

      // first stage: construct a DAG of dependancies (adjacency list)
      int numNodes = this.Graph.moduleNodes.Count;
      Dictionary<string, string[]> dependancies = new Dictionary<string, string[]>(numNodes);

      SortedSet<string> tempSet = new SortedSet<string>();
      string[] tempArray;
      foreach (string id in this.Graph.parameterWireups.Keys)
      {
        foreach (ParameterWireup pw in this.Graph.parameterWireups[id])
        {
          if (paramReferences.ContainsKey(pw.srcIdDotParam) == false)
          {
            paramReferences[pw.srcIdDotParam] = 0;
          }

          paramReferences[pw.srcIdDotParam]++;
          tempSet.Add(pw.srcIdDotParam.Split('.')[0]); // just get the ID part
        }

        if (tempSet.Count > 0)
        {
          tempArray = new string[tempSet.Count];
          tempSet.CopyTo(tempArray);
          dependancies[id] = tempArray;
          tempSet.Clear();
        }
      }

      // stage two: transverse the graph DFS to build the queue
      // TODO: add a check to make sure there are no cycles
      HashSet<string> visited = new HashSet<string>();
      foreach (string id in dependancies.Keys)
      {
        if (visited.Contains(id) == false)
        {
          DFSVisit(id, visited, dependancies, runQueue);
        }
      }
    }

    private void DFSVisit(string id, HashSet<string> visited, Dictionary<string, string[]> dependancies, Queue<string> runQueue)
    {
      visited.Add(id);
      foreach (string dependee in dependancies[id])
      {
        if (visited.Contains(dependee) == false)
        {
          DFSVisit(dependee, visited, dependancies, runQueue);
        }
      }

      runQueue.Enqueue(id);
    }

  }
}

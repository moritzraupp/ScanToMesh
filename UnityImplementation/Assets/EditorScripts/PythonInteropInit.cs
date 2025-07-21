using UnityEngine;
using UnityEditor;
using System;


public class PythonInteropInit
{

    private static bool init = false;
    public static bool Initialized() {return init;}

    [InitializeOnLoadMethod] 
    public static void OnEngineStart()
    {

        PythonInit();

        Application.quitting += OnEngineShutdown;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    }
    

    public static void PythonInit()
    {
        string queryInfo;
        if (stm.PythonInterop.Initialize(
            "Assets/ScanToMesh/python_runtime",
            "Assets/ScanToMesh/PythonScripts",
            out queryInfo) != 0)
        {
            throw new System.Exception(queryInfo);
        }
        Debug.Log(queryInfo);

        init = true;
    }

    public static void OnEngineShutdown()
    {
        init = false;

        Debug.Log("Shutting down Python-Environment");
        stm.PythonInterop.Shutdown();
    }

    public static void OnBeforeAssemblyReload()
    {
        init = false;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

    }   
    
}

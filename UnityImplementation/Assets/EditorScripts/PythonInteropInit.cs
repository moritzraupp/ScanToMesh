using UnityEngine;
using stm;
using UnityEditor;
using Python.Runtime;
using System;


public class PythonInteropInit
{
    [InitializeOnLoadMethod]
    public static void OnEngineStart()
    {
        Debug.Log("Loading ScanToMesh Python-Environment");

        string queryInfo;
        if (stm.PythonInterop.Initialize(
            "Assets/ScanToMesh/python_runtime", 
            "Assets/ScanToMesh/PythonScripts", 
            out queryInfo) != 0)
        {
            throw new System.Exception(queryInfo);
        }
        Debug.Log(queryInfo);

        WarmUpPython();

        Application.quitting += OnEngineShutdown;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    }
    public static void OnEngineShutdown()
    {
        Debug.Log("Shutting down Python-Environment");
        stm.PythonInterop.Shutdown();
    }

    private static void WarmUpPython()
    {
        using (Py.GIL())
        {
            PythonEngine.RunSimpleString(@"
import sys
import os
import glob
import numpy as np
import Rendering
import ImageIO
from FileStack import FileStack
print('Python warm-up complete.')
");
        }
    }

    public static void OnBeforeAssemblyReload() 
    {

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

    }
}

using UnityEngine;
using stm;
using UnityEditor;
using Python.Runtime;
using System;
using System.IO;
using System.Text;

public class UnityConsoleRedirector
{
    private static TextWriter originalOut;
    private static TextWriter originalErr;

    private static UnityTextWriter unityWriter = new UnityTextWriter();
    public static bool isRedirected = false;

    public static void EnableRedirect()
    {
        if (isRedirected)
            return;

        Debug.Log("Enable Console Output");

        originalOut = Console.Out;
        originalErr = Console.Error;

        Console.SetOut(unityWriter);
        Console.SetError(unityWriter);

        isRedirected = true;
    }

    public static void DisableRedirect()
    {
        if (!isRedirected)
            return;

        Debug.Log("Disable Console Output");

        Console.SetOut(originalOut);
        Console.SetError(originalErr);

        isRedirected = false;
    }

    private class UnityTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string value)
        {
            Debug.Log(value);
        }

        public override void Write(string value)
        {
            Debug.Log(value);
        }

    }

    [MenuItem("Tools/ScanToMesh/Show Full Log")]
    public static void ToggleFullLog()
    {
        if (isRedirected)
            DisableRedirect();
        else
            EnableRedirect();

        EditorApplication.delayCall += () => Menu.SetChecked("Tools/ScanToMesh/Show Full Log", isRedirected);
    }
    [MenuItem("Tools/ScanToMesh/Show Full Log", true)]
    public static bool ToggleOptionValidate()
    {
        Menu.SetChecked("Tools/ScanToMesh/Show Full Log", isRedirected);
        return true;
    }
}

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

    [MenuItem("Tools/ScanToMesh/Disable Plugin Import")]
    public static void DisablePluginImport()
    {
        string folderPath = "Assets/ScanToMesh/python_runtime";
        string[] dllFiles = Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories);

        string disabledInfo = "Disabeling DLLs:\n";
        foreach (string dllFile in dllFiles)
        {
            string assetPath = dllFile.Replace(Application.dataPath, "Assets").Replace('\\', '/');
            PluginImporter pluginImporter = AssetImporter.GetAtPath(assetPath) as PluginImporter;

            if (pluginImporter != null)
            {
                pluginImporter.SetCompatibleWithAnyPlatform(false);
                pluginImporter.SetCompatibleWithEditor(false);
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                pluginImporter.SaveAndReimport();
                disabledInfo += assetPath + "\n";
            }
        }

        Debug.Log(disabledInfo);
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

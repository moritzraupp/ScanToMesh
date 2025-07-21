using System.IO;
using UnityEditor;
using UnityEngine;

public class DisablePluginImport
{
    [MenuItem("Tools/ScanToMesh/Disable Plugin Import")]
    public static void DisablePluginImportF()
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
}

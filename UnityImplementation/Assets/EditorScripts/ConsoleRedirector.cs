using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

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
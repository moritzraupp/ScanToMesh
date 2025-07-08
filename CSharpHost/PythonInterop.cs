using System;
using System.IO;
using System.Linq;
using Python.Runtime;

namespace stm
{
    public static class PythonInterop
    {
        private static bool _initialized = false;

        public static int Initialize(string pythonHome_Dir, string pythonScripts_Dir)
        {
            return PythonInterop.Initialize(pythonHome_Dir, pythonScripts_Dir, out _);
        }
        public static int Initialize(string pythonHome_Dir, string pythonScripts_Dir, out string queryInfo)
        {

            queryInfo = "";

            if (_initialized)
            {
                queryInfo = "Already initialized";
                return 1;
            }

            // Validate directories
            if (!Directory.Exists(pythonHome_Dir))
            {
                throw new DirectoryNotFoundException($"pythonHome_Dir not found: {pythonHome_Dir}");
            }
            if (!Directory.Exists(pythonScripts_Dir))
            {
                throw new DirectoryNotFoundException($"pythonScripts_Dir not found: {pythonScripts_Dir}");
            }


            // Prepare Python paths
            string[] scriptPaths = Directory.GetDirectories(pythonScripts_Dir, "*", SearchOption.AllDirectories);
            string[] allPythonPaths = new string[]
            {
                Path.Combine(pythonHome_Dir, "Lib"),
                Path.Combine(pythonHome_Dir, "Lib", "site-packages"),
                pythonScripts_Dir
            }.Concat(scriptPaths).ToArray();

            string pythonPath = string.Join(Path.PathSeparator.ToString(), allPythonPaths);
            string pythonDLLPath = Path.Combine(pythonHome_Dir, "python313.dll");

            if (!File.Exists(pythonDLLPath))
                throw new FileNotFoundException($"Python DLL not foind: {pythonDLLPath}");


            // Set environment variables
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonHome_Dir);
            Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath);


            // Configure Python runtime
            Runtime.PythonDLL = pythonDLLPath;
            PythonEngine.PythonHome = pythonHome_Dir;
            PythonEngine.Initialize();

            _initialized = true;

            try
            {
                using (Py.GIL())
                {
                    using (PyObject sys = Py.Import("sys"))
                    using (PyObject sysPath = sys.GetAttr("path"))
                    {
                        // Update sys.path
                        foreach (var dir in allPythonPaths)
                        {
                            using (PyObject pyDir = new PyString(dir))
                            {
                                sysPath.InvokeMethod("append", pyDir);
                            }
                        }

                        // Build query info
                        using (PyObject version = sys.GetAttr("version"))
                        using (PyObject executable = sys.GetAttr("executable"))
                        {
                            queryInfo += $"Python initialized with:\n";
                            queryInfo += $"PYTHONHOME = {pythonHome_Dir}\n";
                            queryInfo += $"PYTHONPATH = {pythonPath}\n";
                            queryInfo += $"Python Version: {version.ToString()}\n";
                            queryInfo += $"Python Executable: {executable.ToString()}\n";
                            queryInfo += $"Python Path:\n";

                            int pathCount = (int)sysPath.Length();
                            for (int i = 0; i < pathCount; i++)
                            {
                                using (PyObject pathItem = sysPath[i])
                                {
                                    queryInfo += $"  {pathItem.ToString()}\n";
                                }
                            }
                        }
                    }
                }
            }
            catch (PythonException pex)
            {
                queryInfo += pex.Message;
                return 2;
            }
            catch (Exception ex)
            {
                queryInfo += ex.Message;
                return 2;
            }

            return 0;
        }

        public static int Shutdown()
        {
            if (_initialized)
            {
                try
                {
                    PythonEngine.Shutdown();
                }
                catch (NotSupportedException ex)
                {
                    // Expected in Unity: BinaryFormatter usage not supported.
                    Console.WriteLine($"PythonEngine.Shutdown() skipped due to: {ex.Message}");
                }
                _initialized = false;
                return 0;
            }
            return 1;
        }

        public static bool IsInitialized => _initialized;
    }
}

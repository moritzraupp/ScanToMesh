Return to [main page](../README.md)

# Standalone C#

STM can be used in a standalone C# / .Net environment

Python.Net is used for bridging between C# and Python allowing the execution of Python code in C# (see [Tools Used](../README.md#tools-used))

## Setup

1. Load and build `CSharpHost/CSharpHost.csproj`

2. Copy the output to `STM_out` via `copy_stm_to_out.bat`

3. Copy `ScanToMesh` from `STM_out` to your project

4. Reference the DLLs from `ScanToMesh/CSHarpHost/` in your project

5. Start Programming in C#

## Example Code


```csharp
using stm; // use stm namespace

// Always initialize the PythonEngine in the beginning
stm.PythonInterop.Initialize("D:/Thesis/ScanToMesh/python_runtime", "D:/Thesis/ScanToMesh/PythonScripts", out string queryOutput);
Console.WriteLine(queryOutput);

...

// Shutdown in the end
stm.PythonInterop.Shutdown();
```

```csharp
// Using Pipeline Wrapper of C#

Pipeline pipeline = new Pipeline();

pipeline.SetFolderPath("D:\\Thesis\\data\\PP_20150928_15758 PM\\PP_w0");
pipeline.Read();

pipeline.processors.Add(new ImageProcessor());
pipeline.processors[0].SetPythonPath("D:/Thesis/ScanToMesh/PythonScripts/ImageProcessors/Threshold.py");
pipeline.Process();

pipeline.Render();
```

```csharp
// Use Raw Python Code
using Python.Runtime;

using (Py.GIL()) // always use the Python Global Interpreter Lock (GIL) when doing Python stuff
{
    // make sure to prevent memory leaks:

    string result

    // Option 1: use "using"
    using (PyObject module = Py.Import("ImportIO"))
    {
        using (PyObject function = module.GetAttr("extract_image_descriptions"))
        {
            result = function.Invoke(new PyString("demo_file.tif")).ToString();
        }
    }


    // Option 2: manual disposement
    PyObject module = Py.Import("ImportIO");
    PyObject function = module.GetAttr("extract_image_descriptions");

    result = function.Invoke(new PyString("demo_file.tif")).ToString();

    function.Dispose();
    module.Dispose();
}
```

##
Return to [top](#standalone-c)


Return to [main page](../README.md)
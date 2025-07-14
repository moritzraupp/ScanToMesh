using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Python.Runtime;

namespace stm
{
    public class MarchingCubesGenerator : PythonModuleObject
    {
        public float isoVal = 255;
        public MarchingCubesGenerator()
        {
            string moduleName = "Mesh";
            string className = "MeshGen";
            string functionName = "marching_cubes";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _class = _module.GetAttr(className);
                _instance = _class.Invoke();
                _function = _instance.GetAttr(functionName);
            }
        }

        public PyObject Generate(PyObject image)
        {
            using (Py.GIL())
            {
                using(PyObject iso = new PyFloat(isoVal))
                {
                    PyObject result = _function.Invoke(image, iso);
                    return result;
                }
            }
        }
    }


    public class STLWriter : PythonModuleObject
    {
        public string folderPath = null;
        public string fileName = null;

        public STLWriter()
        {
            string moduleName = "Mesh";
            string functionName = "write_stl";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public void Write(PyObject mesh)
        {
            if (mesh == null) { Console.WriteLine("Mesh is null"); return; }
            if (folderPath == null) { Console.WriteLine("No folderPath was set"); return; }
            if (fileName == null) { Console.WriteLine("No fileName was set"); return; }

            string fileNameWithExtension = fileName;

            if (!fileName.EndsWith(".stl", StringComparison.OrdinalIgnoreCase))
            {
                fileNameWithExtension += ".stl";
            }

            string fullPath = System.IO.Path.Combine(folderPath, fileNameWithExtension).ToString();

            Console.WriteLine($"Output file: {fullPath}");
            string parentDir = System.IO.Directory.GetParent(fullPath).ToString();
            if (!System.IO.File.Exists(parentDir)) 
            {
                Directory.CreateDirectory(parentDir);
            }
            
            using (Py.GIL())
            {
                _function.Invoke(new PyString(fullPath), mesh);
            }
        }
    }

    public class OBJWriter : PythonModuleObject
    {
        public string folderPath = null;
        public string fileName = null;

        public OBJWriter()
        {
            string moduleName = "Mesh";
            string functionName = "write_obj";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public void Write(PyObject mesh)
        {
            if (mesh == null) { Console.WriteLine("Mesh is null"); return; }
            if (folderPath == null) { Console.WriteLine("No folderPath was set"); return; }
            if (fileName == null) { Console.WriteLine("No fileName was set"); return; }

            string fileNameWithExtension = fileName;

            if (!fileName.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
            {
                fileNameWithExtension += ".obj";
            }

            string fullPath = System.IO.Path.Combine(folderPath, fileNameWithExtension).ToString();

            Console.WriteLine($"Output file: {fullPath}");
            string parentDir = System.IO.Directory.GetParent(fullPath).ToString();
            if (!System.IO.File.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            using (Py.GIL())
            {
                _function.Invoke(new PyString(fullPath), mesh);
            }
        }
    }

    public class MeshInfo : PythonModuleObject
    {
        public MeshInfo()
        {
            string moduleName = "Mesh";
            string functionName = "get_mesh_info";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public string Get(PyObject mesh)
        {
            if (mesh == null) return "null";

            using (Py.GIL())
            {
                return _function.Invoke(mesh).ToString();
            }
        }
    }
}

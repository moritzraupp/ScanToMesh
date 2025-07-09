using Python.Runtime;
using stm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace stm
{
    [Serializable]
    public class ImageProcessor : PythonModuleObject
    {
        [Serializable]
        public struct Parameter
        {
            public string name;
            public string value;
        }

        public string name;
        public List<Parameter> parameters = new List<Parameter>();

        private string pythonPath;
        private bool _isValid = false;

        public bool IsValid() { return _isValid; }

        public string GetPythonPath() { return pythonPath; }

        public void SetPythonPath(string pythonPath)
        {
            ImageProcessorReflection.ClassInfo classInfo;

            try
            {
                classInfo = ImageProcessorReflection.GetProcessorMetadata(pythonPath);
            }
            catch (PythonException pex)
            {
                Console.WriteLine(pex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            name = classInfo.ClassName;
            parameters.Clear();
            this.pythonPath = pythonPath;
            foreach (var param in classInfo.Params)
            {
                parameters.Add(new Parameter
                {
                    name = param.Name,
                    value = param.DefaultValue ?? "",
                });
            }

            // init py objects for later use
            DisposeClass();
            DisposeModule();
            using (Py.GIL())
            {
                string moduleName = System.IO.Path.GetFileNameWithoutExtension(this.pythonPath);

                _module = Py.Import(moduleName);

                if (!_module.HasAttr(name))
                    throw new Exception($"Module does not contain class {name}");

                _class = _module.GetAttr(name);

                _isValid = true;
            }
        }

        public PyObject Process(PyObject image)
        {
            if (string.IsNullOrEmpty(pythonPath))
                throw new InvalidOperationException("Python path is not set.");

            var args = new List<PyObject>();
            foreach (var p in parameters)
            {
                PyObject val;

                if (int.TryParse(p.value, out int i))
                    val = new PyInt(i);
                else if (float.TryParse(p.value, out float f))
                    val = new PyFloat(f);
                else
                    val = new PyString(p.value);

                args.Add(val);
            }

            PyObject processed = null;

            // Create instance
            using (PyObject instance = _class.Invoke(args.ToArray()))
            {
                // instance.set_image(image)
                instance.InvokeMethod("set_image", image);

                // image = instance.process()
                processed = instance.InvokeMethod("process");    
            }

            // Clean up parameter PyObjects
            foreach (var a in args) a.Dispose();

            if (processed == null)
            {
                Console.WriteLine("processed is null");
            }

            return processed;

        }
    }
}

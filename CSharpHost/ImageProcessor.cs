using Python.Runtime;
using stm;
using System;
using System.Collections.Generic;
using System.Text;

namespace stm
{
    public class ImageProcessor
    {
        public struct Parameter
        {
            public string name;
            public string value;
        }

        public string name;
        public List<Parameter> parameters = new List<Parameter>();

        private string pythonPath;
        private bool isValid = false;

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
        }

        public void Process(ref PyObject image)
        {
            if (string.IsNullOrEmpty(pythonPath))
                throw new InvalidOperationException("Python path is not set.");

            using (Py.GIL())
            {
                string moduleName = System.IO.Path.GetFileNameWithoutExtension(pythonPath);

                // Load module
                using (PyObject module = Py.Import(moduleName))
                {

                    // Get class from module
                    if (!module.HasAttr(name))
                        throw new Exception($"Module does not contain class {name}");

                    using (PyObject cls = module.GetAttr(name))
                    {
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

                        // Create instance
                        using (PyObject instance = cls.Invoke(args.ToArray()))
                        {
                            // instance.set_image(image)
                            instance.InvokeMethod("set_image", image);

                            // image = instance.process()
                            PyObject processed = instance.InvokeMethod("process");
                            
                            image.Dispose(); // Dispose old image if needed
                            image = processed;
                            
                        }

                        // Clean up parameter PyObjects
                        foreach (var a in args)
                            a.Dispose();
                    }
                }
            }
        }
    }
}

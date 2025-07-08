using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace stm
{
    public class ImageProcessorReflection
    {
        public struct ParamInfo
        {
            public string Name;
            public string DefaultValue;  // Always string, can cast later
            public string TypeHint;
        }

        public struct ClassInfo
        {
            public string ClassName;
            public List<ParamInfo> Params;
        }

        public static ClassInfo GetProcessorMetadata(string scriptPath)
        {
            using (Py.GIL())
            {
                // Import python python module
                string moduleName = "MetaReader";
                string functionName = "get_class_params_from_file";
                using (PyObject module = Py.Import(moduleName))
                {
                    using (PyObject func = module.GetAttr(functionName))
                    {
                        // Call function
                        using (PyObject pyResult = func.Invoke(new PyTuple(new PyObject[] { new PyString(scriptPath) })))
                        {
                            // Extract class_name
                            string className = pyResult.GetItem("class_name").ToString();

                            // Extract params list
                            List<ParamInfo> parameters = new List<ParamInfo>();
                            using (PyObject pyParams = pyResult.GetItem("params"))
                            {
                                var count = pyParams.Length();
                                for (int i = 0; i < count; i++)
                                {
                                    using (PyObject pyParam = pyParams[i])
                                    {
                                        string paramName = pyParam.GetItem("name").ToString();

                                        string defaultValue = null;
                                        using (PyObject defObj = pyParam.GetItem("default"))
                                        {
                                            if (!defObj.IsNone())
                                            {
                                                defaultValue = defObj.ToString();
                                            }
                                        }

                                        string typeHint = null;
                                        using (PyObject typeObj = pyParam.GetItem("type"))
                                        {
                                            if (!typeObj.IsNone())
                                            {
                                                typeHint = typeObj.ToString();
                                            }
                                        }

                                        parameters.Add(new ParamInfo
                                        {
                                            Name = paramName,
                                            DefaultValue = defaultValue,
                                            TypeHint = typeHint
                                        });
                                    }
                                }
                            }
                            return new ClassInfo
                            {
                                ClassName = className,
                                Params = parameters
                            };
                        }
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Python.Runtime;

namespace stm
{
    [Serializable]
    public class VolumeRenderer : PythonModuleObject
    {
        public VolumeRenderer() 
        {
            string moduleName = "Rendering";
            string functionName = "render_volume";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public void Render(PyObject image)
        {
            if (image == null)
            {
                Console.WriteLine("Render was called without an image!");
                return;
            }

            using(Py.GIL())
            {
                _function.Invoke(image);
            }
        }
    }
}

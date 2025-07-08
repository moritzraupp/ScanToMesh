using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace stm
{

    public class ImageReader
    {
        public ImageReader(string filePath = null)
        {
            this.filePath = filePath;
        }
        public string filePath;
        public PyObject Read()
        {
            if (filePath == null)
            {
                Console.WriteLine("ImageReader was called without a specifying a file");
                return null;
            }

            if (!System.IO.File.Exists(filePath)) 
            {
                Console.WriteLine($"File does not exist: {filePath}");
                return null;
            }

            using (Py.GIL() )
            {
                string moduleName = "ImageIO";
                string functionName = "read_image";
                using (PyObject module = Py.Import(moduleName))
                {
                    using (PyObject func = module.GetAttr(functionName)) 
                    {
                        PyObject pyResult = func.Invoke(new PyTuple(new PyObject[] {new PyString(filePath)}));

                        return pyResult;
                    }
                }
            }
        }
    }

    public class ImageWriter
    {
        public string filePath;
        public void Write(ref PyObject image) 
        {
            if (filePath == null)
            {
                Console.WriteLine("ImageReader was called without a specifying a file");
                return;
            }

            using (Py.GIL())
            {
                string moduleName = "ImageIO";
                string functionName = "write_image";
                using (PyObject module = Py.Import(moduleName))
                {
                    using (PyObject func = module.GetAttr(functionName))
                    {
                        PyObject pyResult = func.Invoke(new PyTuple(new PyObject[] { new PyString(filePath), image}));
                    }
                }
            }
        }
    }

    public class ImageSeriesReader
    {

    }
}

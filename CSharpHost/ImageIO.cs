using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace stm
{
    
    public class ImageCopy : PythonModuleObject
    {
        public ImageCopy()
        {
            string moduleName = "ImageIO";
            string functionName = "copy_image";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public PyObject Copy(PyObject image)
        {
            if (image == null) return null;

            using (Py.GIL())
            {
                return _function.Invoke(image);
            }
        }
    }

    public class ImageInfo : PythonModuleObject
    {
        public ImageInfo() 
        {
            string moduleName = "ImageIO";
            string functionName = "get_image_info";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public string Get(PyObject image)
        {
            if (image == null) return "null";
            
            using (Py.GIL())
            {
                return _function.Invoke(image).ToString();
            }
        }
    }

    public class ImageReader : PythonModuleObject
    {
        public ImageReader(string filePath)
        {
            string moduleName = "ImageIO";
            string functionName = "read_image";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
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
                PyObject pyResult = _function.Invoke(new PyTuple(new PyObject[] {new PyString(filePath)}));

                return pyResult;
            }
        }
    }

    public class ImageSeriesReader : PythonModuleObject
    {

        public FileStack fileStack = new FileStack();
        private string _folderPath;
        public int startIndex = 0;
        public int endIndex = 0;

        public ImageSeriesReader()
        {
            string moduleName = "ImageIO";
            string functionName = "read_image_stack";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }

        }

        public string GetFolderPath() { return _folderPath; }

        public void SetFolderPath(string folderPath)
        {
            if (folderPath == null)
            {
                Console.WriteLine("ImageSeriesReader was called without a specifying a folder");
                return;
            }

            if (!System.IO.Directory.Exists(folderPath))
            {
                Console.WriteLine($"Folder does not exist: {folderPath}");
                return;
            }

            _folderPath = folderPath;
            fileStack.SetDirectory(folderPath);

            int numberOfFiles = fileStack.Count();

            endIndex = numberOfFiles - 1;

            if (startIndex >= numberOfFiles) startIndex = numberOfFiles-1;
            

        }

        public PyObject Read()
        {
            if (_folderPath == null)
            {
                Console.WriteLine("ImageReader was called without a specifying a folder");
                return null;
            }

            if (!fileStack.IsInitialized())
            {
                Console.WriteLine("ImageReader was called without a valid FileStack");
                return null;
            }

            if (endIndex <= startIndex)
            {
                Console.WriteLine($"No valid range from {startIndex} to {endIndex}");
                return null;
            }

            using (Py.GIL())
            {
                using(PyObject start = new PyInt(startIndex))
                using(PyObject end = new PyInt(endIndex))
                {
                    PyObject result = _function.Invoke(fileStack.GetInstance(), start, end);
                    return result;
                }
            }

            return null;
        }
    }

    public class ImageSeriesWriter : PythonModuleObject
    {
        public string folderPath = null;
        public string fileName = null;
        public ImageSeriesWriter ()
        {
            string moduleName = "ImageIO";
            string functionName = "write_image_stack_with_metadata";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public void Write(PyObject image, bool metaFile = true)
        {
            if (image == null) { Console.WriteLine("Image is null"); return; }
            if (folderPath == null) { Console.WriteLine("No folderPath was set"); return; }
            if (fileName == null) { Console.WriteLine("No fileName was set"); return; }

            string combinedPath = System.IO.Path.Combine(folderPath, fileName);

            using (Py.GIL())
            {
                _function.Invoke(new PyString(combinedPath), new PyString(fileName), image, new PyInt(metaFile ? 1 : 0));
            }
        }

    }

    public class ImageMetadata : PythonModuleObject
    {
        public ImageMetadata()
        {
            string moduleName = "ImageIO";
            string functionName = "get_metadata";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _function = _module.GetAttr(functionName);
            }
        }

        public string Get(PyObject image)
        {
            if (image == null) return "null";

            using (Py.GIL())
            {
                return _function.Invoke(image).ToString();
            }
        }
    }
}

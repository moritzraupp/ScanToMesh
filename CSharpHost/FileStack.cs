using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace stm
{
    [Serializable]
    public class FileStack : PythonModuleObject
    {

        private bool _hasInstance = false;

        public string _directory = null;
        public List<string> _extensions = new List<string>();
        public bool _sortFiles = false;



        public void SetDirectory (string directory)
        {
            if (directory == null) return;

            _directory = directory;

            Init();
        }

        public bool IsInitialized() { return _hasInstance; }

        public int Count()
        {
            if (!_hasInstance) return 0;
            using (Py.GIL())
            {
                using (PyObject result = _instance.InvokeMethod("__len__"))
                {
                    return result.As<int>();
                }
            }
        }

        public void Init()
        {
            if (_directory == null)
            {
                Console.WriteLine("FileStack was called with no directory!");
                return;
            }

            if (!System.IO.Directory.Exists(_directory))
            {
                Console.WriteLine($"Directory {_directory} does not exist!");
                return;
            }

            using (Py.GIL())
            {
                var args = new List<PyObject>();

                PyString dir = new PyString(_directory);
                args.Add(dir);

                PyList extensions = new PyList();
                List<PyObject> extensionStrings = new List<PyObject>();

                foreach (var ex in _extensions)
                {
                    var pyStr = new PyString(ex);
                    extensionStrings.Add(pyStr);    // Track manually created PyStrings
                    extensions.Append(pyStr);
                }

                args.Add(extensions);

                PyInt sort = new PyInt(_sortFiles ? 1 : 0);
                args.Add(sort);

                DisposeInstance();

                _instance = _class.Invoke(args.ToArray());
                _hasInstance = true;

                // Clean up
                foreach (var pyStr in extensionStrings) pyStr.Dispose();
                extensions.Dispose();  // Dispose the list object itself
                foreach (var arg in args) arg.Dispose();
            }
        }

        public PyObject GetInstance()
        {
            if (_hasInstance)
            {
                return _instance;
            }

            return null;
        }

        public FileStack() 
        {
            string moduleName = "FileStack";
            string className = "FileStack";

            using (Py.GIL())
            {
                _module = Py.Import(moduleName);
                _class = _module.GetAttr(className);
            }
        }
    }
}

using Python.Runtime;
using System;

namespace stm
{
    public abstract class PythonModuleObject : IDisposable
    {
        protected PyObject _module = null;
        protected PyObject _class = null;
        protected PyObject _instance = null;
        protected PyObject _function = null;

        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Only dispose if Python is still initialized
            if (PythonEngine.IsInitialized)
            {
                DisposeFunction();
                DisposeInstance();
                DisposeClass();
                DisposeModule();
            }
            else
            {
                // Optionally null out to aid GC
                _function = null;
                _instance = null;
                _class = null;
                _module = null;
            }

            GC.SuppressFinalize(this);
        }

        ~PythonModuleObject() 
        {
            Dispose();
        }

        public void DisposeModule()
        {
            if (_module != null)
            {
                _module.Dispose();
                _module = null;
            }
        }
        public void DisposeClass()
        {
            if ( _class != null)
            {
                _class.Dispose();
                _class = null;
            }
        }
        public void DisposeInstance()
        {
            if (_instance != null)
            { 
                _instance.Dispose();
                _instance = null;
            }
        }
        public void DisposeFunction()
        {
            if (_function != null)
            {
                _function.Dispose();
                _function = null;
            }
        }
    }
}

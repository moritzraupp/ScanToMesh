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

        ~PythonModuleObject()
        {
            Dispose(false); // Don't trust Python in finalizer
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (disposing)
            {
                // Dispose called manually or by using() block
                if (PythonEngine.IsInitialized)
                {
                    SafeDispose(DisposeFunction);
                    SafeDispose(DisposeInstance);
                    SafeDispose(DisposeClass);
                    SafeDispose(DisposeModule);
                }
                else
                {
                    NullOut();
                }
            }
            else
            {
                // Called from finalizer - never touch Python!
                NullOut();
            }
        }

        private void SafeDispose(Action disposeAction)
        {
            try
            {
                disposeAction?.Invoke();
            }
            catch (Exception ex)
            {
                // Replace with UnityEngine.Debug.LogWarning if using Unity
                Console.WriteLine($"[Warning] Error disposing Python object: {ex}");
            }
        }

        private void NullOut()
        {
            _function = null;
            _instance = null;
            _class = null;
            _module = null;
        }

        public void DisposeModule()
        {
            _module?.Dispose();
            _module = null;
        }

        public void DisposeClass()
        {
            _class?.Dispose();
            _class = null;
        }

        public void DisposeInstance()
        {
            _instance?.Dispose();
            _instance = null;
        }

        public void DisposeFunction()
        {
            _function?.Dispose();
            _function = null;
        }
    }
}
using System;

namespace UnityTools
{
    public abstract class Scope : Disposable
    {

    }
    public class Disposable : IDisposable
    {
        // Flag: Has Dispose already been called?
        private bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();

            disposed = true;
        }

        protected virtual void DisposeManaged() { }
        protected virtual void DisposeUnmanaged() { }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}
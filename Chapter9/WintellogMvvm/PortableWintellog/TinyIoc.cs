using System;
using System.Collections.Generic;
using System.Threading;

namespace PortableWintellog
{
    public class TinyIoc : ITinyIoc
    {
        private readonly object _mutex = new object();
        private readonly Dictionary<string, Func<ITinyIoc, object>> _directives = 
            new Dictionary<string, Func<ITinyIoc, object>>();         
        private readonly Dictionary<string, object> _objects = 
            new Dictionary<string, object>(); 

        public TinyIoc()
        {
            Register<ITinyIoc>(tinyIoc => this);
        }

        public void Register<T>(Func<ITinyIoc, T> creation)
        {
            var type = typeof (T).FullName;

            if (_directives.ContainsKey(type))
            {
                return;
            }

            Monitor.Enter(_mutex);

            try
            {
                if (_directives.ContainsKey(type))
                {
                    return;
                }      
          
                _directives.Add(type, tinyIoc => creation(tinyIoc));
            }
            finally
            {
                Monitor.Exit(_mutex);
            }
        }

        public T Resolve<T>()
        {
            return Resolve<T>(false);
        }

        public T Resolve<T>(bool createNew)
        {
            var type = typeof (T).FullName;
            
            if (!_directives.ContainsKey(type))
            {
                throw new Exception(
                    string.Format(
                    "No way to resolve type {0}",
                    type));
            }
            
            if (createNew)
            {
                return (T)_directives[type](this);
            }

            if (_objects.ContainsKey(type))
            {
                return (T) _objects[type];
            }

            Monitor.Enter(_mutex);

            try
            {
                if (_objects.ContainsKey(type))
                {
                    return (T)_objects[type];
                }                

                var instance = _directives[type](this);

                _objects.Add(type, instance);

                return (T)instance;
            }
            finally
            {
                Monitor.Exit(_mutex);
            }
        }
    }
}
using System;

namespace PortableWintellog
{
    public interface ITinyIoc
    {
        void Register<T>(Func<ITinyIoc, T> creation);
        T Resolve<T>();
        T Resolve<T>(bool createNew);
    }
}
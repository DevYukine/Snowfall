using System;

namespace Snowfall.Abstraction;

public interface IStaticDisposable
{
    static void Dispose() => throw new NotImplementedException();
}

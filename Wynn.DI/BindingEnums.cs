﻿namespace Wynn.DI
{
    internal enum BindingResolution
    {
        None = 0,
        AsCached = 1,
        AsTransient = 2,
    }

    internal enum BindingScope
    {
        None = 0,
        OnResolve = 1,
        OnInstall = 2,
    }
}

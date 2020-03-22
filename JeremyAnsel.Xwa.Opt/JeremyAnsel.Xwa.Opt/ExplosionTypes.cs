// -----------------------------------------------------------------------
// <copyright file="ExplosionType.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;

    [Flags]
    public enum ExplosionTypes
    {
        None,
        
        Type1 = 1,
        
        Type2 = 2,
        
        Type3 = 4
    }
}

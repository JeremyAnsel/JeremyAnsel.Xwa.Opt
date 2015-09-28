// -----------------------------------------------------------------------
// <copyright file="ExplosionType.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Flags]
    public enum ExplosionTypes
    {
        None,
        
        Type1 = 1,
        
        Type2 = 2,
        
        Type3 = 4
    }
}

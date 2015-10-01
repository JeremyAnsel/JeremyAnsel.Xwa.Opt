// -----------------------------------------------------------------------
// <copyright file="HardPoint.cs" company="">
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

    public class Hardpoint
    {
        public Hardpoint()
        {
            this.HardpointType = HardpointType.None;
            this.Position = Vector.Empty;
        }

        public HardpointType HardpointType { get; set; }

        public Vector Position { get; set; }
    }
}

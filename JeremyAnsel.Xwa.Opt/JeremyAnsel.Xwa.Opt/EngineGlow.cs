// -----------------------------------------------------------------------
// <copyright file="EngineGlow.cs" company="">
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

    public class EngineGlow
    {
        public EngineGlow()
        {
            this.IsDisabled = false;
            this.CoreColor = 0xFFFFFFFF;
            this.OuterColor = 0xFFFFFFFF;
            this.Format = Vector.Empty;
            this.Position = Vector.Empty;
            this.Look = new Vector(0, 1, 0);
            this.Up = new Vector(0, 0, 1);
            this.Right = new Vector(1, 0, 0);
        }

        public bool IsDisabled { get; set; }

        public uint CoreColor { get; set; }

        public uint OuterColor { get; set; }

        public Vector Format { get; set; }

        public Vector Position { get; set; }

        public Vector Look { get; set; }

        public Vector Up { get; set; }

        public Vector Right { get; set; }
    }
}

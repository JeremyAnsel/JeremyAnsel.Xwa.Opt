// -----------------------------------------------------------------------
// <copyright file="RotationScale.cs" company="">
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

    public class RotationScale
    {
        public RotationScale()
        {
            this.Pivot = Vector.Empty;
            this.Look = new Vector(0, 32767, 0);
            this.Up = new Vector(0, 0, 32767);
            this.Right = new Vector(32767, 0, 0);
        }

        public Vector Pivot { get; set; }

        public Vector Look { get; set; }

        public Vector Up { get; set; }

        public Vector Right { get; set; }

        public RotationScale Clone()
        {
            var rotationScale = new RotationScale
            {
                Pivot = this.Pivot,
                Look = this.Look,
                Up = this.Up,
                Right = this.Right
            };

            return rotationScale;
        }
    }
}

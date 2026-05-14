// -----------------------------------------------------------------------
// <copyright file="RotationScale.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    public class RotationScale
    {
        public RotationScale()
        {
            this.Pivot = Vector.Empty;
            this.RotationAxis = new Vector(0, 32767, 0);
            this.DirectionAxis = new Vector(0, 0, 32767);
            this.UpAxis = new Vector(32767, 0, 0);
        }

        public Vector Pivot { get; set; }

        public Vector RotationAxis { get; set; }

        public Vector DirectionAxis { get; set; }

        public Vector UpAxis { get; set; }

        public RotationScale Clone()
        {
            var rotationScale = new RotationScale
            {
                Pivot = this.Pivot,
                RotationAxis = this.RotationAxis,
                DirectionAxis = this.DirectionAxis,
                UpAxis = this.UpAxis
            };

            return rotationScale;
        }
    }
}

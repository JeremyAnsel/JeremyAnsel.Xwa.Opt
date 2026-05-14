// -----------------------------------------------------------------------
// <copyright file="EngineGlow.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    public class EngineGlow
    {
        public EngineGlow()
        {
            this.IsDisabled = false;
            this.CoreColor = 0xFFFFFFFF;
            this.OuterColor = 0xFFFFFFFF;
            this.Dimensions = Vector.Empty;
            this.Position = Vector.Empty;
            this.LookAxis = new Vector(0, 1, 0);
            this.UpAxis = new Vector(0, 0, 1);
            this.RightAxis = new Vector(1, 0, 0);
        }

        public bool IsDisabled { get; set; }

        public uint CoreColor { get; set; }

        public uint OuterColor { get; set; }

        public Vector Dimensions { get; set; }

        public Vector Position { get; set; }

        public Vector LookAxis { get; set; }

        public Vector UpAxis { get; set; }

        public Vector RightAxis { get; set; }

        public EngineGlow Clone()
        {
            var engineGlow = new EngineGlow
            {
                IsDisabled = this.IsDisabled,
                CoreColor = this.CoreColor,
                OuterColor = this.OuterColor,
                Dimensions = this.Dimensions,
                Position = this.Position,
                LookAxis = this.LookAxis,
                UpAxis = this.UpAxis,
                RightAxis = this.RightAxis
            };

            return engineGlow;
        }
    }
}

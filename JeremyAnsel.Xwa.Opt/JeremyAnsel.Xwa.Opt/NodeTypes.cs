// -----------------------------------------------------------------------
// <copyright file="NodeType.cs" company="">
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

    public enum NodeTypes
    {
        NullNode = -1,
        
        NodeGroup = 0,
        
        FaceData = 1,
        
        MeshVertices = 3,
        
        NodeReference = 7,
        
        VertexNormals = 11,
        
        TextureCoordinates = 13,
        
        Texture = 20,
        
        FaceGrouping = 21,
        
        Hardpoint = 22,
        
        RotationScale = 23,
        
        NodeSwitch = 24,
        
        MeshDescriptor = 25,
        
        TextureAlpha = 26,
        
        EngineGlow = 28,
    }
}

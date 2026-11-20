using System;
namespace UniformGridPathfinder.Navigation
{
    public readonly struct CellId : IEquatable<CellId>
    {
        public readonly string Surface; public readonly int X,Z;
        public CellId(string surface,int x,int z){Surface=surface;X=x;Z=z;}
        public bool Equals(CellId other)=>Surface==other.Surface&&X==other.X&&Z==other.Z;
        public override bool Equals(object other)=>other is CellId id&&Equals(id);
        public override int GetHashCode()=>((Surface?.GetHashCode()??0)*397^X)*397^Z;
        public override string ToString()=>Surface+":"+X+":"+Z;
    }
}

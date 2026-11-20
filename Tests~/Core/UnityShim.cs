using System;
namespace UnityEngine
{
 public class MonoBehaviour { public Transform transform=new Transform(); public T GetComponentInParent<T>() where T:class=>null; }
 public class Transform { public Vector3 position; }
 public class LineRenderer {}
 public class PropertyAttribute:Attribute {}
 public class SerializeField:Attribute {}
 public class MinAttribute:Attribute {public MinAttribute(float value){}}
 public static class Physics {public static bool Raycast(Vector3 a,Vector3 b,float c,int mask)=>false;}
 public struct Bounds {public Vector3 center,size;public Bounds(Vector3 p,Vector3 s){center=p;size=s;}public Vector3 min=>center-size*.5f;public Vector3 max=>center+size*.5f;}
 public struct Vector3:IEquatable<Vector3>
 {
  public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}
  public static Vector3 up=>new Vector3(0,1,0); public static Vector3 down=>new Vector3(0,-1,0);
  public static Vector3 operator +(Vector3 a,Vector3 b)=>new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
  public static Vector3 operator -(Vector3 a,Vector3 b)=>new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
  public static Vector3 operator *(Vector3 a,float n)=>new Vector3(a.x*n,a.y*n,a.z*n);
  public static bool operator ==(Vector3 a,Vector3 b)=>a.Equals(b);public static bool operator !=(Vector3 a,Vector3 b)=>!a.Equals(b);
  public bool Equals(Vector3 b)=>x==b.x&&y==b.y&&z==b.z;public override bool Equals(object b)=>b is Vector3 v&&Equals(v);
  public override int GetHashCode()=>x.GetHashCode()^y.GetHashCode()*397^z.GetHashCode()*7919;
  public override string ToString()=>"("+x+","+y+","+z+")";
 }
 public struct Vector3Int {public int x,y,z;public Vector3Int(int x,int y,int z){this.x=x;this.y=y;this.z=z;}}
 public static class Mathf
 {
  public static float Sqrt(float n)=>(float)Math.Sqrt(n);public static float Abs(float n)=>Math.Abs(n);
  public static int FloorToInt(float n)=>(int)Math.Floor(n);public static float Pow(float n,float p)=>(float)Math.Pow(n,p);
  public static float Floor(float n)=>(float)Math.Floor(n);public static float Log(float n,float b)=>(float)Math.Log(n,b);
 }
}
namespace UnityEngine.InputSystem {public class PlayerInput {}}

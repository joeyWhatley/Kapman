// A STAND-IN FOR THE FEW UnityEngine TYPES THE CREW'S CODE USES, so draft/CrewBody.cs, RowingRig.cs and ShipCrew.cs can
// be RUN outside Unity (Unity was busy and may not be started: see LOOK_AND_MOTION.md). Not for the project: Unity never
// sees this folder. The maths is Unity's (the same quaternion product, the same AngleAxis and FromToRotation).
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 right => new Vector3(1, 0, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public float sqrMagnitude => x * x + y * y + z * z;
        public float magnitude => MathF.Sqrt(sqrMagnitude);
        public Vector3 normalized { get { float m = magnitude; return m > 1e-9f ? this / m : zero; } }
        public void Normalize() { this = normalized; }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator *(Vector3 a, float s) => new Vector3(a.x * s, a.y * s, a.z * s);
        public static Vector3 operator *(float s, Vector3 a) => new Vector3(a.x * s, a.y * s, a.z * s);
        public static Vector3 operator /(Vector3 a, float s) => new Vector3(a.x / s, a.y / s, a.z / s);
        public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) { t = Mathf.Clamp01(t); return a + (b - a) * t; }
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public override string ToString() => $"({x:0.000}, {y:0.000}, {z:0.000})";
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity => new Quaternion(0, 0, 0, 1);
        public static Quaternion AngleAxis(float degrees, Vector3 axis)
        {
            axis = axis.normalized;
            float h = degrees * Mathf.Deg2Rad * 0.5f, s = MathF.Sin(h);
            return new Quaternion(axis.x * s, axis.y * s, axis.z * s, MathF.Cos(h));
        }
        public static Quaternion Inverse(Quaternion q) => new Quaternion(-q.x, -q.y, -q.z, q.w);
        public static Quaternion operator *(Quaternion a, Quaternion b) => new Quaternion(
            a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
            a.w * b.y + a.y * b.w + a.z * b.x - a.x * b.z,
            a.w * b.z + a.z * b.w + a.x * b.y - a.y * b.x,
            a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z);
        public static Vector3 operator *(Quaternion q, Vector3 v)
        {
            var u = new Vector3(q.x, q.y, q.z);
            var t = 2f * Vector3.Cross(u, v);
            return v + q.w * t + Vector3.Cross(u, t);
        }
        public static Quaternion FromToRotation(Vector3 from, Vector3 to)
        {
            Vector3 a = from.normalized, b = to.normalized;
            float d = Vector3.Dot(a, b);
            if (d > 0.999999f)
                return identity;
            if (d < -0.999999f)
            {
                Vector3 n = Vector3.Cross(Vector3.right, a);
                if (n.sqrMagnitude < 1e-6f)
                    n = Vector3.Cross(Vector3.up, a);
                return AngleAxis(180f, n);
            }
            Vector3 c = Vector3.Cross(a, b);
            var q = new Quaternion(c.x, c.y, c.z, 1f + d);
            float m = MathF.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion(q.x / m, q.y / m, q.z / m, q.w / m);
        }
        public static Quaternion Euler(float xDeg, float yDeg, float zDeg) =>
            AngleAxis(yDeg, Vector3.up) * AngleAxis(xDeg, Vector3.right) * AngleAxis(zDeg, Vector3.forward);
    }

    public static class Mathf
    {
        public const float PI = MathF.PI, Deg2Rad = MathF.PI / 180f, Rad2Deg = 180f / MathF.PI;
        public static float Sin(float a) => MathF.Sin(a);
        public static float Cos(float a) => MathF.Cos(a);
        public static float Asin(float a) => MathF.Asin(a);
        public static float Acos(float a) => MathF.Acos(a);
        public static float Atan2(float y, float x) => MathF.Atan2(y, x);
        public static float Sqrt(float a) => MathF.Sqrt(a);
        public static float Abs(float a) => MathF.Abs(a);
        public static float Floor(float a) => MathF.Floor(a);
        public static float Min(float a, float b) => a < b ? a : b;
        public static float Max(float a, float b) => a > b ? a : b;
        public static float Clamp(float v, float a, float b) => v < a ? a : v > b ? b : v;
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float InverseLerp(float a, float b, float v) => a != b ? Clamp01((v - a) / (b - a)) : 0f;
        public static float SmoothStep(float a, float b, float t) { t = Clamp01(t); t = -2f * t * t * t + 3f * t * t; return b * t + a * (1f - t); }
        public static float Repeat(float t, float len) => Clamp(t - Floor(t / len) * len, 0f, len);
        public static float MoveTowards(float a, float b, float d) => Abs(b - a) <= d ? b : a + MathF.Sign(b - a) * d;
        public static float DeltaAngle(float a, float b) { float d = Repeat(b - a, 360f); return d > 180f ? d - 360f : d; }
        // (not Unity's own noise, only as smooth and in the same 0 .. 1 range: the crew uses it for small differences)
        public static float PerlinNoise(float x, float y)
        {
            float xi = Floor(x), yi = Floor(y), fx = x - xi, fy = y - yi;
            float u = fx * fx * (3 - 2 * fx), v = fy * fy * (3 - 2 * fy);
            float H(float a, float b) { float s = Sin(a * 127.1f + b * 311.7f) * 43758.5453f; return s - Floor(s); }
            float a0 = H(xi, yi), a1 = H(xi + 1, yi), b0 = H(xi, yi + 1), b1 = H(xi + 1, yi + 1);
            return (a0 + (a1 - a0) * u) + ((b0 + (b1 - b0) * u) - (a0 + (a1 - a0) * u)) * v;
        }
    }

    public class Object { public string name = ""; public static implicit operator bool(Object o) => o != null; }

    public class Component : Object
    {
        public Transform transform;
        public GameObject gameObject => transform.owner;
        public T GetComponent<T>() where T : class => null;
    }

    public class Renderer : Component { }
    public class Behaviour : Component { public bool enabled = true; }
    public class MonoBehaviour : Behaviour { }

    public class GameObject : Object
    {
        public Transform transform;
        public bool activeSelf = true;
        public void SetActive(bool on) { activeSelf = on; }
        public GameObject(string name = "")
        {
            this.name = name;
            transform = new Transform { name = name, owner = this };
            transform.transform = transform;
        }
        public T AddComponent<T>() where T : Component, new() => new T { transform = transform, name = name };
    }

    public class Transform : Component
    {
        internal GameObject owner;
        Transform up_;
        readonly List<Transform> kids = new List<Transform>();
        public Vector3 localPosition = Vector3.zero, localScale = Vector3.one;
        public Quaternion localRotation = Quaternion.identity;

        public Transform parent { get => up_; set => SetParent(value, false); }
        public void SetParent(Transform p, bool worldPositionStays)
        {
            up_?.kids.Remove(this);
            up_ = p;
            p?.kids.Add(this);
        }
        public int childCount => kids.Count;
        public Transform GetChild(int i) => kids[i];
        public Vector3 lossyScale => up_ != null ? Vector3.Scale(up_.lossyScale, localScale) : localScale;
        public Quaternion rotation
        {
            get => up_ != null ? up_.rotation * localRotation : localRotation;
            set => localRotation = up_ != null ? Quaternion.Inverse(up_.rotation) * value : value;
        }
        public Vector3 position
        {
            get => up_ != null ? up_.TransformPoint(localPosition) : localPosition;
            set => localPosition = up_ != null ? up_.InverseTransformPoint(value) : value;
        }
        public void SetPositionAndRotation(Vector3 p, Quaternion r) { rotation = r; position = p; }
        public Vector3 TransformPoint(Vector3 p) => position + rotation * Vector3.Scale(lossyScale, p);
        public Vector3 InverseTransformPoint(Vector3 w)
        {
            Vector3 v = Quaternion.Inverse(rotation) * (w - position), s = lossyScale;
            return new Vector3(v.x / s.x, v.y / s.y, v.z / s.z);
        }
        public Vector3 TransformDirection(Vector3 d) => rotation * d;
        public Vector3 right => rotation * Vector3.right;
        public Vector3 up => rotation * Vector3.up;
        public Vector3 forward => rotation * Vector3.forward;
    }

    public static class Time { public static float deltaTime = 1f / 60f, time; }

    [AttributeUsage(AttributeTargets.All)] public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.All)] public class HeaderAttribute : Attribute { public HeaderAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.All)] public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    [AttributeUsage(AttributeTargets.All)] public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int o) { } }
}

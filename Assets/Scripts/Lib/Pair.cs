// Helper class, stores a pair of integers in a single object.
using System;

public class Pair : IEquatable<Pair> {
    public int w;
    public int h;
    public Pair(int _w, int _h) {
        w = _w;
        h = _h;
    }
    public override string ToString() {
        return "(" + w + ", " + h + ")";
    }
    public override int GetHashCode() {
        string s = w + ", " + h;
        return s.GetHashCode();
    }
    public override bool Equals(object obj) {
        Pair other = obj as Pair;
        if (other == null) {
            return false;
        }
        return Equals(other);
    }

    public bool Equals(Pair other) {
        return this.w == other.w && this.h == other.h;
    }
}

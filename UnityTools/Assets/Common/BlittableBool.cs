using System;

/// <summary>
/// This is a 4-bytes bool which used in gpu to comply blittable constrains.
/// </summary>
public struct BlittableBool : IEquatable<BlittableBool>
{
    private int value;

    public BlittableBool(bool value)
    {
        this.value = Convert.ToInt32(value);
    }

    public static implicit operator bool(BlittableBool blittableBool)
    {
        return blittableBool.value != 0;
    }

    public static implicit operator BlittableBool(bool value)
    {
        return new BlittableBool(value);
    }

    public bool Equals(BlittableBool other)
    {
        return value == other.value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is BlittableBool && Equals((BlittableBool)obj);
    }

    public override int GetHashCode()
    {
        return value;
    }

    public static bool operator ==(BlittableBool left, BlittableBool right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlittableBool left, BlittableBool right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return ((bool)this).ToString();
    }
}
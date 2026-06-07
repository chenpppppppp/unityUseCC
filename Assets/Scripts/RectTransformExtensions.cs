using UnityEngine;

public static class RectTransformExtensions
{
    public static RectTransform Let(this RectTransform rt, System.Action<RectTransform> action)
    {
        action(rt);
        return rt;
    }

    public static T Let<T>(this T obj, System.Action<T> action) where T : Component
    {
        action(obj);
        return obj;
    }
}

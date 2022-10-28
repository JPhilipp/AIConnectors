using UnityEngine;
using System;
using System.Collections.Generic;

public static class TransformExtensions
{
    public static void SetPositionX(this Transform transform, float value)
    {
        Vector3 vector = transform.position;
        vector.x = value;
        transform.position = vector;
    }
    
    public static void SetPositionY(this Transform transform, float value)
    {
        Vector3 vector = transform.position;
        vector.y = value;
        transform.position = vector;
    }
    
    public static void SetPositionZ(this Transform transform, float value)
    {
        Vector3 vector = transform.position;
        vector.z = value;
        transform.position = vector;
    }

    public static void SetLocalPositionX(this Transform transform, float value)
    {
        Vector3 vector = transform.localPosition;
        vector.x = value;
        transform.localPosition = vector;
    }
    
    public static void SetLocalPositionY(this Transform transform, float value)
    {
        Vector3 vector = transform.localPosition;
        vector.y = value;
        transform.localPosition = vector;
    }
    
    public static void SetLocalPositionZ(this Transform transform, float value)
    {
        Vector3 vector = transform.localPosition;
        vector.z = value;
        transform.localPosition = vector;
    }

    public static void SetLocalPositionXRelative(this Transform transform, float value)
    {
        SetLocalPositionX(transform, transform.localPosition.x + value);
    }
    
    public static void SetLocalPositionYRelative(this Transform transform, float value)
    {
        SetLocalPositionY(transform, transform.localPosition.y + value);
    }
    
    public static void SetLocalPositionZRelative(this Transform transform, float value)
    {
        SetLocalPositionZ(transform, transform.localPosition.z + value);
    }
    
    public static void SetLocalEulerAnglesX(this Transform transform, float value)
    {
        Vector3 vector = transform.localEulerAngles;
        vector.x = value;
        transform.localEulerAngles = vector;
    }
    
    public static void SetLocalEulerAnglesY(this Transform transform, float value)
    {
        Vector3 vector = transform.localEulerAngles;
        vector.y = value;
        transform.localEulerAngles = vector;
    }
    
    public static void SetLocalEulerAnglesZ(this Transform transform, float value)
    {
        Vector3 vector = transform.localEulerAngles;
        vector.z = value;
        transform.localEulerAngles = vector;
    }

    public static void SetLocalEulerAnglesXRelative(this Transform transform, float value)
    {
        SetLocalEulerAnglesX(transform, transform.localEulerAngles.x + value);
    }
    
    public static void SetLocalEulerAnglesYRelative(this Transform transform, float value)
    {
        SetLocalEulerAnglesY(transform, transform.localEulerAngles.y + value);
    }
    
    public static void SetLocalEulerAnglesZRelative(this Transform transform, float value)
    {
        SetLocalEulerAnglesZ(transform, transform.localEulerAngles.z + value);
    }

    public static void SetEulerAnglesX(this Transform transform, float value)
    {
        Vector3 vector = transform.eulerAngles;
        vector.x = value;
        transform.eulerAngles = vector;
    }
    
    public static void SetEulerAnglesY(this Transform transform, float value)
    {
        Vector3 vector = transform.eulerAngles;
        vector.y = value;
        transform.eulerAngles = vector;
    }
    
    public static void SetEulerAnglesZ(this Transform transform, float value)
    {
        Vector3 vector = transform.eulerAngles;
        vector.z = value;
        transform.eulerAngles = vector;
    }

    public static void SetLocalScaleX(this Transform transform, float value)
    {
        Vector3 vector = transform.localScale;
        vector.x = value;
        transform.localScale = vector;
    }
    
    public static void SetLocalScaleY(this Transform transform, float value)
    {
        Vector3 vector = transform.localScale;
        vector.y = value;
        transform.localScale = vector;
    }
    
    public static void SetLocalScaleZ(this Transform transform, float value)
    {
        Vector3 vector = transform.localScale;
        vector.z = value;
        transform.localScale = vector;
    }
}

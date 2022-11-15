using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public static class Misc
{
    // Collects static-only methods of unspecific, generic need.

    const string objectNameIfAlreadyDestroyed = "(already_destroyed)";
    private static System.Random rng = new System.Random();

    public static bool DestroyComponent<T>(MonoBehaviour monoBehaviour) where T : Component
    {
        return DestroyComponent<T>(monoBehaviour.gameObject);
    }

    public static bool DestroyComponent<T>(GameObject gameObject) where T : Component
    {
        bool didDestroy = false;
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            GameObject.Destroy(component);
            didDestroy = true;
        }
        return didDestroy;
    }

    public static T GetAddComponent<T>(Transform transform) where T : Component
    {
        return GetAddComponent<T>(transform.gameObject);
    }

    public static T GetAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    public static GameObject InstantiateRenamed(GameObject prefab, string newName)
    {
        GameObject thisObject = UnityEngine.Object.Instantiate(prefab);
        thisObject.name = newName;
        return thisObject;
    }

    public static void PrintVector3(Vector3 vector3)
    {
        Debug.Log(vector3.ToString("F2"));
    }

    public static string Remove(string text, params string[] finds)
    {
        foreach (string find in finds)
        {
            text = Misc.Remove(text, find);
        }
        return text;
    }

    public static string Remove(string text, string find)
    {
        text = Misc.RepeatedReplace(text, find, "");
        return text;
    }

    public static string RepeatedReplace(string text, string[] finds, string replace)
    {
        foreach (string find in finds)
        {
            text = Misc.RepeatedReplace(text, find, replace);
        }
        return text;
    }

    public static string RepeatedReplace(string text, string find, string replace)
    {
        string oldText = null;
        while (text != oldText)
        {
            oldText = text;
            text = text.Replace(find, replace);
        }
        return text;
    }
    
    public static string GetTextBetween(string source, string start, string end)
    {
        string pattern = string.Format(
            "{0}({1}){2}",
            Regex.Escape(start),
            ".+?",
            Regex.Escape(end));

        foreach (Match m in Regex.Matches(source, pattern))
        {
            return m.Groups[1].Value;
        }
        return null;
    }

    public static List<string> GetTextsBetween(string source, string start, string end)
    {
        var results = new List<string>();
        string pattern = string.Format(
            "{0}({1}){2}",
            Regex.Escape(start),
            ".+?",
            Regex.Escape(end));

        foreach (Match m in Regex.Matches(source, pattern))
        {
            results.Add(m.Groups[1].Value);
        }
        return results;
    }
    
    public static string RemoveFromStart(this string s, string prefix, bool trimStart = false)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (trimStart) { s = s.TrimStart(); }

            if (s.StartsWith(prefix))
            {
                s = s.Substring(prefix.Length);
            }
        }
        return s;
    }

    public static string RemoveFromEnd(this string s, string suffix, bool trimEnd = false)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (trimEnd) { s = s.TrimEnd(); }

            if (s.EndsWith(suffix))
            {
                s = s.Substring(0, s.Length - suffix.Length);
            }
        }
        return s;
    }

    public static void OpenWindowsExplorerAtPath(string path)
    {
        path = path.Replace(@"/", @"\");
        System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
    }

    public static Vector3 ReduceVector3Digits(Vector3 v, int digits = 3)
    {
        return new Vector3(
            ReduceFloatDigits(v.x, digits),
            ReduceFloatDigits(v.y, digits),
            ReduceFloatDigits(v.z, digits)
        );
    }

    public static float ReduceFloatDigits(float value, int digits = 3)
    {
        return (float) Math.Round(value, digits);
    }

    public static void ResetTransform(Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    
    public static int LimitCircular(int value, int max)
    {
        return LimitCircular(value, 0, max);
    }

    public static int LimitCircular(int value, int min, int max)
    {
        if      (value < min) { value = max; }
        else if (value > max) { value = min; }
        return value;
    }

    public static float LimitCircular(float value, float min, float max)
    {
        if      (value < min) { value = max; }
        else if (value > max) { value = min; }
        return value;
    }

    public static float ClampMin(float value, float min)
    {
        if (value < min) { value = min; }
        return value;
    }

    public static float ClampMax(float value, float max)
    {
        if (value > max) { value = max; }
        return value;
    }

    public static int ClampMax(int value, int max)
    {
        if (value > max) { value = max; }
        return value;
    }
    
    public static void ShowInTree(UnityEngine.Object thisObject)
    {
        #if UNITY_EDITOR
            if (thisObject != null)
            {
                EditorGUIUtility.PingObject(thisObject);
            }
            else
            {
                Debug.LogWarning("Trying to ShowInTree null object");
            }
        #endif
    }

    public static Color GetGray(float lightness)
    {
        return new Color(lightness, lightness, lightness, 1f);
    }

    public static string Capitalize(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (s.Length == 1)
            {
                s = s.ToUpper();
            }
            else
            {
                s = s[0].ToString().ToUpper() + s.Substring(1);
            }
        }
        return s;
    }

    public static string LowerCaseFirstLetter(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (s.Length == 1)
            {
                s = s.ToLower();
            }
            else
            {
                s = s[0].ToString().ToLower() + s.Substring(1);
            }
        }
        return s;
    }
   
    public static GameObject[] GetRootObjects()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
    }
    
    public static float Fuzzy(float max)
    {
        return UnityEngine.Random.Range(-max, max);
    }

    public static Vector3 FuzzyVector3(Vector3 max)
    {
        return new Vector3(
            Fuzzy(max.x),
            Fuzzy(max.y),
            Fuzzy(max.z)
        );
    }

    public static Vector3 GetRandomVector3(float max)
    {
        return new Vector3
        (
            UnityEngine.Random.Range(-max, max),
            UnityEngine.Random.Range(-max, max),
            UnityEngine.Random.Range(-max, max)
        );
    }

    public static int GetRandomInt(int min, int max)
    {
        return UnityEngine.Random.Range(min, max + 1);
    }

	public static string Truncate(string text, int maxLength = 20, bool addDots = true) {
        const int dotsLength = 2;
        if ( !string.IsNullOrEmpty(text) && text.Length > maxLength ) {
            if (addDots) {
                text = text.Substring(0, maxLength - dotsLength) + "..";
            }
            else {
                text = text.Substring(0, maxLength);
            }
        }
        return text;
	}

    public static string Vector3String(Vector3 vector3)
    {
        return
            vector3.x.ToString() + ", " +
            vector3.y.ToString() + ", " +
            vector3.z.ToString()
        ;
    }
    
    public static string CamelCaseToTitle(string s)
    {
        return Regex.Replace(s, "([A-Z])", " $1").Trim();
    }

    public static string TextToCamelCaseCodeWord(string s)
    {
        string[] words = s.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = Capitalize(words[i]);
        }
        return string.Join("", words);
    }

    public static Vector3 AngleLerp(Vector3 startAngle, Vector3 endAngle, float t)
    {
        float xLerp = Mathf.LerpAngle(startAngle.x, endAngle.x, t);
        float yLerp = Mathf.LerpAngle(startAngle.y, endAngle.y, t);
        float zLerp = Mathf.LerpAngle(startAngle.z, endAngle.z, t);
        return new Vector3(xLerp, yLerp, zLerp);
    }

    public static void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }

    public static void ShuffleList<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static string[] Split(string text, string stringToUseForSplitting = " ", StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
    {
        if ( text != null && text != "" && text.IndexOf(stringToUseForSplitting) >= 0 )
        {
            string[] separators = new string[] {stringToUseForSplitting};
            return text.Split(separators, options);
        }
        else
        {
            return new string[1] {text};
        }
    }

    public static Transform GetRootChild(string name)
    {
        Transform transform = null;
        GameObject[] gameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.name == name)
            {
                transform = gameObject.transform;
                break;
            }
        }
        return transform;
    }

    public static Vector3 GetVector3(float value)
    {
        return new Vector3(value, value, value);
    }

    public static bool Chance(float chanceFrom0to1 = 0.5f)
    {
        return UnityEngine.Random.Range(0f, 1f) <= chanceFrom0to1;
    }

    public static void RemoveCloneFromName(GameObject gameObject)
    {
        gameObject.name = gameObject.name.Replace("(Clone)", "");
    }
    
    public static void RemoveCloneFromName(Transform transform)
    {
        transform.name = transform.name.Replace("(Clone)", "");
    }
    
    public static void Destroy(GameObject thisObject)
    {
        // Destroy() isn't immediate and DestroyImmediate() not suggested/ doesn't work in all contexts
        thisObject.tag = "Untagged";
        thisObject.name = objectNameIfAlreadyDestroyed;

        List<Component> components = new List<Component>(
            thisObject.GetComponents(typeof(Component)));
        foreach (Component component in components)
        {
            MonoBehaviour mb = component as MonoBehaviour;
            if (mb != null && (mb is MonoBehaviour) && !(mb is System.Object))
            {
                ((MonoBehaviour)mb).StopAllCoroutines();
                Component.Destroy(mb);
            }
        }

        GameObject.Destroy(thisObject);
    }
    
    public static bool IsDestroyed(GameObject thisObject)
    {
        return thisObject == null || thisObject.name == objectNameIfAlreadyDestroyed;
    }

    public static bool IsNullOrFalse(bool? state)
    {
        return state == null || !(bool)state;
    }

    public static float GetObjectSize(GameObject gameObject)
    {
        float maxSize = 0f;

        Collider[] colliders = gameObject.GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            if (collider.bounds.size.x > maxSize) { maxSize = collider.bounds.size.x; }
            if (collider.bounds.size.y > maxSize) { maxSize = collider.bounds.size.y; }
            if (collider.bounds.size.z > maxSize) { maxSize = collider.bounds.size.z; }
        }

        return maxSize;
    }

    public static string AddSpaceIfSet(string s)
    {
        return string.IsNullOrEmpty(s) ? "" : s + " ";
    }

    public static string EscapeJson(string s)
    {
        s = s.Replace("\"", "\\\"");
        s = s.Replace("\n", "\\n");
        s = s.Replace("\r", "\\r");
        return s;
    }

    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += obj => { tcs.SetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }

    public static string PadWithZero(float number, int digits = 2)
    {
        return PadWithZero((int) number, digits);
    }

    public static string PadWithZero(int number, int digits = 2)
    {
        return number.ToString().PadLeft(digits, '0');
    }
}

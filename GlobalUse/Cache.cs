using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;

public class Cache
{
    public static string rootFolder = @"D:\_misc\Cache";
    string subFolder = "Miscellaneous";
    string extension = "dat";

    static List<string> reservedKeys = new List<string>();

    public Cache(string subFolder = null, string extension = null)
    {
        if (subFolder != null)  { this.subFolder = subFolder; }
        if (extension != null)  { this.extension = ToValidExtension(extension); }
    }

    public void Reserve(string key)
    {
        if (!reservedKeys.Contains(key))
        {
            reservedKeys.Add(key);
        }
        else
        {
            Debug.LogWarning("Cache key already reserved: " + key);
        }
    }

    public bool IsReserved(string key)
    {
        return reservedKeys.Contains(key);
    }

    public void ReleaseReservation(string key)
    {
        reservedKeys.Remove(key);
    }

    public string GetText(string key)
    {
        string text = null;

        string path = GetPathByKey(key);
        if (File.Exists(path)) { text = File.ReadAllText(path); }

        return text;
    }

    public byte[] GetData(string key)
    {
        byte[] data = null;

        string path = GetPathByKey(key);
        if (File.Exists(path)) { data = File.ReadAllBytes(path); }

        return data;
    }

    public void SetText(string key, string content)
    {
        CreateMainFoldersIfNeeded();
        string path = GetPathByKey(key);
        CreatePathIfNeeded(path);
        File.WriteAllText(path, content);
    }

    public void SetData(string key, byte[] data, bool createKeyFoldersIfNeeded = false)
    {
        CreateMainFoldersIfNeeded();
        string path = GetPathByKey(key);
        CreatePathIfNeeded(path);
        File.WriteAllBytes(path, data);
    }

    void CreateMainFoldersIfNeeded()
    {
        CreatePathIfNeeded(rootFolder + "\\" + subFolder);
    }

    void CreatePathIfNeeded(string path)
    {
        path = path.Replace("/", "\\");
        string[] folders = path.Split('\\');
        string currentPath = "";
        for (int i = 0; i < folders.Length - 1; i++)
        {
            currentPath += folders[i] + "\\";
            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }
        }
    }

    public string GetPathByKey(string key)
    {
        return rootFolder + "\\" + subFolder + "\\" + key + "." + extension;
    }

    public static string ToKey(string s, bool allowSlash = false)
    {
        // s = s.ToLower();

        s = s.Replace(" ", "_");

        s = s.Replace("\r\n", "_");
        s = s.Replace("\r", "_");
        s = s.Replace("\n", "_");

        s = Regex.Replace(s, @"[^a-zA-Z0-9_" + (allowSlash ? "/" : "") + "]", "_");

        s = s.Replace("\\", "_");
        s = Misc.RepeatedReplace(s, "__", "_");

        s = s.Trim();

        s = Misc.RemoveFromStart(s, "_");
        s = Misc.RemoveFromEnd(s, "_");

        s = Misc.Truncate(s, 150, addDots: false);

        return s;
    }

    string ToValidExtension(string extensionToCheck)
    {
        string extension = "dat";
        if (!string.IsNullOrEmpty(extensionToCheck))
        {
            extensionToCheck = extensionToCheck.ToLower();
            Regex regex = new Regex("^[a-z]+$");
            Match match = regex.Match(extensionToCheck);
            if (match.Success)
            {
                extension = extensionToCheck;
            }
            else
            {
                Debug.LogWarning("Cache extension " + extensionToCheck +
                    " ignored, only a-z allowed.");
            }
        }
        return extension;
    }
}

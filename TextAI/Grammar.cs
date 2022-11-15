using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Grammar
{
    public static string GetAorAn(string s, bool capitalize = false)
    {
        if (!string.IsNullOrEmpty(s))
        {
            string firstLetter = s.Substring(0, 1);
            firstLetter = firstLetter.ToLower();

            string[] lettersForAn = {"a", "e", "i", "o"};
            string aOrAn = lettersForAn.Contains(firstLetter) ? "an" : "a";
            s = aOrAn + " " + s;

            if (capitalize) { s = Misc.Capitalize(s); }
        }
        return s;
    }

    public static string RemoveAorAnFromStart(string s)
    {
        const string stringA  = "a ";
        const string stringAn = "an ";
        
        if (s.StartsWith(stringA, ignoreCase: true, culture: null))
        {
            s = s.Substring(stringA.Length);
        }
        else if (s.StartsWith(stringAn, ignoreCase: true, culture: null))
        {
            s = s.Substring(stringAn.Length);
        }

        return s;
    }
}

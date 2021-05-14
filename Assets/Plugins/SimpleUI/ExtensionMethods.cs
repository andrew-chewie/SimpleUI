using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExtensionMethods
{

    /// <summary>
    /// Determines whether the collection is null or contains no elements.
    /// </summary>
    /// <typeparam name="T">The IEnumerable type.</typeparam>
    /// <param name="enumerable">The enumerable, which may be null or empty.</param>
    /// <returns>
    ///     <c>true</c> if the IEnumerable is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            return true;
        }

        /* If this is a list, use the Count property for efficiency. 
             * The Count property is O(1) while IEnumerable.Count() is O(N). */
        var collection = enumerable as ICollection<T>;
        if (collection != null)
        {
            return collection.Count < 1;
        }

        return !enumerable.Any();
    }

    public static GameObject Show(this GameObject gameObject)
    {
        if (gameObject == null)
            return null;

        gameObject.SetActive(true);
        return gameObject;
    }

    public static GameObject Hide(this GameObject gameObject)
    {
        if (gameObject == null)
            return null;

        gameObject.SetActive(false);
        return gameObject;
    }

    public static GameObject SetVisible(this GameObject gameObject,bool visible)
    {
        if (gameObject == null)
            return null;

        gameObject.SetActive(visible);
        return gameObject;
    }

    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        if (source.Count() == 1) {
            return source.ElementAt(0);
        }

        var enumerable = source.Where(arg => arg != null);
        return enumerable.PickRandom(1).Single();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }

    /// <summary>
    /// Converts RectTransform.rect's local coordinates to world space
    /// Usage example RectTransformExt.GetWorldRect(myRect, Vector2.one);
    /// </summary>
    /// <returns>The world rect.</returns>
    /// <param name="rt">RectangleTransform we want to convert to world coordinates.</param>
    /// <param name="scale">Optional scale pulled from the CanvasScaler. Default to using Vector2.one.</param>
    public static Rect GetWorldRect(this RectTransform rt)
    {
        // Convert the rectangle to world corners and grab the top left
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);

        var size = new Vector2(maxX - minX, maxY - minY);
        var pos = new Vector2(minX, minY);

        return new Rect(pos, size);
    }

    public static Vector2Int ToVector2Int(this Vector2 v)
    {
        return new Vector2Int((int) v.x, (int) v.y);
    }

    public static Bounds ToBounds(this Rect rect)
    {
        Bounds b = new Bounds(rect.center, rect.size);
        return b;
    }

    public static string ToRoman(this int number)
    {
        if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
        if (number < 1) return String.Empty;
        if (number >= 1000) return "M" + ToRoman(number - 1000);
        if (number >= 900) return "CM" + ToRoman(number - 900);
        if (number >= 500) return "D" + ToRoman(number - 500);
        if (number >= 400) return "CD" + ToRoman(number - 400);
        if (number >= 100) return "C" + ToRoman(number - 100);
        if (number >= 90) return "XC" + ToRoman(number - 90);
        if (number >= 50) return "L" + ToRoman(number - 50);
        if (number >= 40) return "XL" + ToRoman(number - 40);
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        throw new ArgumentOutOfRangeException("something bad happened");
    }

    public static bool ContainBounds(this Bounds bounds, Bounds target)
    {
        return bounds.Contains(target.min) && bounds.Contains(target.max);
    }

    public static bool IntersectsBounds(this Bounds bounds, Bounds target)
    {
        return bounds.Contains(target.min) || bounds.Contains(target.max);
    }

    public static void Show(this MonoBehaviour monoBehaviour)
    {
        monoBehaviour.gameObject.Show();
    }

    public static void Hide(this MonoBehaviour monoBehaviour)
    {
        monoBehaviour.gameObject.Hide();
    }

    public static Bounds GetBounds(this Camera camera)
    {
        var cameraCenter = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -camera.transform.position.z));
        var cameraMax = camera.ViewportToWorldPoint(new Vector3(1, 1, -camera.transform.position.z*1.5f));
        var cameraSize = (cameraMax - cameraCenter).Abs();
        return new Bounds(cameraCenter, new Vector3(cameraSize.x * 2f, cameraSize.y * 2f, 10000f));
    }

    public static Vector3 Abs(this Vector3 vec)
    {
        Vector3 result;
        result.x = Mathf.Abs(vec.x);
        result.y = Mathf.Abs(vec.y);
        result.z = Mathf.Abs(vec.z);
        return result;
    }

    public static T Dequeue<T>(this List<T> list) where T : class
    {
        var result = list[0];
        list.Remove(result);
        return result;
    }
        
    public static void Enqueue<T>(this List<T> list, T item) where T : class
    {
        list.Add(item);
    }
        
    public static void InsertFist<T>(this List<T> list, T item) where T : class
    {
        list.Insert(0,item);
    }
        
    public static void InsertAfter<T>(this List<T> list, T item,T afterItem) where T : class
    {
        var index = list.IndexOf(afterItem);
        list.Insert(index+1,item);
    }
        
    public static void InsertBefore<T>(this List<T> list, T item,T afterItem) where T : class
    {
        var index = list.IndexOf(afterItem);
        list.Insert(index-1,item);
    }
        
    public static void InsertAfter<T>(this List<T> list, List<T> items,T afterItem) where T : class
    {
        var index = list.IndexOf(afterItem);
        list.InsertRange(index+1,items);
    }
        
    public static void InsertBefore<T>(this List<T> list, List<T> items,T afterItem) where T : class
    {
        var index = list.IndexOf(afterItem);
        list.InsertRange(index-1,items);
    }

    public static string ElementsToString<T>(this List<T> list) where T : class
    {
        string result = String.Empty;

        if (list.IsNullOrEmpty())
            return result;
            
        return string.Join(",", list.Select(entity => entity.ToString()));
    }
}
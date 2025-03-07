using System.Collections.Generic;
using UnityEngine;

namespace VentLib.Utilities.Extensions;

/// <summary>
/// Provides extensions for <see cref="Transform"/>.
/// </summary>
public static class TransformExtensions
{
    /// <summary>
    /// Gets all Children of a Transform Object.
    /// </summary>
    /// <param name="transform">The transform to get children for.</param>
    /// <param name="recursive">Whether to get the children of all this transform's children.</param>
    /// <returns>List of children.</returns>
    public static List<Transform> GetChildren(this Transform transform, bool recursive = false)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            children.Add(child);

            if (recursive && transform.childCount > 0) children.AddRange(child.GetChildren(true)); 
        }
        return children;
    }
}
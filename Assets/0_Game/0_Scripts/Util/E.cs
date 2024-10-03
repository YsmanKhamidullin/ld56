using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

public static class E
{
    public static async UniTask FadeIn(this CanvasGroup target, float duration = 0.5f)
    {
        await target.DOFade(1f, duration).ToUniTask();
    }
    
    public static async UniTask FadeOut(this CanvasGroup target, float duration = 0.5f)
    {
        await target.DOFade(0f, duration).ToUniTask();
    }

    public static async UniTask ToUniTask(this Tween t)
    {
        if (!t.active)
        {
            if (Debugger.logPriority > 0) Debugger.LogInvalidTween(t);
            return;
        }

        while (t.active && !t.IsComplete()) await UniTask.Yield();
    }

    public static bool Chance(float chance)
    {
        return Random.value <= chance;
    }

    public static Color ZeroAlpha(this Color color)
    {
        color.a = 0;
        return color;
    }

    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }

    public static Transform CreateWorldSpaceTransform(this RectTransform transform)
    {
        Vector3[] v = new Vector3[4];
        transform.GetWorldCorners(v);
        // get center of corners
        var center = (v[0] + v[2]) * 0.5f;

        var newTransform = new GameObject($"{transform.name}").transform;
        newTransform.SetParent(DynamicContainer.DynamicParent);
        newTransform.position = center;

        return newTransform;
    }

    /// <summary>
    /// Reset pos, localRot, localScale
    /// </summary>
    public static void ResetTransform(this Transform trans)
    {
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Sorting list of vector3. Distance from vector to measureFrom
    /// </summary>
    public static List<Vector3> SortByDistance(this List<Vector3> objects, Vector3 measureFrom)
    {
        return objects.OrderBy(x => Vector3.Distance(x, measureFrom)).ToList();
    }

    public static T1 GetRandom<T1>(this IEnumerable<T1> objects)
    {
        var array = objects as T1[] ?? objects.ToArray();
        var randomIndex = Random.Range(0, array.Length);
        return array[randomIndex];
    }

    /// <summary>
    /// Remove duplicates position.
    /// </summary>
    public static void RemoveNear(ref List<Vector3> removeFrom, Vector3 position,
        Vector3? nearValue = null)
    {
        if (nearValue == null)
        {
            nearValue = Vector3.one * 0.001f;
        }

        if (IsNear(removeFrom, position, nearValue))
        {
            removeFrom.Remove(position);
        }
    }

    /// <summary>
    /// Check duplicate pos
    /// </summary>
    public static bool IsNear(IEnumerable<Vector3> poses, Vector3 target, Vector3? nearValue = null)
    {
        if (nearValue == null)
        {
            nearValue = Vector3.one * 0.001f;
        }

        // Iterate through each calculated pose
        foreach (var pose in poses)
        {
            float distance = Vector3.Distance(pose, target);
            if (distance < nearValue.Value.magnitude)
            {
                return true;
            }
        }

        return false;
    }

    #region Objects Controll

    /// <summary>
    /// Call Destroy depends of Applications is playing or not.
    /// </summary>
    public static void DynamicDestroy<T>(this T original) where T : MonoBehaviour
    {
        if (Application.isPlaying)
        {
            GameObject.Destroy(original.gameObject);
        }
        else
        {
            GameObject.DestroyImmediate(original.gameObject);
        }
    }

    /// <summary>
    /// Call Destroy depends of Applications is playing or not
    /// </summary>
    public static void DynamicDestroy(this GameObject original)
    {
        if (Application.isPlaying)
        {
            GameObject.Destroy(original);
        }
        else
        {
            GameObject.DestroyImmediate(original);
        }
    }

    /// <summary>
    /// Destroy transform child by Type
    /// </summary>
    /// <param name="transform"></param>
    /// <typeparam name="T"></typeparam>
    public static void DestroyChildren<T>(this Transform transform)
        where T : MonoBehaviour
    {
        var collection = transform.GetComponentsInChildren<T>();
        foreach (var t in collection)
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(t.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(t.gameObject);
            }
        }
    }

    public static void DestroyChildren(this Transform transform)
    {
        var childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject gameObject;
            if (Application.isPlaying)
            {
                gameObject = transform.GetChild(i).gameObject;
                GameObject.Destroy(gameObject);
            }
            else
            {
                gameObject = transform.GetChild(0).gameObject;
                GameObject.DestroyImmediate(gameObject);
            }
        }
    }

    #endregion

    public static void SetLayerAllChildren(this Transform root, int layer, bool includeInactive = true)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: includeInactive);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


    public static bool CheckPlayerNear(Vector3 pos, float radius)
    {
        if (!Physics.CheckSphere(pos, radius)) return false;
        var hitColliders = Physics.OverlapSphere(pos, radius);
        return hitColliders.Any(hitCollider => hitCollider.CompareTag("Player"));
    }

    public static class DynamicContainer
    {
        public static Transform DynamicParent
        {
            get
            {
                if (_dynamicContainer != null)
                {
                    return _dynamicContainer;
                }

                _dynamicContainer = new GameObject("_DynamicContainer").transform;
                return _dynamicContainer;
            }
        }

        private static Transform _dynamicContainer;
    }
}
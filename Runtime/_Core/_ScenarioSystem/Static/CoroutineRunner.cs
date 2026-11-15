using UnityEngine;
using System.Collections;

public static class CoroutineRunner
{
    private class CoroutineHolder : MonoBehaviour { }

    private static CoroutineHolder _coroutineHolder;
    private static int _activeCoroutines = 0;

    public static void RunCoroutine(IEnumerator coroutine, GameObject coroutineHolderGo)
    {
        if (_coroutineHolder == null)
        {
            _coroutineHolder = coroutineHolderGo.AddComponent<CoroutineHolder>();
        }

        _activeCoroutines++;
        _coroutineHolder.StartCoroutine(RunAndDestroy(coroutine));
    }

    private static IEnumerator RunAndDestroy(IEnumerator coroutine)
    {
        yield return _coroutineHolder.StartCoroutine(coroutine);
        _activeCoroutines--;

        if (_activeCoroutines == 0)
        {
            Object.Destroy(_coroutineHolder);
            _coroutineHolder = null;
        }
    }
}

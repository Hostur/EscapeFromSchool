using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FCData.Common;
using FCData.DataUtils;
using FCData.DataUtils.DI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.Graphics
{
  public static class GraphicsHelper
  {
    public static readonly Color32 DefaultPositiveButtonColor = new Color32(18, 118, 84, 255);
    public static readonly Color32 DefaultNegativeButtonColor = new Color32(228, 4, 33, 255);
    public static readonly Color32 DefaultCancelButtonColor = new Color32(238, 229, 113, 255);

    private const float _fadeSpeed = 2F;
    /// <summary>
    /// Based on single global value all the fading icons are synchronized.
    /// </summary>
    private static float _sinusBasedVisibility;

    private static List<CanvasGroup> _sinusBasedVisibilityCanvasGroups = new List<CanvasGroup>(12);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty(this Color32 color) => color == new Color();

    public static IEnumerator SetSinusBasedVisibility()
    {
      while (true)
      {
        yield return null;
        _sinusBasedVisibility = (Mathf.Sin(Time.time * _fadeSpeed) * -0.2f) + 0.8f;
        _sinusBasedVisibilityCanvasGroups.Each(c => c != null, c => c.alpha = _sinusBasedVisibility);
      }
    }

    public static void SubscribeAlphaBasedVisibility(this CanvasGroup canvasGroup)
    {
      if (!_sinusBasedVisibilityCanvasGroups.Contains(canvasGroup))
        _sinusBasedVisibilityCanvasGroups.Add(canvasGroup);
    }

    public static void UnsubscribeAlphaBasedVisibility(this CanvasGroup canvasGroup)
    {
      if (_sinusBasedVisibilityCanvasGroups.Contains(canvasGroup))
        _sinusBasedVisibilityCanvasGroups.Remove(canvasGroup);
    }

    public static void SetAlpha(this Graphic graphic, float value)
    {
      if (graphic == null) return;

      Color color = graphic.color;
      color.a = value;
      graphic.color = color;
    }

    public static void SetAlpha(this Graphic[] graphics, float value)
    {
      if (graphics == null) return;
      foreach (Graphic graphic in graphics)
      {
        SetAlpha(graphic, value);
      }
    }

    public static void SetText(this Button button, string text)
    {
      try
      {
        button.transform.GetChild(0).GetComponent<Text>().text = text;
      }
      catch
      {
        try
        {
          button.transform.GetChild(0).GetComponent<TMP_Text>().text = text;
        }
        catch
        {
          // ignored
        }
      }
    }

    public static void SetColor(this Button button, Color color)
    {
      var image = button.gameObject.GetComponent<Image>();
      if (image != null)
      {
        image.color = color;
      }
    }

    public static void Show(this CanvasGroup canvasGroup, float time, Action callback = null)
    {
      God.PrayFor<FCMainThreadActionsQueue>().Enqueue(InternalShow(canvasGroup, time, callback));
    }

    public static void Hide(this CanvasGroup canvasGroup, float time, Action callback = null)
    {
      God.PrayFor<FCMainThreadActionsQueue>().Enqueue(InternalHide(canvasGroup, time, callback));
    }

    public static void Show(this CanvasGroup canvasGroup, FCMainThreadActionsQueue coreMainThreadActionsQueue, float time, Action callback = null)
    {
      coreMainThreadActionsQueue.Enqueue(InternalShow(canvasGroup, time, callback));
    }

    public static void Hide(this CanvasGroup canvasGroup, FCMainThreadActionsQueue coreMainThreadActionsQueue, float time, Action callback = null)
    {
      coreMainThreadActionsQueue.Enqueue(InternalHide(canvasGroup, time, callback));
    }

    private static IEnumerator InternalShow(CanvasGroup canvasGroup, float time, Action callback)
    {
      for (float t = 0f; t < time; t += Time.deltaTime)
      {
        float normalizedTime = t / time;
        if (canvasGroup == null) yield break;
        canvasGroup.alpha = Mathf.Lerp(0, 1, normalizedTime);
        yield return null;
      }

      if (canvasGroup == null) yield break;
      canvasGroup.alpha = 1;
      callback?.Invoke();
    }

    private static IEnumerator InternalHide(CanvasGroup canvasGroup, float time, Action callback)
    {
      for (float t = 0f; t < time; t += Time.deltaTime)
      {
        float normalizedTime = t / time;
        if (canvasGroup == null) yield break;
        canvasGroup.alpha = Mathf.Lerp(1, 0, normalizedTime);
        yield return null;
      }

      if (canvasGroup == null) yield break;
      canvasGroup.alpha = 0;
      callback?.Invoke();
    }
  }
}

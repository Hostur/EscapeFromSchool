using System;
using System.Collections;
using Base;
using FCData.DataUtils.DI;
using FCData.Events;
using FCData.Reflection;
using UnityEngine;

/// <summary>
/// Replace GetComponent() usage.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GetAttribute : Attribute { }

/// <summary>
/// Replace FindObjectOfType() usage.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FindAttribute : Attribute { }

/// <summary>
/// Base class that should be used instead of regular MonoBehaviour in most cases.
/// </summary>
public abstract class FCBehaviour : MonoBehaviour, IDisposable
{
    #region Behaviors

    private bool _disposed;

    protected virtual void OnBeforeInjection()
    {
    }

    protected void Awake()
    {
        //this.AssertSerializeFields();
        OnBeforeInjection();
        this.InjectComponents();
        this.FindComponents();
        this.ResolveMyDependencies();
        this.SubscribeMyEventHandlers();

        OnAwake();
    }

    protected virtual void OnAwake()
    {
    }

    protected void InvokeWithDelay(Action action, float delay)
    {
        StartCoroutine(InvokeWithDelayInternal(action, delay));
    }

    protected void InvokeAtTheEndOfFrame(Action action)
    {
        StartCoroutine(InvokeAtTheEndOfFrameInternal(action));
    }

    protected void Publish<T>(T arg) where T : EventArgs => FCGameEventsManager.Publish(this, arg);

    private IEnumerator InvokeWithDelayInternal(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private IEnumerator InvokeAtTheEndOfFrameInternal(Action action)
    {
        yield return new WaitForEndOfFrame();
        action?.Invoke();
    }

    public void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            BeforeDispose();
            foreach (IDisposable disposable in this.GetCoreDisposableMembers())
            {
                disposable.Dispose();
            }
            this.UnSubscribeMyEventHandlers();
            this.DisposeAllMembers();
            AfterDispose();
        }
    }

    protected virtual void BeforeDispose()
    {
    }

    protected virtual void AfterDispose()
    {
    }
    #endregion
}

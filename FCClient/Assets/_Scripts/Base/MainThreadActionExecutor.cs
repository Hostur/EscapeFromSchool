using System;
using System.Collections;
using FCData.Attributes;
using FCData.Common;

#pragma warning disable 649

public class MainThreadActionExecutor : FCBehaviour
{
	[FCInject] private FCMainThreadActionsQueue _mainThreadActionsQueue;

	private Action _action;
	private IEnumerator _iEnumerator;

	private void Update()
	{
		while (_mainThreadActionsQueue.Dequeue(out _action))
		{
			_action?.Invoke();
		}

		while (_mainThreadActionsQueue.Dequeue(out _iEnumerator))
		{
			StartCoroutine(_iEnumerator);
		}
	}
}
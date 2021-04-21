using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityFirewallPunch : MonoBehaviour
{
	private static string _executablePath;

	private void Awake()
	{
		_executablePath = ExecutablePath();
	}

	private void Start()
	{

	}

	private string ExecutablePath()
    {
	    var path = Application.dataPath;
	    if (Application.platform == RuntimePlatform.OSXPlayer)
	    {
		    path += "/../../";
	    }
	    else if (Application.platform == RuntimePlatform.WindowsPlayer)
	    {
		    path += "/../";
	    }

	    return path;
    }

	private void AddToFirewallRule()
	{
	}
}

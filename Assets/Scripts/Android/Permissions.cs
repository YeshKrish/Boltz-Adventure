using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class Permissions : MonoBehaviour, AndroidPermissionHandler.Delegate
{
	private int _IDENTIFIER_MULTIPLE_PERMISSION = 1;
	private int _IDENTIFIER_PERMISSION_STORAGE = 2;


	public Text resultText;


	public void RequestMultiplePermissions()
	{
		Stack<string> permissions = new Stack<string>();
		permissions.Push(Permission.ExternalStorageWrite);

		AndroidPermissionHandler.instance.RequestMultiplePermission(permissions, this, _IDENTIFIER_MULTIPLE_PERMISSION);
	}

	public void RequestStoragePermission()
	{
		AndroidPermissionHandler.instance.RequestSinglePermission(Permission.ExternalStorageWrite, this, _IDENTIFIER_PERMISSION_STORAGE);
	}

	public void OnCompleteAskingPermissionRequest(int identifier)
	{
		Debug.Log("permission Asking Completed for " + identifier);
		resultText.text = "permission Asking Completed for " + identifier;
	}
}
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    [SerializeField]
    private TMP_Text _authText;    
    [SerializeField]
    private TMP_Text _statusText;

    private FirebaseAuth auth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
    void Start()
    {

        auth = FirebaseAuth.DefaultInstance;


        try
        {
            PlayGamesPlatform.Instance.Authenticate((status) =>
            {
                _statusText.text = $"Status = {status}";

                if (status == SignInStatus.Success)
                {
                    try
                    {
                        PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                        {
                            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

                            Credential credential = PlayGamesAuthProvider.GetCredential(code);

                            StartCoroutine(AuthGet());

                            IEnumerator AuthGet()
                            {
                                System.Threading.Tasks.Task<FirebaseUser> task = auth.SignInWithCredentialAsync(credential);

                                while (!task.IsCompleted) yield return null;

                                if (task.IsCanceled)
                                {
                                    _authText.text += "Auth Cancelled";
                                }
                                else if (task.IsFaulted)
                                {
                                    //task.exception
                                    _authText.text += "Faulted" + task.Exception;
                                }
                                else
                                {
                                    Firebase.Auth.FirebaseUser user = auth.CurrentUser;
                                    _authText.text += $"Fully Authenticated User: displayName: {user.UserId}";
                                }
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        _authText.text += e.ToString();
                    }

                }
                else
                {
                    _authText.text += "Autentication Failed";
                }
            });
        }
        catch (Exception e)
        {
            _authText.text += "Exception " + e.ToString();
        }
    }

}

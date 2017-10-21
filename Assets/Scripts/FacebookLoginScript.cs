using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine.SceneManagement;

//이걸 꼭 추가...?
using Assets.Scripts.CloudBread;
using JsonFx.Json;


public class FacebookLoginScript : MonoBehaviour
{

    // Awake function from Unity's MonoBehavior
    void Awake()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void LoginwithPermissions()
    {
        //Debug.Log("get in 2");
        var perms = new List<string>() { "public_profile", "email", "user_friends" };
        //Debug.Log("get in 3");
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }

            // 유저 이름(닉네임) 불러오기
            FB.API("me?fields=name", HttpMethod.GET, NameCallBack);


            // 인증 토큰 가져오기
            // TODO : CloudBread 클래스 생성 후, Login API 호출
            CloudBread cb = new CloudBread();
            cb.Login(AzureAuthentication.AuthenticationProvider.Facebook, aToken.TokenString, Callback_login);
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }

    private void Callback_login(string id, WWW www)
    {
        print(www.text);
        string resultJson = www.text;
        
        JsonReader jsonReader = new JsonReader();
        AuthData resultData = jsonReader.Read<AuthData>(resultJson);

        // Azure 인증을 위해 발급받은 Azure Token 을 헤더에 추가
        AzureMobileAppRequestHelper.AuthToken = resultData.authenticationToken;

        // 게임에서 사용 할 userId 를 PlayerPrefs 에 저장
        // 원래 게임에서는 아래와 같이, Azure 에서 제공해 주는 userID 를 넣는 것이 맞지만,
        // 데모 서버를 사용하는 분들은 임의의 아이디를 넣어서 다른 사람들과 충돌이 나지 않도록 합시다
        PlayerPrefs.SetString("userId", resultData.user.userId);

        CloudBread cb = new CloudBread();
        cb.CBInsRegMember(Callback_CBInsRegMember);
        
    }

    public void Callback_CBInsRegMember(string id, WWW www)
    {
        if (www.error != null) //새로 회원 가입
        {
            print("이미 가입된 회원");
            StartGame();


        }
        else //이미 가입된 회원
        {
            print("새로 가입한 회원");
            StartGame();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("mainGame");
    }

    private void NameCallBack(IGraphResult result)
    {
        string userName = (string)result.ResultDictionary["name"];
        print(userName + "님 안녕하세요^^");
        PlayerPrefs.SetString("nickName", userName);
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    public void FacebookLoginBtnClick()
    {
        if (!FB.IsLoggedIn)
        {
            //Debug.Log("get in");
            LoginwithPermissions();
        }
    }
}
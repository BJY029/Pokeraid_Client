using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
	//UI prefab들을 띄울 캔버스
    public Transform canvas;
	//로그인 UI 프리팹을 저장할 오브젝트
    GameObject loginObj = null;
	//회원가입 UI 프리팹을 저장할 오브젝트
    GameObject registerObj = null;

	private void Start()
	{
		Init();
	}

	//초기화 함수
	private void Init()
	{
		loginObj = null;
		registerObj = null;

		GameDataManager.Instance.ResetData();

		//캔버스 연결
		if(canvas == null)
		{
			canvas = GameObject.Find("Canvas").transform;
		}

		//로그인 프리팹 생성 및 저장
		GameObject prefab = Resources.Load<GameObject>("prefabs/Login");
		loginObj = Instantiate(prefab, canvas);

		//해당 프리팹 버튼에 관련 이벤트 함수 연결
		loginObj.transform.Find("LoginBtn").GetComponent<Button>().onClick.AddListener(OnClickLogin);
		loginObj.transform.Find("RegisterBtn").GetComponent<Button>().onClick.AddListener(OnClickRegisterPage);
	}

	//로그인 시도 버튼에 삽입될 함수
	private void OnClickLogin()
	{
		//id와 password를 가져와서
		string id = loginObj.transform.Find("ID").GetComponent<TMP_InputField>().text;
		string password = loginObj.transform.Find("Password").GetComponent<TMP_InputField>().text;

		//디버그용 출력
		Debug.Log("id : " + id + "Password : " + password);

		NetworkManager.Instance.SendLoginServer(CommonDefine.LOGIN_URL, id, password, CallbackLogin);
	}

	//회원가입 페이지로 넘어가는 버튼에 삽입될 함수
	private void OnClickRegisterPage()
	{
		//회원가입 프리팹이 null인 경우
		if(registerObj == null)
		{
			//새로 생성 후 저장
			GameObject prefab = Resources.Load<GameObject>("prefabs/Register");
			registerObj = Instantiate(prefab, canvas);

			//관련 이벤트 함수 연결
			registerObj.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(OnClickLoginPage);
			registerObj.transform.Find("RegisterBtn").GetComponent<Button>().onClick.AddListener(OnClickRegister);
		}
		else
		{
			//프리팹이 존재하는 경우, 활성화
			registerObj.SetActive(true);
		}

		//ID, PW 초기화
		registerObj.transform.Find("ID").GetComponent<TMP_InputField>().text = "";
		registerObj.transform.Find("Password").GetComponent<TMP_InputField>().text = "";
	}

	//회원가입 페이지에서 로그인 페이지로 돌아가는 함수
	private void OnClickLoginPage()
	{
		//회원가입 UI 비활성화
		registerObj.SetActive(false);
		//로그인 입력창 초기화
		loginObj.transform.Find("ID").GetComponent<TMP_InputField>().text = "";
		loginObj.transform.Find("Password").GetComponent<TMP_InputField>().text = "";
	}

	//회원가입 시도 버튼에 삽입될 함수
	private void OnClickRegister()
	{
		//입력된 id와 pw를 받아와서
		string id = registerObj.transform.Find("ID").GetComponent<TMP_InputField>().text;
		string password = registerObj.transform.Find("Password").GetComponent<TMP_InputField>().text;

		//디버그 출력
		Debug.Log("id : " + id + "pw : " + password);

		NetworkManager.Instance.SendLoginServer(CommonDefine.REGISTER_URL, id, password, CallbackRegister);
	}

	//로그인 성공 및 실패에 따라 호출될 콜백 함수
	private void CallbackLogin(bool result)
	{
		if (result)
		{
			CreateMsgBoxOnBtn("로그인 성공", GetMyPokemon);
		}
		else
		{
			CreateMsgBoxOnBtn("로그인 실패");
		}
	}

	void GetMyPokemon()
	{
		NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_POKEMON_URL, null, CallbackPokemon);
	}

	void CallbackPokemon(bool result)
	{
		if(result)
		{
			GetMyWallet();
		}
		else
		{
			CreateMsgBoxOnBtn("포켓몬 로드 실패");
		}
	}

	//회원가입 성공 및 실패에 따라 호출될 콜백 함수
	private void CallbackRegister(bool result)
	{
		if (result)
		{
			CreateMsgBoxOnBtn("회원가입 성공", OnClickLoginPage);
		}
		else
		{
			CreateMsgBoxOnBtn("회원가입 실패");
		}
	}

	void GetMyWallet()
	{
		NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_WALLET_URL, null, CallbackMyWallet);
	}

	void CallbackMyWallet(bool result)
	{
		if (!result)
		{
			Debug.Log("내 지갑 로드 실패");
		}
		LoadScene(CommonDefine.GAME_SCENE);
	}

	//성공 여부를 알려주는 메시지 박스를 띄우는 함수
	private void CreateMsgBoxOnBtn(string desc, Action checkResult = null)
	{
		//해당 프리팹을 생성하고
		GameObject msgBoxPrefabOnBtn = Resources.Load<GameObject>("prefabs/MessageBox_1Button");
		GameObject obj = Instantiate(msgBoxPrefabOnBtn, canvas);

		//관련 버튼에 이벤트 함수 연결
		obj.transform.Find("desc").GetComponent<TMP_Text>().text = desc;
		//만약 action이 따로 정의되지 않은 경우, 해당 버튼 누르면 파괴되도록 설정
		obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

		//매개변수로 넘어온 이벤트 함수가 존재하면, 해당 함수를 연결
		if(checkResult != null)
		{
			obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => checkResult());
		}

	}

	//오브젝트 파괴 함수
	private void DestroyObject(GameObject obj)
	{
		Destroy(obj);
	}

	void LoadScene(string nextSceneName)
	{
		GameDataManager.Instance.nextScene = nextSceneName;
		SceneManager.LoadScene(CommonDefine.LOADING_SCENE); 
	}
}

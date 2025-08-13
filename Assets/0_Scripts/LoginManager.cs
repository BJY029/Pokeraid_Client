using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
	//UI prefab���� ��� ĵ����
    public Transform canvas;
	//�α��� UI �������� ������ ������Ʈ
    GameObject loginObj = null;
	//ȸ������ UI �������� ������ ������Ʈ
    GameObject registerObj = null;

	private void Start()
	{
		Init();
	}

	//�ʱ�ȭ �Լ�
	private void Init()
	{
		loginObj = null;
		registerObj = null;

		GameDataManager.Instance.ResetData();

		//ĵ���� ����
		if(canvas == null)
		{
			canvas = GameObject.Find("Canvas").transform;
		}

		//�α��� ������ ���� �� ����
		GameObject prefab = Resources.Load<GameObject>("prefabs/Login");
		loginObj = Instantiate(prefab, canvas);

		//�ش� ������ ��ư�� ���� �̺�Ʈ �Լ� ����
		loginObj.transform.Find("LoginBtn").GetComponent<Button>().onClick.AddListener(OnClickLogin);
		loginObj.transform.Find("RegisterBtn").GetComponent<Button>().onClick.AddListener(OnClickRegisterPage);
	}

	//�α��� �õ� ��ư�� ���Ե� �Լ�
	private void OnClickLogin()
	{
		//id�� password�� �����ͼ�
		string id = loginObj.transform.Find("ID").GetComponent<TMP_InputField>().text;
		string password = loginObj.transform.Find("Password").GetComponent<TMP_InputField>().text;

		//����׿� ���
		Debug.Log("id : " + id + "Password : " + password);

		NetworkManager.Instance.SendLoginServer(CommonDefine.LOGIN_URL, id, password, CallbackLogin);
	}

	//ȸ������ �������� �Ѿ�� ��ư�� ���Ե� �Լ�
	private void OnClickRegisterPage()
	{
		//ȸ������ �������� null�� ���
		if(registerObj == null)
		{
			//���� ���� �� ����
			GameObject prefab = Resources.Load<GameObject>("prefabs/Register");
			registerObj = Instantiate(prefab, canvas);

			//���� �̺�Ʈ �Լ� ����
			registerObj.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(OnClickLoginPage);
			registerObj.transform.Find("RegisterBtn").GetComponent<Button>().onClick.AddListener(OnClickRegister);
		}
		else
		{
			//�������� �����ϴ� ���, Ȱ��ȭ
			registerObj.SetActive(true);
		}

		//ID, PW �ʱ�ȭ
		registerObj.transform.Find("ID").GetComponent<TMP_InputField>().text = "";
		registerObj.transform.Find("Password").GetComponent<TMP_InputField>().text = "";
	}

	//ȸ������ ���������� �α��� �������� ���ư��� �Լ�
	private void OnClickLoginPage()
	{
		//ȸ������ UI ��Ȱ��ȭ
		registerObj.SetActive(false);
		//�α��� �Է�â �ʱ�ȭ
		loginObj.transform.Find("ID").GetComponent<TMP_InputField>().text = "";
		loginObj.transform.Find("Password").GetComponent<TMP_InputField>().text = "";
	}

	//ȸ������ �õ� ��ư�� ���Ե� �Լ�
	private void OnClickRegister()
	{
		//�Էµ� id�� pw�� �޾ƿͼ�
		string id = registerObj.transform.Find("ID").GetComponent<TMP_InputField>().text;
		string password = registerObj.transform.Find("Password").GetComponent<TMP_InputField>().text;

		//����� ���
		Debug.Log("id : " + id + "pw : " + password);

		NetworkManager.Instance.SendLoginServer(CommonDefine.REGISTER_URL, id, password, CallbackRegister);
	}

	//�α��� ���� �� ���п� ���� ȣ��� �ݹ� �Լ�
	private void CallbackLogin(bool result)
	{
		if (result)
		{
			CreateMsgBoxOnBtn("�α��� ����", GetMyPokemon);
		}
		else
		{
			CreateMsgBoxOnBtn("�α��� ����");
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
			CreateMsgBoxOnBtn("���ϸ� �ε� ����");
		}
	}

	//ȸ������ ���� �� ���п� ���� ȣ��� �ݹ� �Լ�
	private void CallbackRegister(bool result)
	{
		if (result)
		{
			CreateMsgBoxOnBtn("ȸ������ ����", OnClickLoginPage);
		}
		else
		{
			CreateMsgBoxOnBtn("ȸ������ ����");
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
			Debug.Log("�� ���� �ε� ����");
		}
		LoadScene(CommonDefine.GAME_SCENE);
	}

	//���� ���θ� �˷��ִ� �޽��� �ڽ��� ���� �Լ�
	private void CreateMsgBoxOnBtn(string desc, Action checkResult = null)
	{
		//�ش� �������� �����ϰ�
		GameObject msgBoxPrefabOnBtn = Resources.Load<GameObject>("prefabs/MessageBox_1Button");
		GameObject obj = Instantiate(msgBoxPrefabOnBtn, canvas);

		//���� ��ư�� �̺�Ʈ �Լ� ����
		obj.transform.Find("desc").GetComponent<TMP_Text>().text = desc;
		//���� action�� ���� ���ǵ��� ���� ���, �ش� ��ư ������ �ı��ǵ��� ����
		obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

		//�Ű������� �Ѿ�� �̺�Ʈ �Լ��� �����ϸ�, �ش� �Լ��� ����
		if(checkResult != null)
		{
			obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => checkResult());
		}

	}

	//������Ʈ �ı� �Լ�
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

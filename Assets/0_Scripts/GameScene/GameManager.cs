using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	//���� ������ ����� UI ĵ����
    public Transform Canvas;

	//���� ���� ��� UI ������Ʈ
    GameObject lobbyObj = null;
	GameObject shopObj = null;
	GameObject inventoryObj = null;
	GameObject loadingCircleObj = null;

	//���� ������ ����Ʈ
	List<GameObject> shopItemObjList = new List<GameObject>();
	//�κ��丮 ������ ����Ʈ
	List<GameObject> inventoryList = new List<GameObject>();

	private void Awake()
	{
		Debug.Log("GameManager Init");
	}

	private void Start()
	{
		Init();
		EasterEggInit();
	}

	//Ư�� UI�� ���� �����ϴ� �̽��Ϳ��� �Լ�
	private void Update()
	{
		EasterEgg();
	}

	//GameScene �ҷ�����
	void Init()
	{
		lobbyObj = null;

		if(Canvas == null)
		{
			Canvas = GameObject.Find("Canvas").transform;
		}

		GameObject prefab = Resources.Load<GameObject>("prefabs/GameLobby");
		lobbyObj = Instantiate(prefab, Canvas);

		lobbyObj.transform.Find("LogOutBtn").GetComponent<Button>().onClick.AddListener(OnClickLogOut);
		lobbyObj.transform.Find("Wallet/LinkWalletBtn").GetComponent<Button>().onClick.AddListener(OnClickLinkWalletPage);
		lobbyObj.transform.Find("Wallet/UpdateWalletBtn").GetComponent<Button>().onClick.AddListener(OnClickUpdateWallet);
		lobbyObj.transform.Find("ShopBtn").GetComponent<Button>().onClick.AddListener(OnClickEnterShop);
		lobbyObj.transform.Find("InvenBtn").GetComponent<Button>().onClick.AddListener(OnClickEnterInventory);
	}

	//���� ��ư�� ������ ȣ��Ǵ� �Լ�
	void OnClickEnterShop()
	{
		//���� ���� �����Ͱ� ���� ���
		if(GameDataManager.Instance.pokemonShopList == null)
		{
			//���� �����͸� �޾ƿ´�.
			NetworkManager.Instance.SendServerGet(CommonDefine.SHOP_LIST_URL, null, CallbackShopList);
		}
		else
		{
			//���� �����Ͱ� ������ ������ ����.
			CreateShop();
		}
	}

	//���� �����͸� �ҷ��� ��, ���� ���ο� ���� ������ ����.
	void CallbackShopList(bool result)
	{
		if (result)
		{
			CreateShop();
		}
		else
		{
			CreateMsgBoxOnBtn("���� �ε� ����");
		}
	}

	//������ �����ϴ� �Լ�
	void CreateShop()
	{
		//���� ������Ʈ�� ������ �ҷ��ͼ� �����Ѵ�.
		if(shopObj == null)
		{
			GameObject prefab = Resources.Load<GameObject>("prefabs/Shop");
			shopObj = Instantiate(prefab, Canvas);
		}

		//��ư �̺�Ʈ �߰�
		shopObj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(shopObj));

		//��������Ʈ �̹����� ��� �ҷ��´�.
		Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
		//������ ������ ǥ���� �������� �ҷ��´�.
		GameObject itemPrefab = Resources.Load<GameObject>("prefabs/ShopItem");
		//������ ������ ��Ÿ�� ��ġ�� �����Ѵ�.
		Transform content = shopObj.transform.Find("ScrollView/Viewport/Content");

		//�켱 ���� ������ �������� ��� �����Ѵ�.
		foreach(Transform child in content)
		{
			Destroy(child);
		}
		//���� �����͵� �ʱ�ȭ�Ѵ�.
		shopItemObjList.Clear();

		//�ҷ��� ���� �����͵��� ��� ���鼭
		for(int i = 0; i < GameDataManager.Instance.pokemonShopList.Length; i++)
		{
			//���� ������ �ϳ��� �����ͼ�
			var shopItem = GameDataManager.Instance.pokemonShopList[i];

			//�̹� �������� ���θ� �����ϰ�
			bool isHave = false;
			//���� ���������� true�� ����
			if(GameDataManager.Instance.myPokemonIds != null && GameDataManager.Instance.myPokemonIds.Contains(shopItem.pokemon.id))
			{
				isHave = true;
			}

			//������ �������� content�� �����ϰ�
			GameObject itemObj = Instantiate(itemPrefab, content);
			//�ش� �����տ� ���� ������ ������ �����Ѵ�.
			itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[shopItem.pokemon.id - 1];
			itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = shopItem.pokemon.name;
			itemObj.transform.Find("Context").GetComponent<TMP_Text>().text
				= "hp : " + shopItem.pokemon.hp.ToString() + " / ���� : " + shopItem.price.ToString();

			//�̹� ������ �������� ���
			if (isHave)
			{
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "����";
			}
			else
			{
				//�������� ���� �������̸�, ���� ��ư�� ���� ���� �Լ��� �߰��Ѵ�.
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "����";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PurchasePokemon(shopItem.shop_id));
			}
			shopItemObjList.Add(itemObj);
		}
	}

	//���� �Լ�
	void PurchasePokemon(int idx)
	{
		//��� â ����
		CreateLodingCircle();
		Debug.Log("Purchased Pokemon : " + idx);
		//���� �����͸� �����ϰ�
		PurchasePostData data = new PurchasePostData
		{
			itemId = idx,
		};
		//��û �߽�
		NetworkManager.Instance.SendServerPost(CommonDefine.SHOP_PURCHASE_URL, data, CallbackPurchasePokemon);
	}

	//���� ���ο� ���� ����� �Լ�
	void CallbackPurchasePokemon(bool result)
	{
		if(result)
		{
			//���Ÿ� ������ ���, �������� �� ���ϸ� ������ �����´�.
			NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_POKEMON_URL, null, CallbackMyPokemonAfterPurchasePokemon);
		}
		else
		{
			DestroyLoadingCircle();
			CreateMsgBoxOnBtn("���� ���� ����");
		}
	}

	//�� ���ϸ� ������ �� �����Դ��� Ȯ��
	void CallbackMyPokemonAfterPurchasePokemon(bool result)
	{
		DestroyLoadingCircle();
		if(result)
		{
			CreateMsgBoxOnBtn("���� �Ϸ�");
			UpdateShopItems();
		}
		else
		{
			CreateMsgBoxOnBtn("���� ���� �� ���ϸ� �ε� ����");
		}
	}

	//���ϸ� ������ ������ �� ���� ���¸� ������Ʈ �ϴ� �Լ�
	void UpdateShopItems()
	{
		//��� ���� �����͸� ���鼭
		for(int i = 0; i <GameDataManager.Instance.pokemonShopList.Length; i++)
		{
			var shopItem = GameDataManager.Instance.pokemonShopList[i];

			//���� ���θ� üũ�ϰ�
			bool isHave = false;
			if(GameDataManager.Instance.myPokemonIds != null && GameDataManager.Instance.myPokemonIds.Contains(shopItem.shop_id))
			{
				isHave = true;
			}

			//�� ���� �����Ϳ� ���� �������� ��ǥ���Ѵ�.
			GameObject itemObj = shopItemObjList[i];
			itemObj.transform.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();

			if (isHave)
			{
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "����";
			}
			else
			{
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "����";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PurchasePokemon(shopItem.shop_id));
			}
		}
	}


	//�κ��丮 ��ư�� ������ �� ����� �Լ�
	void OnClickEnterInventory()
	{
		//���� �� ���ϸ� �����Ͱ� ���� ���
		if (GameDataManager.Instance.myPokemonIds == null)
		{
			//�����͸� �޾ƿ´�.
			NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_POKEMON_URL, null, CallbackCreateInventory);
		}
		else
		{
			//���ϸ� �����Ͱ� �ִ� ��� �κ��丮�� ����.
			CreateInventory();
		}
	}

	//ó�� ����� ���� ����� �Լ�
	void CallbackCreateInventory(bool result)
	{
		if(result)
		{
			CreateInventory();
		}
		else
		{
			CreateMsgBoxOnBtn("�� ���ϸ� ������ �ҷ����� ����");
		}
	}

	//�κ��丮 â ����
	void CreateInventory()
	{
		//�κ���� ������Ʈ�� ������ �ҷ��ͼ� �����Ѵ�.
		if (inventoryObj == null)
		{
			GameObject prefab = Resources.Load<GameObject>("prefabs/Inventory");
			inventoryObj = Instantiate(prefab, Canvas);
		}

		//��ư �̺�Ʈ �߰�
		inventoryObj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(inventoryObj));

		//��������Ʈ �̹����� ��� �ҷ��´�.
		Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
		//������ ������ ǥ���� �������� �ҷ��´�.
		GameObject itemPrefab = Resources.Load<GameObject>("prefabs/InventoryItem");
		//������ ������ ��Ÿ�� ��ġ�� �����Ѵ�.
		Transform content = inventoryObj.transform.Find("ScrollView/Viewport/Content");

		//�켱 ���� ������ �������� ��� �����Ѵ�.
		foreach (Transform child in content)
		{
			Destroy(child);
		}
		//�κ��丮 �����͵� �ʱ�ȭ�Ѵ�.
		inventoryList.Clear();

		//�ҷ��� ������ �����͵��� ��� ���鼭
		for (int i = 0; i < GameDataManager.Instance.myPokemonList.Length; i++)
		{
			//���� ������ �ϳ��� �����ͼ�
			var inventoryItem = GameDataManager.Instance.myPokemonList[i];

			//�ش� ���ϸ��� ���� ���õǾ��ִ��� Ȯ���Ѵ�.
			bool picked = false;
			if(GameDataManager.Instance.pickedPokemon != null && GameDataManager.Instance.pickedPokemon.pokemonId == inventoryItem.pokemonId)
				picked = true;

			//������ �������� content�� �����ϰ�
			GameObject itemObj = Instantiate(itemPrefab, content);
			//�ش� ������ ������Ʈ�� ���� ������ ������ �����Ѵ�.
			itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[inventoryItem.pokemonId - 1];
			itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = inventoryItem.name;
			itemObj.transform.Find("Context").GetComponent<TMP_Text>().text
				= "hp : " + inventoryItem.hp.ToString() + " / damage : " + inventoryItem.skills[0].damage;

			//�̹� ���õ� �������� ���
			if (picked)
			{
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "�����";
			}
			else
			{
				//���õ��� ���� �������� ���, ���� ���� ������ ��ư�� �����Ѵ�.
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "���";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PickPokemon(inventoryItem));
			}
			inventoryList.Add(itemObj);
		}

	}

	//���ϸ��� �����ϴ� �Լ�
	void PickPokemon(MyPokemon pickedPokemon)
	{
		Debug.Log("picked : " + pickedPokemon.name);
		//���� ���ϸ��� ������Ʈ �ϰ�
		GameDataManager.Instance.pickedPokemon = pickedPokemon;
		//���� ������ �κ��丮 â������ ������Ʈ �Ѵ�.
		UpdateInventoryItem();
	}

	//�κ��丮 â�� ������Ʈ �ϴ� �Լ�
	void UpdateInventoryItem()
	{
		//�ҷ��� ���� �����͵��� ��� ���鼭
		for (int i = 0; i < GameDataManager.Instance.myPokemonList.Length; i++)
		{
			//���� ������ �ϳ��� �����ͼ�
			var inventoryItem = GameDataManager.Instance.myPokemonList[i];

			//�ش� �������� ���� ���ο� ���ؼ� Ȯ�� ��
			bool picked = false;
			if (GameDataManager.Instance.pickedPokemon != null && GameDataManager.Instance.pickedPokemon.pokemonId == inventoryItem.pokemonId)
				picked = true;

			//���� �ش� ������Ʈ�� ����Ʈ���� �����ͼ�
			GameObject itemObj = inventoryList[i];
			//���Ե� �̺�Ʈ �Լ��� �������ְ�
			itemObj.transform.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();

			//���õ� �������� ���
			if (picked)
			{
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "�����";
			}
			else
			{
				//���õ��� ���� �������̸� ���� �̺�Ʈ �Լ��� �������ش�.
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "���";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PickPokemon(inventoryItem));
			}
		}
	}

	//�α׾ƿ� ��ư�� ������ ����� �Լ�
	void OnClickLogOut()
	{
		LoadScene(CommonDefine.LOGIN_SCENE);
	}

	void OnClickLinkWalletPage()
	{
		GameObject prefab = Resources.Load<GameObject>("prefabs/LinkWallet");
		GameObject obj = Instantiate(prefab, Canvas);

		obj.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
		obj.transform.Find("LinkBtn").GetComponent<Button>().onClick.AddListener(() => OnClickLinkWallet(obj));
		obj.transform.Find("LinkBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));
	}

	void OnClickLinkWallet(GameObject obj)
	{
		string privateKey = obj.transform.Find("PrivateKey").GetComponent<TMP_InputField>().text;

		LinkWalletPostData data = new LinkWalletPostData
		{
			privateKey = privateKey,
		};

		NetworkManager.Instance.SendServerPost(CommonDefine.LINK_WALLET_URL, data, CallbackLinkWallet);
	}

	void CallbackLinkWallet(bool result)
	{
		if (result)
		{
			CreateMsgBoxOnBtn("���� ���� ����", OnClickUpdateWallet);
		}
		else
		{
			CreateMsgBoxOnBtn("���� ���� ����");
		}
	}

	//���� ���� �Լ�
	void OnClickUpdateWallet()
	{
		NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_WALLET_URL, null, UpdateWallet);
	}

	//Balance �ҷ����� ���� ���ο� ���� �б� ����Ǵ� �ݹ� �Լ�
	void UpdateWallet(bool result)
	{
		//balance �ҷ����� ���� �ϸ� ���� �α� ���
		if (!result) Debug.Log("�� ���� �ε� ����");

		//�ܾ��� ������ ��� ����� �ҷ������� �ʾҴٴ� ���� ����
		if(GameDataManager.Instance.walletBalance < 0)
		{
			//������ ǥ��
			lobbyObj.transform.Find("Wallet/balance").GetComponent<TMP_Text>().text = "���� ���� �ȵ�.";
		}
		else
		{
			//����� �ҷ����� ���
			lobbyObj.transform.Find("Wallet/balance").GetComponent<TMP_Text>().text
				= "�ܾ� : " + GameDataManager.Instance.walletBalance.ToString("F2");
		}
	}

	//�Ա� �Լ�
	void OnClickGrant()
	{
		CreateLodingCircle();
		//���� body ������ ����
		WalletGetSetPostData data = new WalletGetSetPostData
		{
			amount = "100"
		};
		//post�� �ش� �����͸� �Բ� ����
		NetworkManager.Instance.SendServerPost(CommonDefine.BLOCKCHAIN_GRANT_URL, data, CallbackGrant);
	}

	//��� �Լ�
	void OnClickDeduct()
	{
		CreateLodingCircle();
		//body ������ ����
		WalletGetSetPostData data = new WalletGetSetPostData
		{
			amount = "100"
		};
		//post�� �ش� �����͸� ������ �Բ� ����
		NetworkManager.Instance.SendServerPost(CommonDefine.BLOCKCHAIN_DEDUCT_URL, data, CallbackDeduct);
	}

	//Grant ���� ���ο� ���� �б� ����� �ݹ� �Լ�
	void CallbackGrant(bool result)
	{
		DestroyLoadingCircle();
		if (result)
		{
			//������ Balance ���� ������Ʈ
			CreateMsgBoxOnBtn("CallbackGrant ����", OnClickUpdateWallet);
		}
		else
		{
			CreateMsgBoxOnBtn("CallbackGrant ����");
		}
	}

	//Deduct ���� ���ο� ���� �б� ����� �ݹ� �Լ�
	void CallbackDeduct(bool result)
	{
		DestroyLoadingCircle();
		if (result)
		{
			//������ Balance ���� ������Ʈ
			CreateMsgBoxOnBtn("CallbackDeduct ����", OnClickUpdateWallet);
		}
		else
		{
			CreateMsgBoxOnBtn("CallbackDeduct ����");
		}
	}

	private void CreateMsgBoxOnBtn(string desc, Action checkResult = null)
	{
		//�ش� �������� �����ϰ�
		GameObject msgBoxPrefabOnBtn = Resources.Load<GameObject>("prefabs/MessageBox_1Button");
		GameObject obj = Instantiate(msgBoxPrefabOnBtn, Canvas);

		//���� ��ư�� �̺�Ʈ �Լ� ����
		obj.transform.Find("desc").GetComponent<TMP_Text>().text = desc;
		//���� action�� ���� ���ǵ��� ���� ���, �ش� ��ư ������ �ı��ǵ��� ����
		obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

		//�Ű������� �Ѿ�� �̺�Ʈ �Լ��� �����ϸ�, �ش� �Լ��� ����
		if (checkResult != null)
		{
			obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => checkResult());
		}
	}

	//�ε� ������ �켱 �̵� ��, �񵿱�� �κ� �� �ε� �� �̵�
	void LoadScene(string nextSceneName)
	{
		GameDataManager.Instance.nextScene = nextSceneName;
		SceneManager.LoadScene(CommonDefine.LOADING_SCENE);
	}

	void CreateLodingCircle()
	{
		GameObject prefab = Resources.Load<GameObject>("prefabs/LoadingCircle");
		loadingCircleObj = Instantiate(prefab, Canvas);
	}

	void DestroyLoadingCircle()
	{
		DestroyObject(loadingCircleObj);
	}

	void DestroyObject(GameObject obj)
	{
		Destroy(obj);
	}

	//�̽��� ���׿� ���� ����
	int upArrowCount = 0;

	void EasterEggInit()
	{
		upArrowCount = 0;

		lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(false);
		lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(false);

		lobbyObj.transform.Find("GrantBtn").GetComponent<Button>().onClick.AddListener(OnClickGrant);
		lobbyObj.transform.Find("DeductBtn").GetComponent<Button>().onClick.AddListener(OnClickDeduct);
	}
	void EasterEgg()
	{
		//�� ȭ��ǥ�� 3�� ������ Ư�� ��ư�� Ȱ��ȭ
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			upArrowCount++;
			if (upArrowCount >= 3)
			{
				lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(true);
				lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(true);
			}
		}
		
		//�� ȭ��ǥ ���� �ٸ� ȭ��ǥ�� ������, Ư�� ��ư ��Ȱ��ȭ
		if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
		{
			upArrowCount = 0;
			lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(false);
			lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(false);
		}
	}
}

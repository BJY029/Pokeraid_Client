using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	//게임 씬에서 사용할 UI 캔버스
    public Transform Canvas;

	//게임 씬에 띄울 UI 오브젝트
    GameObject lobbyObj = null;
	GameObject shopObj = null;
	GameObject inventoryObj = null;
	GameObject loadingCircleObj = null;

	//상점 아이템 리스트
	List<GameObject> shopItemObjList = new List<GameObject>();
	//인벤토리 아이템 리스트
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

	//특정 UI를 띄우고 해제하는 이스터에그 함수
	private void Update()
	{
		EasterEgg();
	}

	//GameScene 불러오기
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

	//상점 버튼을 누르면 호출되는 함수
	void OnClickEnterShop()
	{
		//아직 상점 데이터가 없는 경우
		if(GameDataManager.Instance.pokemonShopList == null)
		{
			//상점 데이터를 받아온다.
			NetworkManager.Instance.SendServerGet(CommonDefine.SHOP_LIST_URL, null, CallbackShopList);
		}
		else
		{
			//상점 데이터가 있으면 상점을 연다.
			CreateShop();
		}
	}

	//상점 데이터를 불러온 후, 성공 여부에 따라 상점을 연다.
	void CallbackShopList(bool result)
	{
		if (result)
		{
			CreateShop();
		}
		else
		{
			CreateMsgBoxOnBtn("상점 로드 실패");
		}
	}

	//상점을 생성하는 함수
	void CreateShop()
	{
		//상점 오브젝트가 없으면 불러와서 저장한다.
		if(shopObj == null)
		{
			GameObject prefab = Resources.Load<GameObject>("prefabs/Shop");
			shopObj = Instantiate(prefab, Canvas);
		}

		//버튼 이벤트 추가
		shopObj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(shopObj));

		//스프라이트 이미지를 모두 불러온다.
		Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
		//아이템 정보를 표시할 프리팹을 불러온다.
		GameObject itemPrefab = Resources.Load<GameObject>("prefabs/ShopItem");
		//아이템 정보를 나타낼 위치를 정의한다.
		Transform content = shopObj.transform.Find("ScrollView/Viewport/Content");

		//우선 기존 아이템 정보들은 모두 삭제한다.
		foreach(Transform child in content)
		{
			Destroy(child);
		}
		//상점 데이터도 초기화한다.
		shopItemObjList.Clear();

		//불러온 상점 데이터들을 모두 돌면서
		for(int i = 0; i < GameDataManager.Instance.pokemonShopList.Length; i++)
		{
			//상점 아이템 하나를 가져와서
			var shopItem = GameDataManager.Instance.pokemonShopList[i];

			//이미 보유한지 여부를 정의하고
			bool isHave = false;
			//만약 보유했으면 true로 설정
			if(GameDataManager.Instance.myPokemonIds != null && GameDataManager.Instance.myPokemonIds.Contains(shopItem.pokemon.id))
			{
				isHave = true;
			}

			//아이템 프리팹을 content에 생성하고
			GameObject itemObj = Instantiate(itemPrefab, content);
			//해당 프리팹에 관련 아이템 정보를 삽입한다.
			itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[shopItem.pokemon.id - 1];
			itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = shopItem.pokemon.name;
			itemObj.transform.Find("Context").GetComponent<TMP_Text>().text
				= "hp : " + shopItem.pokemon.hp.ToString() + " / 가격 : " + shopItem.price.ToString();

			//이미 보유한 아이템일 경우
			if (isHave)
			{
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "보유";
			}
			else
			{
				//보유하지 않은 아이템이면, 구매 버튼에 구매 관련 함수를 추가한다.
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "구매";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PurchasePokemon(shopItem.shop_id));
			}
			shopItemObjList.Add(itemObj);
		}
	}

	//구매 함수
	void PurchasePokemon(int idx)
	{
		//대기 창 띄우고
		CreateLodingCircle();
		Debug.Log("Purchased Pokemon : " + idx);
		//보낼 데이터를 정의하고
		PurchasePostData data = new PurchasePostData
		{
			itemId = idx,
		};
		//요청 발신
		NetworkManager.Instance.SendServerPost(CommonDefine.SHOP_PURCHASE_URL, data, CallbackPurchasePokemon);
	}

	//구매 여부에 따라 실행될 함수
	void CallbackPurchasePokemon(bool result)
	{
		if(result)
		{
			//구매를 성공한 경우, 서버에서 내 포켓몬 정보를 가져온다.
			NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_POKEMON_URL, null, CallbackMyPokemonAfterPurchasePokemon);
		}
		else
		{
			DestroyLoadingCircle();
			CreateMsgBoxOnBtn("상점 구매 실패");
		}
	}

	//내 포켓몬 정보를 잘 가져왔는지 확인
	void CallbackMyPokemonAfterPurchasePokemon(bool result)
	{
		DestroyLoadingCircle();
		if(result)
		{
			CreateMsgBoxOnBtn("구매 완료");
			UpdateShopItems();
		}
		else
		{
			CreateMsgBoxOnBtn("상점 구매 후 포켓몬 로드 실패");
		}
	}

	//포켓몬 정보를 가져온 후 상점 상태를 업데이트 하는 함수
	void UpdateShopItems()
	{
		//모든 상점 데이터를 돌면서
		for(int i = 0; i <GameDataManager.Instance.pokemonShopList.Length; i++)
		{
			var shopItem = GameDataManager.Instance.pokemonShopList[i];

			//보유 여부를 체크하고
			bool isHave = false;
			if(GameDataManager.Instance.myPokemonIds != null && GameDataManager.Instance.myPokemonIds.Contains(shopItem.shop_id))
			{
				isHave = true;
			}

			//각 상점 데이터에 붙을 정보들을 재표기한다.
			GameObject itemObj = shopItemObjList[i];
			itemObj.transform.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();

			if (isHave)
			{
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "보유";
			}
			else
			{
				itemObj.transform.Find("Button/buyText").GetComponent<TMP_Text>().text = "구매";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PurchasePokemon(shopItem.shop_id));
			}
		}
	}


	//인벤토리 버튼이 눌렸을 때 실행될 함수
	void OnClickEnterInventory()
	{
		//아직 내 포켓몬 데이터가 없는 경우
		if (GameDataManager.Instance.myPokemonIds == null)
		{
			//데이터를 받아온다.
			NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_POKEMON_URL, null, CallbackCreateInventory);
		}
		else
		{
			//포켓몬 데이터가 있는 경우 인벤토리를 연다.
			CreateInventory();
		}
	}

	//처리 결과에 따라 실행될 함수
	void CallbackCreateInventory(bool result)
	{
		if(result)
		{
			CreateInventory();
		}
		else
		{
			CreateMsgBoxOnBtn("내 포켓몬 데이터 불러오기 실패");
		}
	}

	//인벤토리 창 열기
	void CreateInventory()
	{
		//인벤토로 오브젝트가 없으면 불러와서 저장한다.
		if (inventoryObj == null)
		{
			GameObject prefab = Resources.Load<GameObject>("prefabs/Inventory");
			inventoryObj = Instantiate(prefab, Canvas);
		}

		//버튼 이벤트 추가
		inventoryObj.transform.Find("closeBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(inventoryObj));

		//스프라이트 이미지를 모두 불러온다.
		Sprite[] spriteFrontAll = Resources.LoadAll<Sprite>("images/pokemon-front");
		//아이템 정보를 표시할 프리팹을 불러온다.
		GameObject itemPrefab = Resources.Load<GameObject>("prefabs/InventoryItem");
		//아이템 정보를 나타낼 위치를 정의한다.
		Transform content = inventoryObj.transform.Find("ScrollView/Viewport/Content");

		//우선 기존 아이템 정보들은 모두 삭제한다.
		foreach (Transform child in content)
		{
			Destroy(child);
		}
		//인벤토리 데이터도 초기화한다.
		inventoryList.Clear();

		//불러온 아이템 데이터들을 모두 돌면서
		for (int i = 0; i < GameDataManager.Instance.myPokemonList.Length; i++)
		{
			//보유 아이템 하나를 가져와서
			var inventoryItem = GameDataManager.Instance.myPokemonList[i];

			//해당 포켓몬이 현재 선택되어있는지 확인한다.
			bool picked = false;
			if(GameDataManager.Instance.pickedPokemon != null && GameDataManager.Instance.pickedPokemon.pokemonId == inventoryItem.pokemonId)
				picked = true;

			//아이템 프리팹을 content에 생성하고
			GameObject itemObj = Instantiate(itemPrefab, content);
			//해당 생성된 오브젝트에 관련 아이템 정보를 삽입한다.
			itemObj.transform.Find("Icon/IconImage").GetComponent<Image>().sprite = spriteFrontAll[inventoryItem.pokemonId - 1];
			itemObj.transform.Find("Title").GetComponent<TMP_Text>().text = inventoryItem.name;
			itemObj.transform.Find("Context").GetComponent<TMP_Text>().text
				= "hp : " + inventoryItem.hp.ToString() + " / damage : " + inventoryItem.skills[0].damage;

			//이미 선택된 아이템일 경우
			if (picked)
			{
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "사용중";
			}
			else
			{
				//선택되지 않은 아이템일 경우, 선택 관련 로직을 버튼에 삽입한다.
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "사용";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PickPokemon(inventoryItem));
			}
			inventoryList.Add(itemObj);
		}

	}

	//포켓몬을 변경하는 함수
	void PickPokemon(MyPokemon pickedPokemon)
	{
		Debug.Log("picked : " + pickedPokemon.name);
		//선택 포켓몬을 업데이트 하고
		GameDataManager.Instance.pickedPokemon = pickedPokemon;
		//관련 정보를 인벤토리 창에서도 업데이트 한다.
		UpdateInventoryItem();
	}

	//인벤토리 창을 업데이트 하는 함수
	void UpdateInventoryItem()
	{
		//불러온 보유 데이터들을 모두 돌면서
		for (int i = 0; i < GameDataManager.Instance.myPokemonList.Length; i++)
		{
			//보유 아이템 하나를 가져와서
			var inventoryItem = GameDataManager.Instance.myPokemonList[i];

			//해당 아이템의 선택 여부에 대해서 확인 후
			bool picked = false;
			if (GameDataManager.Instance.pickedPokemon != null && GameDataManager.Instance.pickedPokemon.pokemonId == inventoryItem.pokemonId)
				picked = true;

			//실제 해당 오브젝트를 리스트에서 꺼내와서
			GameObject itemObj = inventoryList[i];
			//삽입된 이벤트 함수를 삭제해주고
			itemObj.transform.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();

			//선택된 아이템일 경우
			if (picked)
			{
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "사용중";
			}
			else
			{
				//선택되지 않은 아이템이면 관련 이벤트 함수를 삽입해준다.
				itemObj.transform.Find("Button/useText").GetComponent<TMP_Text>().text = "사용";
				itemObj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => PickPokemon(inventoryItem));
			}
		}
	}

	//로그아웃 버튼을 누르면 실행될 함수
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
			CreateMsgBoxOnBtn("지갑 연동 성공", OnClickUpdateWallet);
		}
		else
		{
			CreateMsgBoxOnBtn("지갑 연동 실패");
		}
	}

	//지갑 갱신 함수
	void OnClickUpdateWallet()
	{
		NetworkManager.Instance.SendServerGet(CommonDefine.GET_MY_WALLET_URL, null, UpdateWallet);
	}

	//Balance 불러오기 성공 여부에 따라 분기 실행되는 콜백 함수
	void UpdateWallet(bool result)
	{
		//balance 불러오기 실패 하면 다음 로그 출력
		if (!result) Debug.Log("내 지갑 로드 실패");

		//잔액이 음수인 경우 제대로 불러와지지 않았다는 것을 뜻함
		if(GameDataManager.Instance.walletBalance < 0)
		{
			//다음을 표시
			lobbyObj.transform.Find("Wallet/balance").GetComponent<TMP_Text>().text = "지갑 연동 안됨.";
		}
		else
		{
			//제대로 불러와진 경우
			lobbyObj.transform.Find("Wallet/balance").GetComponent<TMP_Text>().text
				= "잔액 : " + GameDataManager.Instance.walletBalance.ToString("F2");
		}
	}

	//입금 함수
	void OnClickGrant()
	{
		CreateLodingCircle();
		//다음 body 데이터 설정
		WalletGetSetPostData data = new WalletGetSetPostData
		{
			amount = "100"
		};
		//post로 해당 데이터를 함께 전송
		NetworkManager.Instance.SendServerPost(CommonDefine.BLOCKCHAIN_GRANT_URL, data, CallbackGrant);
	}

	//출력 함수
	void OnClickDeduct()
	{
		CreateLodingCircle();
		//body 데이터 설정
		WalletGetSetPostData data = new WalletGetSetPostData
		{
			amount = "100"
		};
		//post로 해당 데이터를 서버로 함께 전송
		NetworkManager.Instance.SendServerPost(CommonDefine.BLOCKCHAIN_DEDUCT_URL, data, CallbackDeduct);
	}

	//Grant 성공 여부에 따라 분기 실행될 콜백 함수
	void CallbackGrant(bool result)
	{
		DestroyLoadingCircle();
		if (result)
		{
			//성공시 Balance 값을 업데이트
			CreateMsgBoxOnBtn("CallbackGrant 성공", OnClickUpdateWallet);
		}
		else
		{
			CreateMsgBoxOnBtn("CallbackGrant 실패");
		}
	}

	//Deduct 성공 여부에 따라 분기 실행될 콜백 함수
	void CallbackDeduct(bool result)
	{
		DestroyLoadingCircle();
		if (result)
		{
			//성공시 Balance 값을 업데이트
			CreateMsgBoxOnBtn("CallbackDeduct 성공", OnClickUpdateWallet);
		}
		else
		{
			CreateMsgBoxOnBtn("CallbackDeduct 실패");
		}
	}

	private void CreateMsgBoxOnBtn(string desc, Action checkResult = null)
	{
		//해당 프리팹을 생성하고
		GameObject msgBoxPrefabOnBtn = Resources.Load<GameObject>("prefabs/MessageBox_1Button");
		GameObject obj = Instantiate(msgBoxPrefabOnBtn, Canvas);

		//관련 버튼에 이벤트 함수 연결
		obj.transform.Find("desc").GetComponent<TMP_Text>().text = desc;
		//만약 action이 따로 정의되지 않은 경우, 해당 버튼 누르면 파괴되도록 설정
		obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => DestroyObject(obj));

		//매개변수로 넘어온 이벤트 함수가 존재하면, 해당 함수를 연결
		if (checkResult != null)
		{
			obj.transform.Find("CheckBtn").GetComponent<Button>().onClick.AddListener(() => checkResult());
		}
	}

	//로딩 씬으로 우선 이동 후, 비동기로 로비 씬 로딩 후 이동
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

	//이스터 에그에 사용될 변수
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
		//위 화살표가 3번 눌리면 특정 버튼을 활성화
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			upArrowCount++;
			if (upArrowCount >= 3)
			{
				lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(true);
				lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(true);
			}
		}
		
		//위 화살표 제외 다른 화살표가 눌리면, 특정 버튼 비활성화
		if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
		{
			upArrowCount = 0;
			lobbyObj.transform.Find("GrantBtn").gameObject.SetActive(false);
			lobbyObj.transform.Find("DeductBtn").gameObject.SetActive(false);
		}
	}
}

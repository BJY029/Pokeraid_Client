using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager>
{
	//SocketIO Ŭ���̾�Ʈ ��ü�� ������ client ���� ����
	private SocketIO client = null;
	protected override void Awake()
	{
		base.Awake();

		Debug.Log("NetworkManager Init");
	}

	//�α��� ������ ������ ������ �ڷ�ƾ �Լ��� �����ϴ� �Լ�
	public void SendLoginServer(string api, string id, string password, Action<bool> onResult)
	{
		Debug.Log(api);
		//���� ��� �ڷ�ƾ ����
		StartCoroutine(ServerLoginInCall(api, id, password, onResult));
	}

	//�α��� ���� �� ��û�� ������ ���� ó���ϴ� �ڷ�ƾ
	//api : ���(users/login), id, password : ����� �Է�, onResult : ���� ���θ� �񵿱� �ݹ����� ����
	IEnumerator ServerLoginInCall(string api, string id, string password, Action<bool> onResult)
	{
		//������ ����ȭ�� ����, ����� �Է��� �ش� ��ü �������� ����
		LoginPostData data = new LoginPostData
		{
			id = id,
			password = password
		};
		//��ü ���·� ����� �����͸� Json�� ����ȭ
		string json = JsonUtility.ToJson(data);

		//UnityWebReauest : Unity���� HTTP ��û(GET,Post ��)�� ������ �����κ��� ������ �ޱ� ���� ����ϴ� Ŭ����
		//URL�� ���� Method(post, get ��)�� ������ ��û ���� ����
		UnityWebRequest request = new UnityWebRequest(CommonDefine.WEB_BASE_URL + api, "POST");
		//json���� ����ȭ �� �����͸� ����Ʈ �迭�� ����(Body Raw)
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
		//Body Raw �����͸� request ���ڿ� ����� ������ ����
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		//������ ������ ������ �޸� ���� ����. .text, .data�� ���� �� �ֵ��� �Ѵ�.
		request.downloadHandler = new DownloadHandlerBuffer();
		//������ ���ε�� �����Ͱ� Json ������ �����Ͷ�� �� �� �ֵ��� ���ش�.
		request.SetRequestHeader("Content-Type", "application/json");

		//������ ������ �� ������ �񵿱�� ����Ѵ�.
		yield return request.SendWebRequest();

		//���� ������ ������ ���
		if(request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("���� : " + request.downloadHandler.text);
			//���� ���� ó���� ȣ��
			HandleResponse(api, request.downloadHandler.text);
			//�ݹ� �Լ��� �������� �ؼ� ȣ��
			onResult?.Invoke(true);
		}
		else
		{
			Debug.LogError("POST ���� : " + request.error);
			//�ݹ� �Լ��� ���з� �ؼ� ȣ��
			onResult?.Invoke(false);
		}
	}


	public void SendServerGet(string api, List<ServerPacket> packetList, Action<bool> onResult)
	{
		Debug.Log(api);
		StartCoroutine(ServerCallGet(api, packetList, onResult));
	}

	IEnumerator ServerCallGet(string api, List<ServerPacket> packetList, Action<bool> onResult)
	{
		//���� ���ڿ� ����
		string packetStr = "";
		if(packetList != null) //���޵� ������ �����ϸ�
		{
			//packetType=packetValue �������� ���ڿ��� ���� &�� ����
			for(int i = 0; i < packetList.Count; i++)
			{
				if(packetStr.Length > 0)
				{
					packetStr += "&";
				}
				ServerPacket packet = packetList[i];
				packetStr += packet.packetType + "=" + packet.packetValue;
			}
		}

		//��û URL ����
		string url = CommonDefine.WEB_BASE_URL + api;
		if(packetStr.Length > 0)
		{
			url += "?" + packetStr;
		}

		//Get ��û�� ���� ��
		UnityWebRequest request = UnityWebRequest.Get(url);
		//�ش� ��û�� header�� session id�� ����(Guard ���� ��)
		request.SetRequestHeader("authorization", GameDataManager.Instance.loginData.sessionId);
		//��û�� �����ϰ� ������ ���
		yield return request.SendWebRequest(); ;

		//���� ������ ������ ���
		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("���� : " + request.downloadHandler.text);
			//���� ���� ó���� ȣ��
			HandleResponse(api, request.downloadHandler.text);
			//�ݹ� �Լ��� �������� �ؼ� ȣ��
			onResult?.Invoke(true);
		}
		else
		{
			Debug.LogError("GET ���� : " + request.error);
			//�ݹ� �Լ��� ���з� �ؼ� ȣ��
			onResult?.Invoke(false);
		}
	}

	public void SendServerPost(string api, object packet, Action<bool> onResult)
	{
		StartCoroutine(ServerCallPost(api, packet, onResult));
	}

	IEnumerator ServerCallPost(string api, object packet, Action<bool> onResult)
	{
		//packet ������ Json���� �Ľ�
		string json = JsonUtility.ToJson(packet);
		//request ����
		UnityWebRequest request = new UnityWebRequest(CommonDefine.WEB_BASE_URL + api, "POST");
		//json���� ������ packet ������ body�� raw�� ��´�.
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
		//�ش� bodyRaw�� ������ ����
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		//������ ������ �޸� ���ۿ� �����Ͽ� .text/ .data �� ���� �� �ְ� �Ѵ�.
		request.downloadHandler = new DownloadHandlerBuffer();

		//��û ����� �ش� �����Ͱ� JSON ������ ���������� �˷��ش�.
		request.SetRequestHeader("Content-Type", "application/json");
		//Guard�� ���� session ��ȿ�� �˻縦 ���� ���� ������ ����� ���� ������.
		request.SetRequestHeader("authorization", GameDataManager.Instance.loginData.sessionId);

		//������ ��ȯ�� ������ ����Ѵ�.
		yield return request.SendWebRequest();

		//���� ������ ������ ���
		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("���� : " + request.downloadHandler.text);
			//���� ���� ó���� ȣ��
			HandleResponse(api, request.downloadHandler.text);
			//�ݹ� �Լ��� �������� �ؼ� ȣ��
			onResult?.Invoke(true);
		}
		else
		{
			Debug.LogError("Post ���� : " + request.error);
			//�ݹ� �Լ��� ���з� �ؼ� ȣ��
			onResult?.Invoke(false);
		}
	}


	//���� ó����
	private void HandleResponse(string api, string data)
	{
		if (string.IsNullOrEmpty(api) || string.IsNullOrEmpty(data))
			return;

		//api�� ���� �б�
		switch (api)
		{
			//�α��� api�� ���
			case CommonDefine.LOGIN_URL:
				{
					//�α��� ���� JSON �����͸� LoginData ��ü�� ��ȯ �� �����Ѵ�.
					//�α��� ��, sessionId ���� ���Ŀ� ����ؾ� �ϱ� ������ �ش� ���ڸ� loginData�� ���� �����Ѵ�.
					GameDataManager.Instance.loginData = JsonUtility.FromJson<LoginData>(data);
				}
				break;
			case CommonDefine.GET_MY_POKEMON_URL:
				{
					GameDataManager.Instance.myPokemonList = JsonHelper.FromJson<MyPokemon>(data);
					GameDataManager.Instance.myPokemonIds = new HashSet<int>(GameDataManager.Instance.myPokemonList.Select(p => p.pokemonId));
				}
				break;
			case CommonDefine.GET_MY_WALLET_URL:
				{
					WalletData wallet = JsonUtility.FromJson<WalletData>(data);
					GameDataManager.Instance.walletBalance = double.Parse(wallet.balance);
				}
				break;
			case CommonDefine.SHOP_LIST_URL:
				{
					GameDataManager.Instance.pokemonShopList = JsonHelper.FromJson<PokemonShop>(data);
				}
				break;
			case CommonDefine.ROOM_LIST_URL:
				{
					GameDataManager.Instance.roomList = JsonHelper.FromJson<Room>(data);
				}
				break;
		}
	}

	//�ش� �޼���� �񵿱� �޼����̴�.(async Task)
	//async ��� ��, ������ �Ϸ�� ������ ��ٸ��� ���� ������ ���ߴ� ������ ���� �� �ִ�.
	//Task�� �񵿱� �۾��� ���¸� ��Ÿ���� ��ü�̴�.
	//�Ű������� Action Ÿ���� ��������Ʈ �ݹ� �Լ��� �޴´�.
	public async Task ConnectSocket(Action<SocketIOResponse> OnRoomUpdate)
	{
		//Ŭ���̾�Ʈ ��ü�� ���ų�, ������� ���� ���¶��
		if(client == null || client.Connected == false)
		{
			//������ ������ ��û�� �� �Բ� ���� �߰� �����͸� ������ ���� ��ųʸ� ���·� �����Ѵ�.
			var payload = new Dictionary<string, string>()
			{
				{"sessionid", GameDataManager.Instance.loginData.sessionId },
			};

			//client ������ ���ο� SocketIO �ν��Ͻ��� �����Ͽ� �Ҵ��Ѵ�.
			client = new SocketIO(CommonDefine.WEB_SOCKET_URL, new SocketIOOptions
			{
				//���� ���ῡ ���� �پ��� �ɼ��� �����Ѵ�.
				//HTTP ����� ������ ���� payload �����͸� �߰��Ѵ�.
				ExtraHeaders = payload,
				//������ ����ġ�ʰ� ������ ��, �ڵ����� �翬���� �õ��ϵ��� ����
				Reconnection = true,
				//�ִ� 5�� ���� �翬�� �õ�
				ReconnectionAttempts = 5,
				//�翬�� �õ� ������ ������ 1000ms(1sec)�� ����
				ReconnectionDelay = 1000,
				//��� ���������� WebSocket���� ��������� �����Ѵ�.
				Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
			});

			//Ŭ���̾�Ʈ�� ������ ���������� ���� �Ǿ��� �� ���� �� �̺�Ʈ �ڵ鷯�� ����Ѵ�.
			client.OnConnected += OnConnected;
			//����� ���� �̺�Ʈ �ڵ鷯�� ����Ѵ�.
			//�������� SOCKET_ROOM_UPDATE��� �̸��� �̺�Ʈ�� emit�� ������, �Ű������� ���� �ݹ� �Լ��� ����ȴ�.
			client.On(CommonDefine.SOCKET_ROOM_UPDATE, OnRoomUpdate);
			
			//���� ������ �񵿱�� �����Ѵ�.
			await client.ConnectAsync();
		}
	}

	//������ ���������� ������ �Ϸ� �Ǿ��� �� ����� �Լ��̴�.
	private void OnConnected(object sender, EventArgs e)
	{
		Debug.Log("Connected to Socket.IO server");
		Debug.Log("Connected : " + client.Connected);
	}

	//�� ���� �̺�Ʈ�� �߼��ϴ� �Լ�
	public async void CreateRoom(Action<SocketIOResponse> OnRoomUpdate, int boosId, int pokemonId)
	{
		//���� ���� Ȯ��(������ �ȵǾ� ������ ����)
		await ConnectSocket(OnRoomUpdate);

		//�� ������ �ʿ��� body ������ ������ ���� ����
		var payload = new Dictionary<string, int>
		{
			{"boosId", boosId },
			{"myPoketmonId", pokemonId},
		};

		//�̺�Ʈ �߼�
		await client.EmitAsync(CommonDefine.SOCKET_CREATE_ROOM, payload);
	}

	//�� ���� �̺�Ʈ�� �߼��ϴ� �Լ�
	public async void JoinRoom(Action<SocketIOResponse> OnRoomUpdate, string roomId, int pokemonId)
	{
		//���� ���� Ȯ��(������ �ȵǾ� ������ ����)
		await ConnectSocket(OnRoomUpdate);

		//�� ������ �ʿ��� body ������ ������ ���� ����
		//string, int�� ȥ�յ� ��ü�̹Ƿ� object�� ���
		var payload = new Dictionary<string, object>
		{
			{"roomId", roomId},
			{"myPoketmonId", pokemonId}
		};
		//�̺�Ʈ �߼�
		await client.EmitAsync(CommonDefine.SOCKET_JOIN_ROOM, payload);
	}

	//�� ������ �̺�Ʈ�� �߼��ϴ� �Լ�
	public async void LeaveRoom(Action<SocketIOResponse> OnRoomUpdate, string roomId)
	{
		//���� ���� Ȯ��(������ �ȵǾ� ������ ����)
		await ConnectSocket(OnRoomUpdate);

		//�� �����⿡ �ʿ��� body ������ ������ ���� ����
		var payload = new Dictionary<string, string>
		{
			{"roomId", roomId},
		};
		//�̺�Ʈ �߼�
		await client.EmitAsync(CommonDefine.SOCKET_LEAVE_ROOM, payload);
	}

	//���� ������ ���� �񵿱� �Լ�
	public async Task DisconnectSocket()
	{
		if(client != null) 
			await client.DisconnectAsync();
	}

	//�ۿ��� ���� �� ���� ������ ���´�.
	async void OnApplicationQuit()
	{
		await DisconnectSocket();
	}

	//Unity�� JsonUtility�� Json �迭�� ���� �Ľ����� ���ϱ� ������, �迭�� �Ľ��Ϸ��� Wrapper ��ü�� ������� �Ѵ�.
	//���� JSON : [{}, {}, ...]
	//wrapper ���θ� : {"array": [{}, {}, ...] }
	public static class JsonHelper
	{
		public static T[] FromJson<T>(string json)
		{
			// JSON �迭�� array��� key�� ���μ� JsonUtility�� �Ľ��� �����ϵ��� ��ȯ���ְ�
			string newJson = "{ \"array\": " + json + "}";
			// �ش� ���ڿ��� T ������ ������ȭ �� ��
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
			// ����� ��ȯ
			return wrapper.array;
		}

		[Serializable]
		private class Wrapper<T>
		{
			public T[] array;
		}
	}
}



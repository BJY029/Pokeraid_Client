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
	//SocketIO 클라이언트 객체를 저장할 client 변수 선언
	private SocketIO client = null;
	protected override void Awake()
	{
		base.Awake();

		Debug.Log("NetworkManager Init");
	}

	//로그인 정보를 서버로 보내는 코루틴 함수를 실행하는 함수
	public void SendLoginServer(string api, string id, string password, Action<bool> onResult)
	{
		Debug.Log(api);
		//서버 통신 코루틴 실행
		StartCoroutine(ServerLoginInCall(api, id, password, onResult));
	}

	//로그인 관련 웹 요청을 보내고 응답 처리하는 코루틴
	//api : 경로(users/login), id, password : 사용자 입력, onResult : 성공 여부를 비동기 콜백으로 전달
	IEnumerator ServerLoginInCall(string api, string id, string password, Action<bool> onResult)
	{
		//데이터 직렬화를 위해, 사용자 입력을 해당 객체 형식으로 저장
		LoginPostData data = new LoginPostData
		{
			id = id,
			password = password
		};
		//객체 형태로 저장된 데이터를 Json을 직렬화
		string json = JsonUtility.ToJson(data);

		//UnityWebReauest : Unity에서 HTTP 요청(GET,Post 등)을 보내고 서버로부터 응답을 받기 위해 사용하는 클래스
		//URL과 실행 Method(post, get 등)을 저장한 요청 인자 생성
		UnityWebRequest request = new UnityWebRequest(CommonDefine.WEB_BASE_URL + api, "POST");
		//json으로 직렬화 한 데이터를 바이트 배열로 넣음(Body Raw)
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
		//Body Raw 데이터를 request 인자에 저장된 서버로 전송
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		//서버의 응답을 저장할 메모리 버퍼 생성. .text, .data로 읽을 수 있도록 한다.
		request.downloadHandler = new DownloadHandlerBuffer();
		//서버로 업로든된 데이터가 Json 형식의 데이터라고 알 수 있도록 해준다.
		request.SetRequestHeader("Content-Type", "application/json");

		//서버가 응답을 할 때까지 비동기로 대기한다.
		yield return request.SendWebRequest();

		//서버 응답이 성공한 경우
		if(request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("응답 : " + request.downloadHandler.text);
			//응답 본문 처리기 호출
			HandleResponse(api, request.downloadHandler.text);
			//콜백 함수를 성공으로 해서 호출
			onResult?.Invoke(true);
		}
		else
		{
			Debug.LogError("POST 실패 : " + request.error);
			//콜백 함수를 실패로 해서 호출
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
		//쿼리 문자열 생성
		string packetStr = "";
		if(packetList != null) //전달된 쿼리가 존재하면
		{
			//packetType=packetValue 형식으로 문자열을 만들어서 &로 연결
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

		//요청 URL 구성
		string url = CommonDefine.WEB_BASE_URL + api;
		if(packetStr.Length > 0)
		{
			url += "?" + packetStr;
		}

		//Get 요청을 생성 후
		UnityWebRequest request = UnityWebRequest.Get(url);
		//해당 요청의 header에 session id를 설정(Guard 인증 용)
		request.SetRequestHeader("authorization", GameDataManager.Instance.loginData.sessionId);
		//요청을 전송하고 응답을 대기
		yield return request.SendWebRequest(); ;

		//서버 응답이 성공한 경우
		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("응답 : " + request.downloadHandler.text);
			//응답 본문 처리기 호출
			HandleResponse(api, request.downloadHandler.text);
			//콜백 함수를 성공으로 해서 호출
			onResult?.Invoke(true);
		}
		else
		{
			Debug.LogError("GET 실패 : " + request.error);
			//콜백 함수를 실패로 해서 호출
			onResult?.Invoke(false);
		}
	}

	public void SendServerPost(string api, object packet, Action<bool> onResult)
	{
		StartCoroutine(ServerCallPost(api, packet, onResult));
	}

	IEnumerator ServerCallPost(string api, object packet, Action<bool> onResult)
	{
		//packet 정보를 Json으로 파싱
		string json = JsonUtility.ToJson(packet);
		//request 정의
		UnityWebRequest request = new UnityWebRequest(CommonDefine.WEB_BASE_URL + api, "POST");
		//json으로 변경한 packet 정보를 body에 raw로 담는다.
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
		//해당 bodyRaw를 서버로 전송
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		//서버의 응답을 메모리 버퍼에 저장하여 .text/ .data 로 읽을 수 있게 한다.
		request.downloadHandler = new DownloadHandlerBuffer();

		//요청 헤더에 해당 데이터가 JSON 형식의 데이터임을 알려준다.
		request.SetRequestHeader("Content-Type", "application/json");
		//Guard에 사용될 session 유효성 검사를 위해 다음 정보를 헤더에 같이 보낸다.
		request.SetRequestHeader("authorization", GameDataManager.Instance.loginData.sessionId);

		//서버가 반환할 때까지 대기한다.
		yield return request.SendWebRequest();

		//서버 응답이 성공한 경우
		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("응답 : " + request.downloadHandler.text);
			//응답 본문 처리기 호출
			HandleResponse(api, request.downloadHandler.text);
			//콜백 함수를 성공으로 해서 호출
			onResult?.Invoke(true);
		}
		else
		{
			Debug.LogError("Post 실패 : " + request.error);
			//콜백 함수를 실패로 해서 호출
			onResult?.Invoke(false);
		}
	}


	//본문 처리기
	private void HandleResponse(string api, string data)
	{
		if (string.IsNullOrEmpty(api) || string.IsNullOrEmpty(data))
			return;

		//api에 따라서 분기
		switch (api)
		{
			//로그인 api인 경우
			case CommonDefine.LOGIN_URL:
				{
					//로그인 응답 JSON 데이터를 LoginData 객체로 변환 후 저장한다.
					//로그인 후, sessionId 값을 추후에 사용해야 하기 때문에 해당 인자를 loginData에 따로 저장한다.
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

	//해당 메서드는 비동기 메서드이다.(async Task)
	//async 사용 시, 연결이 완료될 때까지 기다리는 동안 게임이 멈추는 현상을 막을 수 있다.
	//Task는 비동기 작업의 상태를 나타내는 객체이다.
	//매개변수로 Action 타입의 델리게이트 콜백 함수를 받는다.
	public async Task ConnectSocket(Action<SocketIOResponse> OnRoomUpdate)
	{
		//클라이언트 객체가 없거나, 연결되지 않은 상태라면
		if(client == null || client.Connected == false)
		{
			//서버에 연결을 요청할 때 함께 보낼 추가 데이터를 다음과 같이 딕셔너리 형태로 생성한다.
			var payload = new Dictionary<string, string>()
			{
				{"sessionid", GameDataManager.Instance.loginData.sessionId },
			};

			//client 변수에 새로운 SocketIO 인스턴스를 생성하여 할당한다.
			client = new SocketIO(CommonDefine.WEB_SOCKET_URL, new SocketIOOptions
			{
				//소켓 연결에 대한 다양한 옵션을 설정한다.
				//HTTP 헤더에 위에서 만든 payload 데이터를 추가한다.
				ExtraHeaders = payload,
				//연결이 예기치않게 끊겼을 때, 자동으로 재연결을 시도하도록 설정
				Reconnection = true,
				//최대 5번 까지 재연결 시도
				ReconnectionAttempts = 5,
				//재연결 시도 사이의 간격을 1000ms(1sec)로 설정
				ReconnectionDelay = 1000,
				//통신 프로토콜을 WebSocket으로 명시적으로 지정한다.
				Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
			});

			//클라이언트가 서버에 성공적으로 연결 되었을 때 실행 될 이벤트 핸들러를 등록한다.
			client.OnConnected += OnConnected;
			//사용자 정의 이벤트 핸들러를 등록한다.
			//서버에서 SOCKET_ROOM_UPDATE라는 이름의 이벤트가 emit을 보내면, 매개변수로 받은 콜백 함수가 실행된다.
			client.On(CommonDefine.SOCKET_ROOM_UPDATE, OnRoomUpdate);
			
			//실제 서버를 비동기로 연결한다.
			await client.ConnectAsync();
		}
	}

	//서버에 성공적으로 연결이 완료 되었을 때 실행될 함수이다.
	private void OnConnected(object sender, EventArgs e)
	{
		Debug.Log("Connected to Socket.IO server");
		Debug.Log("Connected : " + client.Connected);
	}

	//방 생성 이벤트를 발송하는 함수
	public async void CreateRoom(Action<SocketIOResponse> OnRoomUpdate, int boosId, int pokemonId)
	{
		//소켓 연결 확인(연결이 안되어 있으면 연결)
		await ConnectSocket(OnRoomUpdate);

		//방 생성에 필요한 body 정보를 다음과 같이 생성
		var payload = new Dictionary<string, int>
		{
			{"boosId", boosId },
			{"myPoketmonId", pokemonId},
		};

		//이벤트 발송
		await client.EmitAsync(CommonDefine.SOCKET_CREATE_ROOM, payload);
	}

	//방 참가 이벤트를 발송하는 함수
	public async void JoinRoom(Action<SocketIOResponse> OnRoomUpdate, string roomId, int pokemonId)
	{
		//소켓 연결 확인(연결이 안되어 있으면 연결)
		await ConnectSocket(OnRoomUpdate);

		//방 참가에 필요한 body 정보를 다음과 같이 생성
		//string, int가 혼합된 객체이므로 object형 사용
		var payload = new Dictionary<string, object>
		{
			{"roomId", roomId},
			{"myPoketmonId", pokemonId}
		};
		//이벤트 발송
		await client.EmitAsync(CommonDefine.SOCKET_JOIN_ROOM, payload);
	}

	//방 떠나기 이벤트를 발송하는 함수
	public async void LeaveRoom(Action<SocketIOResponse> OnRoomUpdate, string roomId)
	{
		//소켓 연결 확인(연결이 안되어 있으면 연결)
		await ConnectSocket(OnRoomUpdate);

		//방 떠나기에 필요한 body 정보를 다음과 같이 생성
		var payload = new Dictionary<string, string>
		{
			{"roomId", roomId},
		};
		//이벤트 발송
		await client.EmitAsync(CommonDefine.SOCKET_LEAVE_ROOM, payload);
	}

	//소켓 연결을 끊는 비동기 함수
	public async Task DisconnectSocket()
	{
		if(client != null) 
			await client.DisconnectAsync();
	}

	//앱에서 나갈 때 소켓 연결을 끊는다.
	async void OnApplicationQuit()
	{
		await DisconnectSocket();
	}

	//Unity의 JsonUtility는 Json 배열을 직접 파싱하지 못하기 때문에, 배열을 파싱하려면 Wrapper 객체로 감싸줘야 한다.
	//원래 JSON : [{}, {}, ...]
	//wrapper 감싸면 : {"array": [{}, {}, ...] }
	public static class JsonHelper
	{
		public static T[] FromJson<T>(string json)
		{
			// JSON 배열을 array라는 key로 감싸서 JsonUtility가 파싱이 가능하도록 변환해주고
			string newJson = "{ \"array\": " + json + "}";
			// 해당 문자열을 T 형으로 역직렬화 한 뒤
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
			// 결과를 반환
			return wrapper.array;
		}

		[Serializable]
		private class Wrapper<T>
		{
			public T[] array;
		}
	}
}



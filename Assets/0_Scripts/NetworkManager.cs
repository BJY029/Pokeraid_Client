using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager>
{
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
		}
	}
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

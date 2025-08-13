using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

//웹 API 주소를 모아둔 상수 정의 객체
public class CommonDefine
{
    public const string WEB_BASE_URL = "http://127.0.0.1:3000/";

    public const string REGISTER_URL = "users/register";
    public const string LOGIN_URL = "users/login";
    public const string GET_MY_POKEMON_URL = "users/pokemons";
    
    public const string LINK_WALLET_URL = "users/wallet/link";
    public const string GET_MY_WALLET_URL = "blockchain/balance";
    public const string BLOCKCHAIN_GRANT_URL = "blockchain/grant";
    public const string BLOCKCHAIN_DEDUCT_URL = "blockchain/deduct";

    public const string SHOP_LIST_URL = "shop/items";
	public const string SHOP_PURCHASE_URL = "shop/purchase";

	public const string LOADING_SCENE = "LoadingScene";
    public const string GAME_SCENE = "GameScene";
    public const string LOGIN_SCENE = "SampleScene";
}

//로그인 할 때 전송할 데이터 객체
//로그인 시 서버에 보낼 JSON Body 데이터를 표현하는 클래스
public class LoginPostData
{
    public string id;
    public string password;
}

//로그인 후 반환받을 데이터 객체
//로그인 요청 후 서버가 응답으로 반환하는 데이터(JSON)를 역직렬화해서 담을 클래스
public class LoginData
{
    public string sessionId;
    public string id;
}

public class ServerPacket
{
    public string packetType;
    public string packetValue;
}

[System.Serializable]
public class Pokemon
{
    public int id;
    public string name;
    public string hp;
    public List<PokemonSkill> skills;
}

[System.Serializable]
public class MyPokemon
{
	public int pokemonId;
	public string name;
	public string hp;
	public List<PokemonSkill> skills;
}


[System.Serializable]
public class PokemonSkill
{
    public int pokemon_id;
    public int skill_id;
    public string name;
    public string type;
    public string target;
    public int damage;
    public string pp;
}

[System.Serializable]
public class PokemonShop
{
    public int shop_id;
    public int price;
    public int stock;
    public Pokemon pokemon;
}

public class WalletData
{
    public string balance;
}

public class WalletGetSetPostData
{
    public string amount;
}

public class LinkWalletPostData
{
    public string privateKey;
}

public class PurchasePostData
{
    public int itemId;
}
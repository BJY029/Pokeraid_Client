using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : Singleton<GameDataManager> {
	protected void Awake()
	{
		base.Awake();
		Debug.Log("GameDataManager Init");
	}

	//로그인 데이터를 저장하기 위한 선언
	public LoginData loginData = null;

	public MyPokemon pickedPokemon = null;
	//플레이어의 포켓몬 정보를 저장할 배열
	public MyPokemon[] myPokemonList = null;
	//각 포켓몬들의 스킬들을 저장할 HashSet
	//HashSet는 중복을 허용하지 않는 집합 자료구조이다.
		//Hashtable 기반으로 구현되어 있으므로 빠른 검색이 가능하다.
	public HashSet<int> myPokemonIds = null;

	public PokemonShop[] pokemonShopList = null;

	public double walletBalance = -1;

	public string nextScene = "";

	public void ResetData()
	{
		loginData = null;
		myPokemonList = null;
		myPokemonIds = null;
		pokemonShopList = null;

		pickedPokemon = null;

		walletBalance = -1;
	}
}

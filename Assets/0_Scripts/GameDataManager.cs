using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : Singleton<GameDataManager> {
	protected void Awake()
	{
		base.Awake();
		Debug.Log("GameDataManager Init");
	}

	//�α��� �����͸� �����ϱ� ���� ����
	public LoginData loginData = null;

	public MyPokemon pickedPokemon = null;
	//�÷��̾��� ���ϸ� ������ ������ �迭
	public MyPokemon[] myPokemonList = null;
	//�� ���ϸ���� ��ų���� ������ HashSet
	//HashSet�� �ߺ��� ������� �ʴ� ���� �ڷᱸ���̴�.
		//Hashtable ������� �����Ǿ� �����Ƿ� ���� �˻��� �����ϴ�.
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

using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Loading : MonoBehaviour
{
	//�ε� UI�� ��� ĵ����
	public Transform canvas;

	//�ε��� ���� �ؽ�Ʈ
	Slider loadingBar;
	TMP_Text loadingBarText;
	TMP_Text loadingText;

	private void Start()
	{
		Init();
	}

	//�ε��� UI �ʱ�ȭ �Լ�
	void Init()
	{
		canvas = GameObject.Find("Canvas").transform;

		GameObject prefab = Resources.Load<GameObject>("prefabs/Loading");
		GameObject obj = Instantiate(prefab, canvas);

		loadingBar = obj.transform.Find("LoadingBar").GetComponent<Slider>();
		loadingBarText = obj.transform.Find("LoadingBar/LoadingText").GetComponent<TMP_Text>();
		loadingText = obj.transform.Find("LoadingText").GetComponent<TMP_Text>();

		loadingBar.value = 0f;
		loadingBar.maxValue = 1.0f;
		StartCoroutine(LoadScene());
		//StartCoroutine(LoadSceneTime(3.0f);
		StartCoroutine(LoadingText(0.3f, 3));
	}

	//�ε� �� ����� �ؽ�Ʈ �ڷ�ƾ
	IEnumerator LoadingText(float duration, int maxDotCount)
	{
		int curDot = 0;
		//���� ����Ǳ� ������ ���� �ݺ�
		while (true)
		{
			string dot = "";
			//���� ������ �� ������ŭ dot ����
			for(int i = 0; i < curDot; i++)
			{
				dot += ".";
			}
			loadingText.text = "Loading" + dot;
			++curDot;
			//���� ���� dot ���� ������ �� �������� ������ 0���� �ʱ�ȭ
			if(curDot > maxDotCount) curDot = 0;

			yield return new WaitForSeconds(duration);
		}
	}


	//���� �񵿱�� �ε��ϴ� �ڷ�ƾ
	IEnumerator LoadScene()
	{
		yield return null;
		//�񵿱�� ���� �ε�
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameDataManager.Instance.nextScene);
		//�ش� ���� �ε��� �Ϸ�Ǿ �� ��ȯ�� �����Ѵ�.
		asyncOperation.allowSceneActivation = false;
		//������ Ÿ�̸�
		float timer = 0.0f;

		//�� �ε��� ������ �� ���� �ݺ� ����
		while(!asyncOperation.isDone)
		{
			yield return null;
			//���� ���൵ ���� Ÿ�̸�
			timer += Time.deltaTime;
			//0.9 �� ������
			if(asyncOperation.progress < 0.9f)
			{
				//���� �ε��� ���� ���� ���൵���� ���� ����
				SetLoadingBar(Mathf.Lerp(loadingBar.value, asyncOperation.progress, timer));
				//�ε��� ���� ���� ���൵ ���� ��������, ���� �ܰ� ������ ���� �ʱ�ȭ
				if(loadingBar.value >= asyncOperation.progress) timer = 0.0f;
			}
			else
			{
				//���൵�� 0.9 �̻��� ���
				//�ε��ٸ� 1 ���� �����ϰ�
				SetLoadingBar(Mathf.Lerp(loadingBar.value, 1f, timer));
				//�ε��ٰ� 1�� �����ϸ�
				if(loadingBar.value == 1.0f)
				{
					//�� ��ȯ
					asyncOperation.allowSceneActivation = true;
					yield break;
				}
			}
		}

	}

	//�׽�Ʈ�� �ε� �� �ڷ�ƾ
	//Ư�� �ð���ŭ ���������� �ε��� �����ϰ� �� ����
	IEnumerator LoadSceneTime(float duration)
	{
		float time = 0f;
		float startValue = 0f;
		float endValue = loadingBar.maxValue;

		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameDataManager.Instance.nextScene);
		asyncOperation.allowSceneActivation = false;

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;
			SetLoadingBar(Mathf.Lerp(startValue, endValue, t));
			yield return null;
		}

		loadingBar.value = endValue;
		asyncOperation.allowSceneActivation = true;
	}

	//Ư�� ������ �ε��ٸ� �����ϴ� �Լ�
	void SetLoadingBar(float cur)
	{
		loadingBar.value = cur;
		//�Ҽ��� ���ڸ� ���� ǥ��
		loadingBarText.text = (cur * 100).ToString("F1") + " / 100.0";
	}

}

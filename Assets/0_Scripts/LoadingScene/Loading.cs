using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Loading : MonoBehaviour
{
	//로딩 UI를 띄울 캔버스
	public Transform canvas;

	//로딩바 관련 텍스트
	Slider loadingBar;
	TMP_Text loadingBarText;
	TMP_Text loadingText;

	private void Start()
	{
		Init();
	}

	//로딩바 UI 초기화 함수
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

	//로딩 시 재생할 텍스트 코루틴
	IEnumerator LoadingText(float duration, int maxDotCount)
	{
		int curDot = 0;
		//씬이 변경되기 전까지 무한 반복
		while (true)
		{
			string dot = "";
			//현재 설정된 점 개수만큼 dot 설정
			for(int i = 0; i < curDot; i++)
			{
				dot += ".";
			}
			loadingText.text = "Loading" + dot;
			++curDot;
			//만약 다음 dot 수가 설정한 점 개수보다 많으면 0으로 초기화
			if(curDot > maxDotCount) curDot = 0;

			yield return new WaitForSeconds(duration);
		}
	}


	//씬을 비동기로 로딩하는 코루틴
	IEnumerator LoadScene()
	{
		yield return null;
		//비동기로 씬을 로딩
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameDataManager.Instance.nextScene);
		//해당 씬이 로딩이 완료되어도 씬 전환은 보류한다.
		asyncOperation.allowSceneActivation = false;
		//보간용 타이머
		float timer = 0.0f;

		//씬 로딩이 끝나기 전 까지 반복 실행
		while(!asyncOperation.isDone)
		{
			yield return null;
			//현재 진행도 계산용 타이머
			timer += Time.deltaTime;
			//0.9 전 까지는
			if(asyncOperation.progress < 0.9f)
			{
				//현재 로딩바 값과 실제 진행도간의 보간 진행
				SetLoadingBar(Mathf.Lerp(loadingBar.value, asyncOperation.progress, timer));
				//로딩바 값이 실제 진행도 값과 같아지면, 다음 단계 보간을 위해 초기화
				if(loadingBar.value >= asyncOperation.progress) timer = 0.0f;
			}
			else
			{
				//진행도가 0.9 이상인 경우
				//로딩바를 1 까지 보간하고
				SetLoadingBar(Mathf.Lerp(loadingBar.value, 1f, timer));
				//로딩바가 1에 도달하면
				if(loadingBar.value == 1.0f)
				{
					//씬 전환
					asyncOperation.allowSceneActivation = true;
					yield break;
				}
			}
		}

	}

	//테스트용 로딩 씬 코루틴
	//특정 시간만큼 인위적으로 로딩을 수행하고 씬 변경
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

	//특정 값으로 로딩바를 설정하는 함수
	void SetLoadingBar(float cur)
	{
		loadingBar.value = cur;
		//소수점 한자리 까지 표기
		loadingBarText.text = (cur * 100).ToString("F1") + " / 100.0";
	}

}

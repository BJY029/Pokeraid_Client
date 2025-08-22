using UnityEngine;

//Where T : MonoBehaviour => 타입 T는 반드시 MonoBehaviour이어야 한다는 제약조건
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    //싱글톤 인스턴스를 저장하는 정적 변수
    private static T _instance;
    //멀티스레딩 상황에서도 싱글톤이 한 번만 생성되도록 보장하는 락 오브젝트
    private static readonly object _lock = new object();

	//Instance 프로퍼티, 싱글톤 접근점
	public static T Instance
	{
		get
		{
			//Mutual Exclution 구현
			lock (_lock)
			{
				if (_instance == null)
				{
					//현재 씬에 존재하는 T 타입의 오브젝트를 찾아 반환
					_instance = FindFirstObjectByType<T>();

					//만약 없을 경우, 해당 타입의 이름으로 오브젝트를 하나 생성 후, T 타입의 컴포넌트를 붙여 인스턴스 화
					if (_instance == null)
					{
						GameObject obj = new GameObject(typeof(T).Name);
						_instance = obj.AddComponent<T>();
					}

					//씬 전환시에도 제거되지 않도록 설정
					DontDestroyOnLoad(_instance.gameObject);
				}

				return _instance;
			}
		}
	}

	//인스턴스 중복 방지
	protected virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if(_instance != this)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

//Where T : MonoBehaviour => Ÿ�� T�� �ݵ�� MonoBehaviour�̾�� �Ѵٴ� ��������
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    //�̱��� �ν��Ͻ��� �����ϴ� ���� ����
    private static T _instance;
    //��Ƽ������ ��Ȳ������ �̱����� �� ���� �����ǵ��� �����ϴ� �� ������Ʈ
    private static readonly object _lock = new object();

	//Instance ������Ƽ, �̱��� ������
	public static T Instance
	{
		get
		{
			//Mutual Exclution ����
			lock (_lock)
			{
				if (_instance == null)
				{
					//���� ���� �����ϴ� T Ÿ���� ������Ʈ�� ã�� ��ȯ
					_instance = FindFirstObjectByType<T>();

					//���� ���� ���, �ش� Ÿ���� �̸����� ������Ʈ�� �ϳ� ���� ��, T Ÿ���� ������Ʈ�� �ٿ� �ν��Ͻ� ȭ
					if (_instance == null)
					{
						GameObject obj = new GameObject(typeof(T).Name);
						_instance = obj.AddComponent<T>();
					}

					//�� ��ȯ�ÿ��� ���ŵ��� �ʵ��� ����
					DontDestroyOnLoad(_instance.gameObject);
				}

				return _instance;
			}
		}
	}

	//�ν��Ͻ� �ߺ� ����
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

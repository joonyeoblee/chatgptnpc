using System.Collections;
using System.Numerics;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class TaskTest : MonoBehaviour
{
	// '비동기': '동기'의 반대말로 어떤 '작업'을 실행할 때 그 작업이 완료되지 않아도
	//           다음 코드를 실행하는 방식
	// 그 '작업'의 특징: 시간이 오래걸린다. (ex. 연산량이 많거나, IO 작업 등)

	public GameObject PatrolObject;
    
	private async void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("Space");
            
			// 1. 동기
			//LongLoop();
            
			// 2. 비동기(Coroutine)
			//StartCoroutine(LongLoop_Coroutine());
            
			// 3. 비동기(Task)
			// 3-1. 반환 값이 없는 Task
			// Task task1 = new Task(LongLoop);
			// task1.Start();
            
			// 3-2. 반환 값이 있는 Task
			Task<int> task2 = new Task<int>(LongLoop2);
			// task2.Start();

			// task2.Wait(); // task2 작업이 끝나기를 기다린다. (비동기를 강제로 동기로 만드므로 사용 X)
			// var result = task2.Result;
			// Debug.Log(result);
			// task2.ContinueWith((t) =>
			// {
			// 	int result = t.Result;
			// 	Debug.Log(result);
			// });
			
			// 비동기를 동기처럼 이해하기 쉽게 만드는 키워드가 async + await
			// int result = await task2;
			// Debug.Log(result);
			
			// 3-3 UniTask
			// UniTask를 사용하여 백그라운드에서 LongLoop2 실행
			int result = await UniTask.Run(LongLoop2);
			Debug.Log(result);
		}
	}
 

	// 연산량이 많은 작업
	private void LongLoop()
	{
		long sum = 1;
		for (long i = 1; i < 10000000000; ++i)
		{
			sum *= i;
		}
		Debug.Log("작업 완료");
		// 동기방식, Coroutine방식을 써도 사라지지만
		// Task를 이용한 호출에서 아래 MonoBehaviour를 상속받는 코드는 실행이 될 수도 안될수도 있다.
		PatrolObject.SetActive(false);
	}
	private int LongLoop2()
	{
		long sum = 1;
		for (long i = 1; i < 10000000000; ++i)
		{
			sum *= i;
		}

		Debug.Log("작업완료2");
		return 32413;
	}
    
	private IEnumerator LongLoop_Coroutine()
	{
		BigInteger sum = 1;
		for (long i = 1; i < 10000000000; ++i) // 
		{
			sum += i;
			// 한 프레임당 1000번만 계산하게 적어둠
			// 시간이 오래걸리는건 비동기로 해야한다.
			if (i % 1000000 == 0)
			{
				yield return null;
			}
		}
		Debug.Log("작업 완료");

	}
}

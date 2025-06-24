using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebTest : MonoBehaviour
{
	public Text MyTextUI;
	private void Start() {
		StartCoroutine(GetText());
	}
 
	IEnumerator GetText()
	{
		string url = "https://openapi.naver.com/v1/search/news.json?query=한화이글스?display=30";
		UnityWebRequest www = UnityWebRequest.Get(url);
		www.SetRequestHeader("X-Naver-Client-Id", "nCGjaCrS9GCiJdGzZCT5");
		www.SetRequestHeader("X-Naver-Client-Secret", "kdfQQUyJhN");
		yield return www.SendWebRequest();
 
		if(www.isNetworkError || www.isHttpError) {
			Debug.Log(www.error);
		}
		else {
			// Show results as text
			MyTextUI.text = www.downloadHandler.text;
		}
	}
}

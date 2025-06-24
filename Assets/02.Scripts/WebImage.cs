using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebImage : MonoBehaviour
{
	public RawImage MyImage;
	private void Start() {
		StartCoroutine(GetTexture());
	}
 
	IEnumerator GetTexture() {
		UnityWebRequest www = UnityWebRequestTexture.GetTexture("https://resources.chimhaha.net/article/1687956178681-nuqt3g1u3ge.jpg");
		yield return www.SendWebRequest();

		if(www.isNetworkError || www.isHttpError) {
			Debug.Log(www.error);
		}
		else {
			Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
			MyImage.texture = myTexture;
		}
	}
	
	
}

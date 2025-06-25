using OpenAI.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPrefab : MonoBehaviour
{

	public TextMeshProUGUI textUI;
	public Image MyImage;
	public TextMeshProUGUI text2UI;
	public TextMeshProUGUI text3UI;
	public void SetText(NpcResponse npcResponse, Choice choice)
	{
		textUI.text = npcResponse.ReplyMessage;
		text2UI.text = npcResponse.Appearance;
		text3UI.text = $"상태 : {npcResponse.Emotion}";
	}
	public void SetImage(Sprite sprite)
	{
		MyImage.sprite = sprite;
	}
	
	

}

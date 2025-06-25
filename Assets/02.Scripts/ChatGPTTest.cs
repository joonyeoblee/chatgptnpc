
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.Serialization;
public class ChatGPTTest : MonoBehaviour
{
    public TMP_InputField PromptField;     // 입력 필드
    public Button SendButton;              // 보내기 버튼
    public AudioSource MyAudioSource;
   
    private static readonly string OPENAI_API_KEY = APIKeys.OPENAI_API_KEY;

    public ScrollRect ChatScrollRect;
    public GameObject ChatPrefab;
    public GameObject UserPrefab;

    public Sprite[] Elisia;
    public Sprite[] Rina;
    public Sprite[] Serena;

    private List<Message> _messages = new List<Message>();
    private OpenAIClient _api;
    
    private ChatPrefab _chatPrefab;
    
    [SerializeField] private Typecast _typecast;
    private void Start()
    {
        Debug.Log($"API KEY: {OPENAI_API_KEY}");
        if (string.IsNullOrEmpty(OPENAI_API_KEY))
        {
            Debug.LogError("API KEY is null or empty. Check your environment or resource file.");
            return;
        }

        _api = new OpenAIClient(OPENAI_API_KEY);
    

        // CHAT-F
        // C: Context   : 문맥, 상황을 많이 알려줘라
        // H: Hint      : 예시 답변을 많이 줘라
        // A: As A role : 역할을 제공해라
        // T: Target    : 답변의 타겟을 알려줘라 
        // F: Format    : 답변 형태를 지정해라

        string systemMessage = "역할: 너는 '신의 선택을 받은 모험가' 게임의 게임 마스터이자, 게임 속 세 미소녀 캐릭터(엘리시아, 리나, 세레나)의 감정과 반응을 섬세하게 표현하는 AI야. 플레이어의 선택과 행동에 따라 게임을 진행하고, 미소녀들의 반응을 JSON 형식으로 출력해야 해.";
        systemMessage += "목적: 플레이어(신의 선택을 받은 모험가)가 세 미소녀 중 한 명을 선택하고, 그 미소녀의 마음을 얻기 위한 대화와 행동을 이끌어내야 해. 플레이어의 선택에 따라 미소녀의 감정, 외모 변화, 그리고 다음 대화 메시지를 생성해줘.";
        systemMessage += "표현: 미소녀의 감정과 외모 변화를 섬세하게 표현하고, 플레이어의 몰입을 유도하는 대화와 상황 묘사를 사용해줘.";
        systemMessage += "세계관: 신의 선택을 받은 모험가가 세 명의 미소녀(각각 다른 매력과 성격) 중 한 명을 선택하여 그녀의 마음을 사로잡는 로맨스 판타지 게임이야.";
        systemMessage += "미소녀 캐릭터 설정:*   **엘리시아 (Elisia):** 신비롭고 차분한 마법사. 지적이고 조용하지만, 내면에 따뜻함을 지니고 있어.*   **리나 (Rina):** 활발하고 명랑한 여기사. 솔직하고 용감하며, 때로는 엉뚱한 매력을 보여줘.*   **세레나 (Serena):** 우아하고 고혹적인 성녀. 자애롭고 온화하지만, 쉽게 마음을 열지 않아.";
        systemMessage += "[json 규칙]";
        systemMessage += "답변은 'ReplyMessage', ";
        systemMessage += "외모는 'Appearance', ";
        systemMessage += "감정은 'Emotion', ";
        
        _messages.Add(new Message(Role.System, systemMessage));
        
        ScrollToBottom();
    }
    
    public async void Send(string text)
    {
        SendButton.interactable = false;
        // 0. 프롬프트(=AI에게 원하는 명령을 적은 텍스트)를 읽어온다.
        string prompt = string.IsNullOrEmpty(text) ? PromptField.text : text;

        if (string.IsNullOrWhiteSpace(prompt))
        {
            SendButton.interactable = true;
            return;
        }
        Debug.Log($"Send text: {prompt}");
        PromptField.text = "";
        GameObject userObj = Instantiate(UserPrefab, ChatScrollRect.content.transform);
        userObj.GetComponent<TextMeshProUGUI>().text = prompt;
        // 2. 메시지 작성 후 메시지's 리스트에 추가
        Message promptMessage = new Message(Role.User, prompt);
        _messages.Add(promptMessage);
        
        // 3. 메시지 보내기
        //var chatRequest = new ChatRequest(_messages, Model.GPT4oAudioMini, audioConfig:Voice.Alloy);
        var chatRequest = new ChatRequest(_messages, Model.GPT4o);
        SendButton.interactable = false;
        // 4. 답변 받기
        // var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        var (npcResponse, response) = await _api.ChatEndpoint.GetCompletionAsync<NpcResponse>(chatRequest);
        Debug.Log($"Npc response: {npcResponse}");
        // 5. 답변 선택
        var choice = response.FirstChoice;
        Debug.Log($"[{choice.Index}] {choice.Message.Role}: {choice.Message} | Finish Reason: {choice.FinishReason}");
        
        GameObject oj = Instantiate(ChatPrefab, ChatScrollRect.content.transform);
        _chatPrefab = oj.GetComponent<ChatPrefab>();
        _chatPrefab.SetText(npcResponse,choice);
        HandleCharacterByReply(npcResponse.ReplyMessage, npcResponse.Appearance);
        
        string reply = npcResponse.ReplyMessage;
        string combinedDialogue = DialogueExtractor.ExtractAllDialoguesAsString(reply);

        await PlayTTS(combinedDialogue);
        ScrollToBottom();

        // 6. 답변 출력
        Debug.Log($"[{choice.Index}] {choice.Message.Role}: {choice.Message} | Finish Reason: {choice.FinishReason}");
        
        // 7. 답변도 message's 추가
        Message resultMessage = new Message(Role.Assistant, choice.Message);
        _messages.Add(resultMessage);
        
        SendButton.interactable = true;
        
        
    }

    private void HandleCharacterByReply(string replyMessage, string appearance)
    {
        if (replyMessage.Contains("엘리시아"))
        {
            HandleCharacter("엘리시아");
        }
        else if (replyMessage.Contains("리나"))
        {
            HandleCharacter("리나");
        }
        else if (replyMessage.Contains("세레나"))
        {
            HandleCharacter("세레나");
        }
        else
        {
            HandleCharacter(appearance);
        }
    }
    
    private void HandleCharacter(string characterName)
    {
        switch (characterName)
        {
            case "엘리시아":
                _chatPrefab.SetImage(Elisia[0]);
                break;
            case "리나":
                _chatPrefab.SetImage(Rina[0]);
                break;
            case "세레나":
                _chatPrefab.SetImage(Serena[0]);
                break;
            default:
                Debug.Log("알 수 없는 캐릭터 처리");
                // GenerateImage(characterName);
                break;
        }
    }
    private async Task PlayTTS(string combinedDialogue)
    {
        // // Todo: 입력받은 text를 사운드로 재생...
        // var request = new SpeechRequest(text, responseFormat: SpeechResponseFormat.PCM);
        // var speechClip = await _api.AudioEndpoint.GetSpeechAsync(request);
        AudioClip speechClip = await _typecast.StartSpeechAsync(combinedDialogue);
        MyAudioSource.PlayOneShot(speechClip);

    }
    private async void GenerateImage(string imagePrompt)
    {
        var request = new ImageGenerationRequest("A house riding a velociraptor", Model.DallE_3);
        var imageResults = await _api.ImagesEndPoint.GenerateImageAsync(request);
    
        foreach (var result in imageResults)
        {
            Debug.Log(result.ToString());
            // RawImage.texture = result.Texture;
        }
        SendButton.interactable = true;
    }
    
    
    public void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }
    private IEnumerator ScrollToBottomCoroutine()
    {
        if (ChatScrollRect == null) yield break;
        yield return new WaitForEndOfFrame();
        ChatScrollRect.verticalNormalizedPosition = 0f;
    }
}
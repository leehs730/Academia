using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class AddText : MonoBehaviour
{
    /// <summary>
    /// 씬 안에서 스크립트가 표시되는 모든 textbox
    /// </summary>
    private static List<TextMeshProUGUI> textBox = new List<TextMeshProUGUI>();
    /// <summary>
    /// 텍스트 박스 사이에 빈 공간 효과를 주는 empty text
    /// </summary>
    private static List<TextMeshProUGUI> space = new List<TextMeshProUGUI>();
    /// <summary>
    /// 현재 ID값
    /// </summary>
    private static string currentID;
    /// <summary>
    /// 엑셀파일에서 읽어온 현재 id에 대한 스크립트의 정보
    /// </summary>
    private Script script;
    private List<Choice> choice;
    /// <summary>
    /// 씬 안에서 표시되는 모든 image
    /// </summary>
    private static List<Image> imgarr = new List<Image>();
    private static List<bool> isimage = new List<bool>();
    /// <summary>
    /// 공백의 textbox
    /// </summary>
    private static TextMeshProUGUI empty;
    /// <summary>
    /// typing 효과를 주는 coroutine 함수에게 주는 delay값
    /// </summary>
	public static float delay = 0.01f;
    private static Coroutine coroutine;

    private static string fulltext;
    private string currentText;
    private static TextMeshProUGUI tb;

    private static bool text_exit;
    private static bool text_full;
    private static bool text_cut;
    /// <summary>
    /// typing 효과를 주는 coroutine을 시작하는 trigger
    /// </summary>
    private static bool trigger = false;

    private float _emptyTextSize = 0f;

    private float widthRatio = 2.5f;
    private float heightRatio = 2.5f;
    /// <summary>
    /// 스크롤이 밀리는 현상을 없애기 위해 보정값 추가할지 여부
    /// </summary>
    private static bool correction = true;

    private GameObject EndingCanvasK;
    private GameObject EndingCanvasY;
    private GameObject EndingCanvasB1;
    private GameObject EndingCanvasB2;
    // Start is called before the first frame update
    void Start()
    {
        currentID = "M0_0";
        correction = true;  
        AddScript();
    }

    /// <summary>
    /// 스크립트를 텍스트박스에 추가 및 스크롤과 그림에 대한 값 제어
    /// </summary>
    public void AddScript()
    {
        script = MakeDialog.instance.FindScript(NextContainer.instance.NextText);
        DestroyEmpty(); //Destroy empty text prefab 
        //float ScrollAmount = 0;

        if (script.acvUpdate.Count != 0 && PlayerPrefs.GetString("ScriptID") != script.id)
            AcvUpdate_By_CSV(script.acvUpdate);

        //엔딩을 나타내는 text Script 일 때
        if (script.endingTrigger == 1)
        {
            EndingCanvasK = GameObject.Find("Ending").transform.GetChild(0).gameObject;
            EndingCanvasK.GetComponent<Ending>().SetEndingScript(script);
            EndingCanvasK.gameObject.SetActive(true);
            return;
        }
        else if (script.endingTrigger == 2)
        {
            EndingCanvasY = GameObject.Find("Ending").transform.GetChild(0).gameObject;
            EndingCanvasY.GetComponent<Ending>().SetEndingScript(script);
            EndingCanvasY.gameObject.SetActive(true);
            return;
        }
        else if (script.endingTrigger == 3)
        {
            EndingCanvasB1 = GameObject.Find("Ending").transform.GetChild(0).gameObject;
            EndingCanvasB1.GetComponent<Ending>().SetEndingScript(script);
            EndingCanvasB1.gameObject.SetActive(true);
            return;
        }
        else if (script.endingTrigger == 4)
        {
            EndingCanvasB2 = GameObject.Find("Ending").transform.GetChild(0).gameObject;
            EndingCanvasB2.GetComponent<Ending>().SetEndingScript(script);
            EndingCanvasB2.gameObject.SetActive(true);
            return;
        }

        if (currentID[0] == script.id[0] && currentID[1] == script.id[1]) //Check if the screen is switched
        {
            AddSpace();
            if(script.sprite != "null")  //Check if there's a picture that needs to be added
            {
                Sprite pic = Resources.Load<Sprite>("Images/" + script.sprite);
                AddPicture(pic);
                isimage.Add(true);
            }
            else
            {
                isimage.Add(false);
            }

            textBox.Add(Instantiate(Resources.Load<TextMeshProUGUI>("Prefab/TextPrefab")));  //add text prefab
            textBox[textBox.Count-1].transform.SetParent(GameObject.Find("Content").transform , true);

            textBox[textBox.Count - 1].rectTransform.localScale = new Vector3(widthRatio, heightRatio);
            if (!Player.instance.isAdmin)
            {
                PlayerPrefs.SetString("ScriptID", script.id); //save id
                Get_Typing(script.text.Replace("{name}", Player.instance.GetPlayerName()), textBox[textBox.Count-1]);  //typing animation start
            }
            else
            {
                textBox[textBox.Count - 1].text = script.text.Replace("{name}", Player.instance.GetPlayerName());
                text_exit = true;
            }


            if(isimage[isimage.Count-2])  //If image exist, increase the amount of scrolling.
            {
                Scroll.instance.pos += imgarr[imgarr.Count-1].rectTransform.rect.height * imgarr[imgarr.Count-1].rectTransform.localScale.y;// + 200f;
                // ScrollAmount += 480f;
            }

            //Increase the amount of scrolling by the size of the text box
            Scroll.instance.pos += textBox[textBox.Count - 2].GetComponent<RectTransform>().rect.height * textBox[textBox.Count - 2].rectTransform.localScale.y + 200f;
            // ScrollAmount += textBox[textBox.Count-2].GetComponent<RectTransform>().rect.height * 0.555f;
            // Scroll.Instance.pos += 200f;
            // ScrollAmount += 200f;
            AddEmpty();//add empty text
            // Scroll.Instance.scrollAmount = ScrollAmount;
            if (correction)    //보정값 추가
            {
                Debug.Log("보정값 넣기");
                Scroll.instance.pos -= 10;
                correction = false;
            }
            Scroll.instance.isScroll = true;
        }
        else
        {
            DestroyScript();  //destroy previous text
            DestroyPicture();  //destroy previous picture
            DestroySpace();  //destroy previous empty text box
            if(script.sprite != "null")  //Check if there's a picture that needs to be added
            {
                Sprite pic = Resources.Load<Sprite>("Images/" + script.sprite);
                AddPicture(pic);
                isimage.Add(true);
            }
            else
            {
                isimage.Add(false);
            }
            textBox.Add(Instantiate(Resources.Load<TextMeshProUGUI>("Prefab/TextPrefab")));  //add text prefab
            textBox[0].transform.SetParent(GameObject.Find("Content").transform , true);
            textBox[textBox.Count - 1].rectTransform.localScale = new Vector3(widthRatio, heightRatio);
            
            if (!Player.instance.isAdmin)
            {
                PlayerPrefs.SetString("ScriptID", script.id); //save id
                Get_Typing(script.text.Replace("{name}", Player.instance.GetPlayerName()), textBox[0]);  //typing animation start
            }
            else
            {
                textBox[0].text = script.text.Replace("{name}", Player.instance.GetPlayerName());
                text_exit = true;
            }
            

            Scroll.instance.pos = 0f; //reset screen's position
            correction = true; //이제 보정값을 추가해야 함.

            AddEmpty();
            Scroll.instance.ScrollReset();
        }
        NextContainer.instance.NextChoice = script.next;
        currentID = script.id;


        string coloredStr = "";
        string value = script.text;
        Debug.Log(value);

        int indexNum = 0;
        string[] val2 = value.Split('^');
        if (val2.Length != 1)
        {
            indexNum = val2.Length;
            for (int i = 0; i < indexNum; i++)
            {
                if (i % 2 == 0)
                {
                    coloredStr += val2[i];
                }
                else
                {
                    coloredStr += "<color=#008000>";
                    coloredStr += val2[i];
                    coloredStr += "</color>";
                }
            }
            script.text = coloredStr;
        }

        string[] val3 = script.text.Split('*');
        string result = "";
        if (val3.Length != 1)
        {
            indexNum = val3.Length;
            for (int i = 0; i < indexNum; i++)
            {
                if (i % 2 == 0)
                {
                    result += val3[i];
                }
                else
                {
                    result += "<color=#ff0000>";
                    result += val3[i];
                    result += "</color>";
                }
            }
            script.text = result;
        }
    }

    /// <summary>
    /// 아이디가 바뀔 때 씬의 텍스트 박스 모두 제거하여 씬 초기화
    /// </summary>
    public void DestroyScript()
    {
        for(int i = 0; i < textBox.Count(); i++){
            if(textBox[i] != null)
                Destroy(textBox[i].gameObject);
        }
        textBox.Clear();
    }

    /// <summary>
    /// 그림 추가
    /// </summary>
    public void AddPicture(Sprite picture)
    {
        int screen_width = Screen.width;
        int screen_height = Screen.height;

        Image img = Instantiate(Resources.Load<Image>("Prefab/Image"));
        img.sprite = picture;
        //img.rectTransform.localScale =
        //    new Vector3(img.rectTransform.localScale.x - 0.4f, img.rectTransform.localScale.y - 0.4f);
        img.transform.SetParent(GameObject.Find("Content").transform, true);
        img.SetNativeSize();

        float width = img.rectTransform.rect.width;
        float height = img.rectTransform.rect.height;

        // 화면 크기에 대한 삽화의 비율, 0~1사이의 실수값
        float image_ratio = 0.9f;

        if ((width >= screen_width && height >= screen_height && width-screen_width >= height-screen_height)||
            (width >= screen_width && height < screen_height)||
            (width < screen_width && height < screen_height && screen_width-width <= screen_height-height))
        {
            img.rectTransform.sizeDelta = new Vector2(screen_width*image_ratio, height*screen_width/width*image_ratio);
        }
        else if ((width >= screen_width && height >= screen_height && width-screen_width < height-screen_height)||
                (width < screen_width && height >= screen_height)||
                (width < screen_width && height < screen_height && screen_width-width > screen_height-height))
        {
            img.rectTransform.sizeDelta = new Vector2(width*screen_height/height*image_ratio, screen_height*image_ratio);
        }
        else
        {
            img.rectTransform.sizeDelta = new Vector2(screen_width*image_ratio, height*screen_width/width*image_ratio);
            Debug.Log("Unknown Case");
        }
        imgarr.Add(img);
        //Scroll.instance.pos += img.rectTransform.rect.height * img.rectTransform.localScale.y + 200f;
    }

    /// <summary>
    /// 아이디가 바뀔 때 씬의 그림 모두 제거하여 씬 초기화
    /// </summary>
    public void DestroyPicture()
    {
        for(int i = 0; i < imgarr.Count();i++){
            Destroy(imgarr[i].gameObject);
        }
        imgarr.Clear();
    }

    /// <summary>
    /// empty text 추가하여 텍스트가 화면 상단에 오게 content 크기 조절
    /// </summary>
    public void AddEmpty()
    {
        empty = Instantiate(Resources.Load<TextMeshProUGUI>("Prefab/EmptyText"));
        empty.transform.SetParent(GameObject.Find("Content").transform, true);
         if (empty.rectTransform.sizeDelta.y + textBox[textBox.Count - 1].rectTransform.sizeDelta.y*textBox[textBox.Count - 1].rectTransform.localScale.y <=
            empty.transform.parent.parent.GetComponent<RectTransform>().rect.height)
        {
            empty.rectTransform.sizeDelta = new Vector2(textBox[textBox.Count - 1].rectTransform.sizeDelta.x,
                Mathf.Abs(empty.transform.parent.parent.GetComponent<RectTransform>().rect.height -
                          textBox[textBox.Count - 1].rectTransform.sizeDelta.y * textBox[textBox.Count - 1].rectTransform.localScale.y));
            
        }
        empty.rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 이미 사용한 empty text 제거
    /// </summary>
    public void DestroyEmpty()
    {
        if(empty != null)
        {
            Destroy(empty.gameObject);
            empty = null;
        }
    }

    /// <summary>
    /// space 효과
    /// </summary>
    public void AddSpace()
    {
        space.Add(Instantiate(Resources.Load<TextMeshProUGUI>("Prefab/EmptyText")));
        space[space.Count()-1].transform.SetParent(GameObject.Find("Content").transform, true);
        space[space.Count()-1].rectTransform.localScale = Vector3.one;
        space[space.Count()-1].rectTransform.sizeDelta = new Vector2(200, 200);
    }

    /// <summary>
    /// id 전환 시 space 효과 주던 empty text 삭제
    /// </summary>
    public void DestroySpace()
    {
        for (int i = 0; i < space.Count(); i++)
        {
            Destroy(space[i].gameObject);
        }
        space.Clear();
    }
    
    /// <summary>
    /// 선택지를 선택했을 때 stat update 동작 (excel 파일에서 받아옴)
    /// </summary>
    public void Stat()
    {
        for(int i = 0; i < script.result.Count; i++)
        {
            string temp_str = script.result[i];
            string string_val = "";
            if (temp_str == "기사")
            {
                Player.instance.SetPlayerDepartment(PlayerDepartment.Knight);
            }
            else if (temp_str == "마법")
            {
                Player.instance.SetPlayerDepartment(PlayerDepartment.Wizard);
            }
            else if (temp_str == "관료")
            {
                Player.instance.SetPlayerDepartment(PlayerDepartment.Politics);
            }
            else
            {
                if (temp_str != "-")
                {
                    if (temp_str[0] == '지')
                    {
                        if (temp_str[1] == '+')
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.Changeability(PlayerAbility.Intellect, Int32.Parse(string_val));
                        }
                        else
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.Changeability(PlayerAbility.Intellect, Int32.Parse(string_val) * (-1));
                        }
                    }
                    if (temp_str[0] == '무')
                    {
                        if (temp_str[1] == '+')
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.Changeability(PlayerAbility.Force, Int32.Parse(string_val));
                        }
                        else
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.Changeability(PlayerAbility.Force, Int32.Parse(string_val) * (-1));
                        }
                    }
                    if (temp_str[0] == '마')
                    {
                        if (temp_str[1] == '+')
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.Changeability(PlayerAbility.Mana, Int32.Parse(string_val));
                        }
                        else
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.Changeability(PlayerAbility.Mana, Int32.Parse(string_val) * (-1));
                        }
                    }
                    if (temp_str[0] == '체')
                    {
                        if (temp_str[1] == '+')
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.HealthChange(Int32.Parse(string_val));
                        }
                        else
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.HealthChange(Int32.Parse(string_val) * (-1));
                        }
                    }
                    if (temp_str[0] == '정')
                    {
                        if (temp_str[1] == '+')
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.MentalChange(Int32.Parse(string_val));
                        }
                        else
                        {
                            for (int j = 2; j < temp_str.Length; j++)
                            {
                                string_val += temp_str[j];
                            }
                            Player.instance.MentalChange(Int32.Parse(string_val) * (-1));
                        }
                    }
                    else
                    {
                        if (Player.instance.CheckKeyAvailable(temp_str[0].ToString()))
                        {
                            if (temp_str[1] == '+')
                            {
                                for (int j = 2; j < temp_str.Length; j++)
                                {
                                    string_val += temp_str[j];
                                }
                                Player.instance.ChangeLikeable(temp_str[0].ToString(), Int32.Parse(string_val));
                            }
                            else
                            {
                                for (int j = 2; j < temp_str.Length; j++)
                                {
                                    string_val += temp_str[j];
                                }
                                Player.instance.ChangeLikeable(temp_str[0].ToString(), Int32.Parse(string_val) * (-1));
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 타이핑 시작
    /// </summary>
    /// <param name="_fullText">타이핑 효과를 줄 텍스트의 문자열</param>
    /// <param name="textbox">텍스트를 넣을 textbox</param>
    public void Get_Typing(string _fullText, TextMeshProUGUI textbox)
    {
        text_exit = false;
        text_full = false;
        text_cut = false;

        fulltext = _fullText;
        tb = textbox;

        trigger = true;  //if trigger is true, coroutine start at update fuction
    }

    /// <summary>
    /// 타이핑 효과 즉시 종료
    /// </summary>
    public void End_Typing()
    {
        if (!text_full)
        {
            text_cut = true;
        }
    }
    /// <summary>
    /// 타이핑 효과 coroutine
    /// </summary>
    /// <param name="_fullText">타이핑 효과를 줄 텍스트의 문자열</param>
    /// <param name="_textBox">텍스트를 넣을 textbox</param>
    private IEnumerator ShowText(string _fullText, TextMeshProUGUI _textBox)
    {
        currentText = "";
        for (int i = 0; i < _fullText.Length; i++)
        {
            if (text_cut)
            {
                break;
            }
            currentText = _fullText.Substring(0, i + 1);
            _textBox.text = currentText;
            yield return new WaitForSeconds(delay);
        }
        _textBox.text = _fullText;
        text_exit = true;

        StopCoroutine(coroutine);
        text_full = true;
    }

    /// <summary>
    /// CSV 파일에 의한 업적 값 update
    /// </summary>
    /// <param name="achievement">update할 script의 acvUpdate값</param>
    private void AcvUpdate_By_CSV(Dictionary<int, int> achievement)
    {
        foreach (var elem in achievement)
        {
            AchievementManager.Instance.Achieve_achievement(elem.Key, elem.Value);
        }
    }

    /// <summary>
    /// 타이핑 효과 coroutine 제어 및 타이핑 효과 스킵시 동작
    /// </summary>
    void Update() 
    {
        if (!Player.instance.isAdmin)
        {
            if (trigger)
            {
                coroutine = StartCoroutine(ShowText(fulltext, tb));
                trigger = false;
            }
        }
        if (text_exit)  //if typing effect end, next button express.
        {
            int n = GameObject.Find("Panel").transform.childCount;
            for (int i = 0; i < n; i++)
            {
                GameObject.Find("Panel").transform.GetChild(i).gameObject.SetActive(true);
            }

            //empty prefab bug fix
            DestroyEmpty();
            AddEmpty();

            text_exit = false;
        }
        // if (StaticCoroutine.is_play == false && Achivement.acv_delay.Count >= 1)
        // {
        //     AchivementManager.acv_clear(Achivement.acv_delay[0]);
        //     Achivement.acv_delay.RemoveAt(0);
        // }
    }
}
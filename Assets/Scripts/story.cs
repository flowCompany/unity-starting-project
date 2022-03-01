using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Sprites;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class story : MonoBehaviour
{
    struct StoryYaml { 
        public struct Character
        {
            public string name;
            public string face;
            public string shownName;
            public int id;
        }
        public List<Character> characters;

        public class Str
        {
            public int id;
            public string name;
            public string text;
            public int? next;
            public string nextType; //null means 下一个是剧情（或没有下一个）
            public bool isClosed;//是否已经看过该剧情
            public List<int> dependence;
            public List<int> downStream;
            public bool isReproducable;
            public List<int> forbid;
            public bool isForbidden;
            public bool nextRegardless;
        }
        public List<Str> storys;
    }
    private StoryYaml storyYaml;

    struct CharacterStory
    {
        public Sprite face;
        public string shownName;
        public List<int> storyIds;//可能的
        public int id;
    }
    private Dictionary<string, CharacterStory> characterStories;
    private Dictionary<int, string> cid2name;

    public GameObject dialog;
    private main_status mainStatus;

    private void initLoadStory()
    {
        var textFile = Resources.Load<TextAsset>("story");
        var deserializer = new YamlDotNet.Serialization.Deserializer();

        storyYaml = deserializer.Deserialize<StoryYaml>(textFile.text);
        storyYaml.storys.Insert(0, new StoryYaml.Str());

        characterStories = new Dictionary<string, CharacterStory>() { };
        cid2name = new Dictionary<int, string>() { };
        foreach (StoryYaml.Character character in storyYaml.characters)
        {
            Sprite face = Resources.Load<Sprite>("faces/face_" + character.face);
            CharacterStory characterStory = new CharacterStory() { face = face,
                shownName = character.shownName, storyIds = new List<int>() { },
                id = character.id
            };
            characterStories.Add(character.name, characterStory);

            cid2name.Add(character.id, character.name);
        }

        HashSet<int> h = new HashSet<int>() { };
        //Debug.Log("before str: " + storyYaml.storys[47].forbid.ToString());
        for (int i = 0; i < storyYaml.storys.Count; i++)
        {
            storyYaml.storys[i].downStream = new List<int>();
            if (storyYaml.storys[i].forbid is null)
            storyYaml.storys[i].forbid = new List<int>();
        }
        for (int i = 0; i < storyYaml.storys.Count; i++ )
        {
            StoryYaml.Str str = storyYaml.storys[i];
            if (str.id == 0)
                continue;
            if (str.next != null)
            {
                h.Add((int)str.next);
            }
            if ((h.Contains(str.id) && !str.nextRegardless) || !characterStories.ContainsKey(str.name))
            {
                //Debug.Log("oops the " + i.ToString() + " " + str.nextRegardless.ToString());
                continue;
            }
            if (str.dependence is null) str.dependence = new List<int>();
            foreach (int dependentId in str.dependence)
            {
               // Debug.Log(dependentId);
                storyYaml.storys[dependentId].downStream.Add(str.id);
              //  Debug.Log(dependentId);
            }
            if (str.dependence.Count == 0)
                characterStories[str.name].storyIds.Add(str.id);

        }
    }

    private UnityEngine.UI.Text textView;
    private UnityEngine.UI.Text nameTextView;
    private UnityEngine.UI.Image imageView;
    private UnityEngine.UI.Button[] buttons;
    private UnityEngine.UI.Text[] buttonTextViews = new UnityEngine.UI.Text[3];

    // Start is called before the first frame update
    void Start()
    {
        dialog.SetActive(false);
        mainStatus = gameObject.GetComponent<main_status>();
        initLoadStory();

        //获取两个 textView, 用于 UI
        UnityEngine.UI.Text[] texts = dialog.GetComponentsInChildren<UnityEngine.UI.Text>();
        foreach (UnityEngine.UI.Text t in texts)
        {
            if (t.gameObject.name == "Name")
            {
                nameTextView = t;
            } else if (t.gameObject.name == "Text")
            {
                textView = t;
            }
            if (t.gameObject.name == "ButtonText0")
            {
                buttonTextViews[0] = t;
            }
            if (t.gameObject.name == "ButtonText1")
            {
                buttonTextViews[1] = t;
            }
            if (t.gameObject.name == "ButtonText2")
            {
                buttonTextViews[2] = t;
            }
        }
        imageView = dialog.GetComponentInChildren<UnityEngine.UI.Image>();

        buttons = new UnityEngine.UI.Button[3];
        UnityEngine.UI.Button[] bs = dialog.GetComponentsInChildren<UnityEngine.UI.Button>();
        foreach (UnityEngine.UI.Button b in bs)
        {
            if (b.gameObject.name == "Button0")
            {
                buttons[0] = b;
                buttons[0].onClick.AddListener(onClickButton0);
            }
            if (b.gameObject.name == "Button1")
            {
                buttons[1] = b;
                buttons[1].onClick.AddListener(onClickButton1);
            }
            if (b.gameObject.name == "Button2")
            {
                buttons[2] = b;
                buttons[2].onClick.AddListener(onClickButton2);
            }
        }
    }

    private int bufferFrames = 0;
    void Update()
    {
        if (bufferFrames > 0)
        {
            --bufferFrames;
        }
        if (mainStatus.CheckSID(1,"","story_update") && bufferFrames == 0 && nowStoryType == "single")
        {
            if (Input.GetMouseButtonDown(0))
            {
                StoryYaml.Str s = storyYaml.storys[nowStoryId];
                switch (s.nextType)
                {
                    case null:
                        if (s.next is null)
                        {
                            recoverMovingFromStory();
                        }
                        else
                        {
                            StartStory((int)s.next);
                        }
                        break;
                    case "character":
                        if (s.next != null)
                        {
                            if (characterStories[cid2name[(int)s.next]].storyIds.Count > 1)
                            {
                                StartChooseableStory(cid2name[(int)s.next]);
                            }
                            else if (characterStories[cid2name[(int)s.next]].storyIds.Count == 1)
                            {
                                int storyId = characterStories[cid2name[(int)s.next]].storyIds[0];
                                StartStory(storyId);
                                if (!storyYaml.storys[storyId].isReproducable)
                                {
                                    characterStories[cid2name[(int)s.next]].storyIds.RemoveAt(0);
                                }

                            }
                            else
                            {
                                recoverMovingFromStory();
                            }
                        }
                        break;
                    case "combact":
                        mainStatus.startCombactFromStory((int)s.next);
                        dialog.SetActive(false);
                        break;
                }
            }
        }
    }

    private void recoverMovingFromStory()
    {
        dialog.SetActive(false);
        mainStatus.recoverMovingFromStory();
    }

    public bool hasStory(HashSet<int> chosenMovingCids, int targetCid)
    {
        if (!cid2name.ContainsKey(targetCid))
        {
            Debug.Log(gameObject + "Error StartStory: targetCid= " + targetCid);
            return false;
        }
        string targetCname = cid2name[targetCid];
        if (characterStories[targetCname].storyIds.Count == 0)
        {
            Debug.Log("No more story");
            return false;
        }
        return true;
    }
    public bool StartStory(HashSet<int> chosenMovingCids, int targetCid)
    {
        if (!hasStory(chosenMovingCids, targetCid))
        {
            return false;
        }
        string targetCname = cid2name[targetCid];
        if (characterStories[targetCname].storyIds.Count == 1)
        {
            int storyId = characterStories[targetCname].storyIds[0];
            StartStory(storyId);
            if (!storyYaml.storys[storyId].isReproducable)
            {
                characterStories[targetCname].storyIds.RemoveAt(0);
            }
        } else
        {
            StartChooseableStory(targetCname);
        }
        return true;
    }

    private int nowStoryId;
    private string nowStoryType; // "single", "chooseable"
    private string nowStoryCname;

    //不会检验是否合法开启该 story
    public void StartStory(int storyId) {
        if (storyId == 0)
        {
            Debug.Log("Jog " + name + " storyId is 0");
            return;
        }
        if (storyId >= storyYaml.storys.Count)
        {
            Debug.Log("Jog " + name + " storyId more than storys count");
        }
        //Debug.Log("fou");
        StoryYaml.Str s = storyYaml.storys[storyId];
        CharacterStory characterStory = characterStories[s.name];

        s.isClosed = true;

        foreach (int downStreamId in s.downStream)
        {
            updateStoryAvailabilities(downStreamId);
        }
        //Debug.Log("storyId: " + storyId.ToString() + " " + s.forbid.Count.ToString());
        foreach (int forbidId in s.forbid)
        {
            storyYaml.storys[forbidId].isForbidden = true;
            CharacterStory c = characterStories[storyYaml.storys[forbidId].name];
            //Debug.Log("oops you dog: " + storyYaml.storys[forbidId].name);
            if (c.storyIds.Contains(forbidId))
            {
                c.storyIds.Remove(forbidId);
            }
        }
        foreach (UnityEngine.UI.Button b in buttons)
        {
            b.gameObject.SetActive(false);
        }
        textView.text = s.text;
        nameTextView.text = characterStory.shownName;
        imageView.sprite = characterStory.face;
        bufferFrames = 3;
        nowStoryId = storyId;
        nowStoryType = "single";
        dialog.SetActive(true);

        mainStatus.setsceneID(1, "start story" + storyId.ToString());
    }

    //不会检验是否合法开启该角色的 chooseable story
    public void StartChooseableStory(string targetCname)
    {
        nowStoryType = "chooseable";
        nowStoryCname = targetCname;
        List<int> storyIds = characterStories[targetCname].storyIds;
        textView.text = "";
        nameTextView.text = characterStories[targetCname].shownName;
        dialog.SetActive(true);
        imageView.sprite = characterStories[targetCname].face;

        mainStatus.setsceneID(1, "start chooseable story: " + targetCname);

        if (storyIds.Count < 2)
        {
            Debug.Log(gameObject.name + " Error StartChooseableStory: target character have less than 2 available story");
        } else if (storyIds.Count == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                buttons[i].gameObject.SetActive(true);
                buttonTextViews[i].text = storyYaml.storys[storyIds[i]].text;
            }
        } else
        {
            for (int i = 0; i < 3; i++)
            {
                buttons[i].gameObject.SetActive(true);
                buttonTextViews[i].text = storyYaml.storys[storyIds[i]].text;
            }
        }
    }
    public void onClickButton0() {
        Debug.Log("oops click 0");
        int storyId = characterStories[nowStoryCname].storyIds[0];
        StartStory(storyId);
        if (!storyYaml.storys[storyId].isReproducable)
            characterStories[nowStoryCname].storyIds.RemoveAt(0);
    }

    public void onClickButton1() {
        Debug.Log(characterStories[nowStoryCname].storyIds.Count);
        Debug.Log(characterStories[nowStoryCname].storyIds[1]);
        int storyId = characterStories[nowStoryCname].storyIds[1];
        StartStory(characterStories[nowStoryCname].storyIds[1]);
        if (!storyYaml.storys[storyId].isReproducable)
            characterStories[nowStoryCname].storyIds.RemoveAt(1);
    }

    public void onClickButton2() {
        int storyId = characterStories[nowStoryCname].storyIds[2];
        StartStory(characterStories[nowStoryCname].storyIds[2]);
        if (!storyYaml.storys[storyId].isReproducable)
            characterStories[nowStoryCname].storyIds.RemoveAt(2);
    }

    public void updateStoryAvailabilities(int storyId)
    {
        bool isAvailabe = true;
        StoryYaml.Str s = storyYaml.storys[storyId];
        if (s.isForbidden)
        {
            return;
        }
        foreach (int dependentId in s.dependence)
        {
            if (storyYaml.storys[storyId].isClosed)
            {
                isAvailabe = false;
                break;
            }
        }
        if (isAvailabe && !characterStories[s.name].storyIds.Contains(storyId))
        {
            characterStories[s.name].storyIds.Add(storyId);
        }
    }
}

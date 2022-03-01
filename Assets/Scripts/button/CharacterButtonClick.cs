using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterButtonClick : MonoBehaviour,IEndDragHandler
{
    // Start is called before the first frame update
    void Start()
    {
        inp = GameObject.Find("/main_controller").GetComponent<input_controller>();

        EventTrigger trigger = GetComponent<EventTrigger>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void init(int paraID, int __cost = 9, int energy = 12) {
        ID = paraID;
        cost = __cost;
        if (gameObject.name == "buttonenemy(Clone)")
        {
            return;
        }
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>();
        foreach (RectTransform rectTransform in rectTransforms)
        {
            Debug.Log("name: " + rectTransform.gameObject.name);
            if (rectTransform.gameObject.name == "unitCostShadow")
            {
                costImageView = rectTransform.gameObject.GetComponent<UnityEngine.UI.Image>();
            }
            else if (rectTransform.gameObject.name == "unitCostText")
            {
                costTextView = rectTransform.gameObject.GetComponent<UnityEngine.UI.Text>();
            }
        }

        costTextView.text = __cost.ToString();
        updateTextViewAlpha(energy);
    }

    public void updateTextViewAlpha(int energy)
    {
        if (costImageView == null)
        {
            return;
        }
        Color color = costImageView.color;

        //Debug.Log("so energy: " + energy);
        if (energy >= cost)
        {
            color.a = 0f;
        } else
        {
            color.a = .8f;
        }
        costImageView.color = color;
    }
    public void Enter()
    {
        inp.dealCombactButton(common.EnterButtonType, ID);
    }

    public void exit()
    {
        inp.dealCombactButton(common.ExitButtonType, ID);
    }

    public void OnClick()
    {
        Debug.Log("Button Clicked.Character.");
        //inp.CombactCharacterButtonClick(ID);
    }

    public void OnBeginDrag(BaseEventData eventData)
    {
        //Debug.Log("CharacterButtonClick " + "ID: " + ID);
        switch (readyType) {
            case 0:
                inp.onBeginDrag(ID);
                break;
        }
    }
    public void OnDrag(BaseEventData eventData)
    {
        //Debug.Log("Youp you drag");
        switch (readyType)
        {
            case 0:
                inp.OnDrag(ID);
                break;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        switch (readyType)
        {
            case 0:
                //Debug.Log("yeah stop drag: ");
                inp.onEndDrag(ID);
                break;
            case 1:
                //Debug.Log("stop drag1");
                inp.togetherMove(characters.Mouse.calcMousePos());
                break;
        }
    }
    private input_controller inp;
    private int ID;
    private int cost;
    private UnityEngine.UI.Image costImageView;
    private UnityEngine.UI.Text costTextView;
    [HideInInspector]
    private int readyType = 0; //0: ’ŸªΩ£¨ 1£∫“∆∂Ø

    public void makeReady2move()
    {
        readyType = 1;
        inp.addMovingButton(ID);
    }

    public bool isReady2move()
    {
        return readyType == 1;
    }

    public void resetReady2move()
    {
        readyType = 0;
    }
}

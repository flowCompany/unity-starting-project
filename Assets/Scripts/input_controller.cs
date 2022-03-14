using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class input_controller : MonoBehaviour
{
	public GameObject realtimeCombactUI;
	private GameObject panelUI;
	private GameObject moveCommandButton;
	private GameObject energyNumberText;

    private void Start()
    {
		RectTransform[] childs = realtimeCombactUI.GetComponentsInChildren<RectTransform>();
		foreach (RectTransform child in childs)
        {
			switch (child.name)
            {
				case "Panel":
					panelUI = child.gameObject;
					break;
                case "moveCommandButton":
					moveCommandButton = child.gameObject;
					break;
				case "EnergyNumberText":
					energyNumberText = child.gameObject;
					break;
            }
        }

		panelUI.SetActive(false);

		Transform[] transforms = UISpecialEffect.GetComponentsInChildren<Transform>();
		foreach (Transform child in transforms)
        {
			switch (child.name)
            {
				case "RedPlane":
					redPlane = child.gameObject;
					redPlaneMaterial = redPlane.GetComponent<MeshRenderer>().material;
					break;
            }
        }

		combact = GetComponent<Combact>();
    }

	private bool isDraging = false;
    private void Update()
	{
		Debug.Log("Input_controller Update,statusMask:" + statusMask);
		if (statusMask != 0) return;
		if (Input.GetMouseButtonDown(0))
		{
			//characters.pushCharacter();
		}
		if (isDraging)
        {
			redPlaneMaterial.SetFloat("_CenterX", mainCharacter.transform.position[0] - redPlane.transform.position[0]);
			redPlaneMaterial.SetFloat("_CenterZ", mainCharacter.transform.position[2] - redPlane.transform.position[2]);
		}

		updateEnergy();
	}

	public GameObject UISpecialEffect;
	private GameObject redPlane;
	private Material redPlaneMaterial;
	public float summonRadius = 10f;
	public float summonAlpha = 0.5f;
	private bool isActiveDrag = false;
	public void onBeginDrag(int buttonID)
    {
		int unitCost = characters.getCostByButton(buttonID);
		if (unitCost > combact.energy)
        {
			isActiveDrag = false;
			return;
        }
		isActiveDrag = true;
		//Debug.Log("yep you drag");
		CombactCharacterButtonClick(buttonID);
		redPlane.SetActive(true);
		redPlaneMaterial.SetFloat("_Radius", summonRadius);
		isDraging = true;
	}

	private float battlefieldWest = 15f;
	private float battlefieldEast = 75f;
	private float battlefieldNorth = 10f;
	private float battlefieldSouth = -18f;

	public GameObject mainCharacter;
	public void OnDrag(int buttonID)
	{
		if (!isActiveDrag)
			return;
		characters.refreshMouseCharacter();
	}
	public void onEndDrag(int buttonID)
    {
		if (!isActiveDrag)
        {
			return;
        }
		Vector3 worldPosition = characters.Mouse.calcMousePos();
		redPlane.SetActive(false);

		Vector3 distant = mainCharacter.transform.position - worldPosition;
		distant[1] = 0;
		isDraging = false;

		if (worldPosition[0] < battlefieldWest ||
		worldPosition[0] > battlefieldEast ||
		worldPosition[2] < battlefieldSouth || 
		worldPosition[2] > battlefieldNorth || 
		distant.magnitude > summonRadius)
        {
			characters.destroyMouseCharacter();
        } else
        {
			//Debug.Log("charObj endDrag");
			combact.energy -= characters.getCostByButton(buttonID);
			characters.pushCharacter();
			updateButtonEnergy();
		}
	}
    public void CombactCanvasEnterCombactClick()
	{
		Debug.Log("CombactCanvasAttackClick is called");
		combact.enterIntoCombact(0);
	}
	public void CombactCanvasEnterCombactReadyClick()
	{
		Debug.Log("CombactCanvasEnterCombactClick is called");
		combact.enterIntoCombactReady(0);
	}

	public void loadCombactUI()
    {
		panelUI.SetActive(true);
		prepareUISpecialEffect();
	}

	public void CombactCharacterButtonClick(int id)
	{
		Debug.Log("CombactCharacterButtonClick is called");
		characters.createCharByButton(id);
	}

	public void dealCombactButton(int type, int ID)
    {
        switch(type)
        {
			case common.EnterButtonType:
				statusMask |= (1 << ID);
				break;
			case common.ExitButtonType:
				statusMask &= (~(1 << ID));
				break;
		}

		//当鼠标置于按钮上时，隐藏依附鼠标角色
		if(characters.MouseChar != null)
        {
			if(statusMask > 0)
			{
				characters.MouseChar.SetActive(false);
			}
			else
			{
				characters.MouseChar.SetActive(true);
			}
		}
		
	}

	private int statusMask = 0;
	private Combact combact;
	public characters characters;
	//public GameObject mainController;
	private List<GameObject> characterButtons = new List<GameObject>();

	/**
	 * 在进入战斗页面时，更新伙伴按钮
	 */
	public void instantiateButtonFriend(GameObject button, GameObject character, characters.CharacterObject.CharacterType characterType)
    {
		const float buttonCharScale = 22f;
		const float rotation = 170f;
		GameObject newButton = Instantiate(button, panelUI.transform);
		newButton.transform.localScale = new Vector3(2f, 2f, 2f);
		characterButtons.Add(newButton);
		newButton.GetComponent<CharacterButtonClick>().init(characterType.ID, characterType.cost);

		moveCommandButton.transform.SetAsLastSibling();

		GameObject charmodel = Instantiate(character.gameObject, newButton.transform);
		charmodel.transform.localScale = new Vector3(buttonCharScale, buttonCharScale, buttonCharScale);
		charmodel.transform.localRotation = Quaternion.Euler(0, rotation, 0);
		charmodel.transform.localPosition = new Vector3(-15f, -8f, -15f);
		GameObject.Destroy(charmodel.GetComponent<Rigidbody>());
	}

	/**
	 * 在进入战斗时，准备 UI 特效，以在 UI 交互时快速调用这些特效
	 */
	private void prepareUISpecialEffect()
	{
		//redPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		redPlane.GetComponent<Collider>().enabled = false;
		redPlane.GetComponent<MeshRenderer>().material.color = new Color(158f / 255f, 36f / 255f, 36f/ 255f, summonAlpha);
		redPlane.transform.position = new Vector3(
			(battlefieldWest + battlefieldEast) / 2,
			.1f,
			(battlefieldNorth + battlefieldSouth) / 2);
		redPlane.transform.localScale = new Vector3(
			(battlefieldEast - battlefieldWest) / 10,
			1,
			(battlefieldNorth - battlefieldSouth) / 10);
		redPlane.transform.parent = UISpecialEffect.transform;
		redPlane.GetComponent<MeshRenderer>().shadowCastingMode = 0;
		redPlane.SetActive(false);
	}

	public void updateButtonEnergy()
    {
		foreach (GameObject characterButton in characterButtons)
        {
			characterButton.GetComponent<CharacterButtonClick>().updateTextViewAlpha(combact.energy);
        }
    }
	private void updateEnergy()
    {
		string text = combact.energy.ToString();
		energyNumberText.GetComponent<UnityEngine.UI.Text>().text = text;
    }

	/**
	 * 下面这些处理移动逻辑，包括移动按钮的加载和移动指令
	 */
	private List<int> movingButtons = new List<int>();

	// 不会检验是否有按钮为此 buttonId
	public void addMovingButton(int buttonId)
    {
		movingButtons.Add(buttonId);
    }

	//Character Button Click 通过调用 togetherMove 实现角色的统一移动
	public void togetherMove(Vector3 targetPosition)
    {
		//List<characters.CharacterObject> l = new List<characters.CharacterObject>();
		////Debug.Log(movingButtons.Count);
		//foreach (characters.CharacterObject c in characters.charObjList)
		//      {
		//	if (movingButtons.Contains(c.type.ID))
		//          {
		//		l.Add(c);
		//	}
		//      }
		//targetPosition[1] = 0;
		//Debug.Log("There are " + l.Count + " ready to move，targetPosition is: " + targetPosition[0] + 
		//	" " + targetPosition[1] + " " + targetPosition[2]);
		//foreach (characters.CharacterObject c in l)
		//      {
		//	c.moveToTarget(targetPosition);
		//      }

		characters.togetherMoveOrder(targetPosition, movingButtons);

		movingButtons = new List<int>();
		foreach (GameObject cButton in characterButtons)
        {
			cButton.GetComponentInChildren<CharacterButtonClick>().resetReady2move();
			foreach (UnityEngine.UI.Image child in cButton.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
				Debug.Log("childs name: " + child.gameObject.name);
				if (child.name == "moveCommandButton(Clone)")
                {
					Destroy(child.gameObject);
					break; 
				}
            }
        }
    }
}

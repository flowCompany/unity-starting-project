using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveCommandButtonAdapter : MonoBehaviour,IEndDragHandler
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("endDrag");
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hitInfo;
        //if (Physics.Raycast(ray, out hitInfo))
        //{
        //    Debug.Log("EndDrag: " + hitInfo.transform.gameObject.name);

        //}

        Debug.Log("end dog: " + eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject.name);
        if (eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject.name == "buttonfriend(Clone)")
        {
            Debug.Log("end you: " + eventData.pointerCurrentRaycast.gameObject.name);
            GameObject buttonFriend = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;

            CharacterButtonClick characterButtonClick = buttonFriend.GetComponentInChildren<CharacterButtonClick>();
            if (characterButtonClick.isReady2move())
            {
                return;
            }

            characterButtonClick.makeReady2move();

            // 复制移动图标过去
            GameObject moveButtonView = Instantiate(gameObject);
            moveButtonView.transform.SetParent(buttonFriend.transform, false);
            Destroy(moveButtonView.GetComponent<MoveCommandButtonAdapter>());
            moveButtonView.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 0.4f);
            moveButtonView.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            moveButtonView.GetComponent<RectTransform>().position = buttonFriend.GetComponent<RectTransform>().position;
            //moveButtonView.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            moveButtonView.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
            Destroy(moveButtonView.GetComponent<UnityEngine.UI.Button>());
        }


    }

}

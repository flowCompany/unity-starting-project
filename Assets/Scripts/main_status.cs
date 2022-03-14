using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main_status : MonoBehaviour
{
    /***************����ID���**************/
    /***************************************/
    private int lastsceneID = 0;
    private int sceneID = 0; // 0: �ƶ����棻1��������棻2��ս�����棻3��ս�����ý���
    [HideInInspector]
    public bool isStopMoving = false;

    //��SID������֤����
    private bool isLockedSID = false;
    private int LockedSID = -1;

    //ȫ��״̬
    [HideInInspector]
    public static int currentFrame;

    private HashSet<int> chosenMovingCids;
    private story _story;
    private Combact combact;
    public eventFormatSystem eventFormatSystem;
    // Start is called before the first frame update
    void Start()
    {
        chosenMovingCids = new HashSet<int>(){1};
        _story = gameObject.GetComponent<story>();
        combact = GetComponent<Combact>();
    }
    
    public bool LockSID(int SID = -1,string calledFunction = "anonymous_one")
    {
        if (isLockedSID)
        {
            Debug.LogWarningFormat("Scene ID is locked by %d, you shouldn't locked together", LockedSID);
            return false;
        }
        Debug.LogFormat("Scene ID is locked by %d, now change to %d", LockedSID,SID);
        isLockedSID = true;
        LockedSID = SID;
        return true;
    }

    public bool unLockSID(int SID = -1,string calledFunction = "anonymous_one")
    {
        if(!isLockedSID)
        {
            Debug.LogWarningFormat("The SID is not lock, you shouldn't unlock");
            return false;
        }
        if(LockedSID != SID)
        {
            Debug.LogWarningFormat("LockedSID %d is not equal to parameters %d",SID);
        }

        return true;
    }

    // Update is called once per frame
    private bool hasStartFirstStory = false;
    void Update()
    {
        isJustChangeSceneID = false; 
        if (sceneID == 0)
        {
            tryStartStoryFromMoving();
        }
        if (!hasStartFirstStory)
        {
            Debug.Log("update main_status");
            hasStartFirstStory = true;
            _story.StartStory(1);
        }
    }

    //TODO: ����ֻ֧�ֵ�����
    public List<GameObject> mainCharacters;
    public float movingMargin = 4f;
    void tryStartStoryFromMoving() {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo))
            {
                return;
            }
            GameObject gameObject = hitInfo.transform.gameObject;
            Vector3 towardPlace = gameObject.transform.position;
            //Debug.Log(gameObject.GetComponent<character_id>());
            //Debug.Log(gameObject);
            if (gameObject.GetComponent<character_id>() == null)
            {
                return;
            }
            int targetCid = gameObject.GetComponent<character_id>().cid;
            if (_story.hasStory(chosenMovingCids, targetCid))
            {
                if ((mainCharacters[0].transform.position - gameObject.transform.position).magnitude < movingMargin)
                {
                    startStoryFromMoving(targetCid);
                } else
                {
                    Action action = () => { startStoryFromMoving(targetCid); };
                    mainCharacters[0].GetComponent<main_character>().
                        movingWithMargin(movingMargin, towardPlace, action);
                }
            }
        }
    }

    /**
     * Begin: ����̬���ƶ�̬��ս��̬֮����л�
     * 
     */
    /**
     * ���Կ������������������ƶ�̬ת��Ϊ����̬��
     * 1. ������Ǻ�Ŀ������� movingMargin �ڣ������ֱ�ӽ������
     * 2. ������Ǻ�Ŀ������� movingMargin �⣬�����ƶ��� margin �ڣ��ٽ������
     */
    private void startStoryFromMoving(int targetCid)
    {
        Debug.Log("oops you");
        _story.StartStory(chosenMovingCids, targetCid);
        sceneID = 1;
        isStopMoving = true;
    }
    public void recoverMovingFromStory()
    {
        sceneID = 0;
        isStopMoving = false;
    }
    public void startCombactFromStory(int combactId)
    {
        combact.enterIntoCombact(combactId);
        isStopMoving = false;
        setsceneID(3, "", "mainStatus.startCombactFromStory");
    }

    /**
     * End:  ����̬���ƶ�̬��ս��̬֮����л�
     */
    bool isJustChangeSceneID = false;
    public bool setsceneID(int __sceneID, string log, string calledFunction = "anonymous_one")
    {
        if (isLockedSID)
        {
            Debug.LogFormat("Scene ID %d is Locked,function %s shouldn't change to %d,log is %s", sceneID, calledFunction, __sceneID,log);
            return false;
        }

        if (isJustChangeSceneID)
        {
             Debug.LogWarning("Just change sceneID" + log);
             return true;
        }

        sceneID = __sceneID;
        isJustChangeSceneID = true;
        Debug.LogFormat("Scene ID is changed to %d,changed function is %s,log is %s", __sceneID, calledFunction,log);
        return true;
    }

    public bool CheckSID(int __sceneID, string log, string calledFunction = "anonymous_one")
    {
        Debug.LogFormat("CheckSID function is called,SceneID id {0},called by {1}", sceneID, calledFunction);
        return sceneID == __sceneID;
    }
}

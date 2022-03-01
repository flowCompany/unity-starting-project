using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combact : MonoBehaviour
{
    public Vector3 SliderOffset; //Ѫ����Խ�ɫ��ƫ����

    // Start is called before the first frame update
    void Start()
    {
        mstatus = GetComponent<main_status>();
        characters.Init(canvas,canvaswithcamera);
        combactList = GetComponent<CombactList>();

        //combactTimeQueue = gameObject.AddComponent<CombactTimeQueue>();
        //combactTimeQueue.
        combactTimeQueue = new CombactTimeQueue(this){ };

        common.SliderOffset = SliderOffset;
        inputController = GetComponent<input_controller>();
    }

    // Update is called once per frame
    void Update()
    {
        combactTimeQueue.Update();
        //Debug.Log("combact energy: " + energy);
        if (mstatus.CheckSID(common.Combact_Scene,"","combact_update"))
        {
            TaskTree();
            CharacterProcess();
            return;
        }
        else if(mstatus.CheckSID(common.Combact_ReadyScene, "", "combact_update_ready"))
        {
            //characters.refreshMouseCharacter();
            return;
        }
        // ��ʤ��ʱ��Ҫ
        //if (shengli)
        //{
        //    victory()
        //}
    }

    void TaskTree()
    {
        foreach (characters.CharacterObject character in characters.charObjList)
        {
            if (character.data.Inprocess)//1.����ִ������
            {
                
            }
            else //2.���������ȼ�Ѱ������
            {
                if (character.data.task != null)
                {
                    switch (character.data.task.taskID)
                    {
                        case common.Task_Attack: //2.1 ������ȼ���ִ������
                            continue;
                        case common.Task_Alert:  //���䷶Χ�ڵ��ж����޸����ĵз���λ
                            break;
                        case common.Task_Moving: //�ƶ�״̬ͬ�����ж�
                            break;
                        default:
                            Debug.LogWarningFormat("Task ID {0} is wrong", character.data.task.taskID);
                            break;
                    }
                }
                //2.2\2.3 Ѱ�Ҿ��������λ �ڹ�����Χ��ִ�й������ھ��䷶Χ�����������ƶ�
                int ATKIdx = -1;
                double ATKRange = 1000000;//�˴���һ������ֵ��ģ�����ֵ
                for (int i = 0; i < characters.charObjList.Count; i++)
                {
                    //��λ����Ҳ�����ͬ����Ӫ
                    if (characters.charObjList[i].data.death == true || (characters.charObjList[i].data.vaild == false)||
                        (character.bothOwner(characters.charObjList[i]))) continue;
                    double dis = characters.distanceTo(character, characters.charObjList[i]);
                    if (characters.distanceTo(character, characters.charObjList[i]) < ATKRange)
                    {
                            ATKIdx = i;
                            ATKRange = dis;
                    }
                }
                //2.2 ������Χ֮��
                if (ATKIdx != -1 && characters.CharBinCharAATKRange(character, characters.charObjList[ATKIdx]))
                {
                    if (character.data.FreeTime + character.type.ATKSpeed > main_status.currentFrame)
                        continue;

                    if (characters.charObjList[ATKIdx].data.death)
                    {
                        Debug.LogError("���" + ATKIdx + "�Ķ�����������ɫӦ��������Ŀ��");
                        continue;
                    }
                    characters.CharAAttackCharB(character.data.dataIdx, ATKIdx);
                }
                else if (ATKIdx != -1 && characters.CharBinCharAAlertRange(character, characters.charObjList[ATKIdx]))//2.3 ���䷶Χ֮��
                {
                    Debug.Log("is trying attack: " + character.type.ID);
                    character.moveToTarget(characters.charObjList[ATKIdx].obj.transform.position);
                }
                else //2.4�ƶ���Ŀ��ص�
                {

                }
            }
        }
    }

    void CharacterProcess()
    {
        Debug.LogFormat("characters.charObjList length is {0}", characters.charObjList.Count);
        foreach (characters.CharacterObject character in characters.charObjList)
        {
            if(character.data.task == null)
            {
                Debug.LogFormat("{0}th character have no task", character.data.dataIdx);
                continue;
            }
            Debug.LogFormat("{0}th character taskId is {1}", character.data.dataIdx, character.data.task.taskID);
            switch (character.data.task.taskID)
            {
                case common.Task_Moving:
                    character.moving();
                    break;
                default:
                    Debug.LogWarningFormat("{0}th character taskId is {1}", character.data.dataIdx, character.data.task.taskID);
                    break;
            }
        }
    }
    bool IsVictory()
    {
        return false;
    }

    public bool enterIntoCombactReady(int combactId)
    {
        mstatus.setsceneID(common.Combact_ReadyScene, "", "combact_enterIntoCombactReady");
        //��Ⱦ����ť���
        characters.loadCharacterlists();

        combactTimeQueue.addActionafter(0.1f, combactTimeQueue.addEnergyAction);

        combactList.loadCombact(combactId);

        inputController.loadCombactUI();
        return true;
    }
    
    

    public void enterIntoCombact()
    {
        mstatus.setsceneID(common.Combact_Scene, "", "combact_enterIntoCombact");
        combactTimeQueue.addActionafter(0.1f, combactTimeQueue.addEnergyAction);
        return;
    }

    private void Victory()
    {

    }

    public characters characters;
    public Canvas canvas;
    public Canvas canvaswithcamera;
    private main_status mstatus;
    private CombactList combactList;
    private input_controller inputController;

    /**
     * Start: ����ÿ����������ѣ����߼�
     */
    public float addEnergyTimeInterval = 1f;
    public int energy = 10;

    private class CombactTimeQueue : TimeQueue
    {
        public Combact combact;
        public CombactTimeQueue(Combact __combact)
        {
            combact = __combact;
            //0.1 sec ��ӷ��
            
        }

        override protected float proceedSpeed()
        {
            if (combact.mstatus.CheckSID(common.Combact_ReadyScene, "", "combact_update_ready") ||
                combact.mstatus.CheckSID(common.Combact_Scene, "", "combact_update"))
            {
                return 1f;
            } else
            {
                return 0f;
            }
        }

        public void addEnergyAction()
        {
            combact.energy += 1;
            combact.inputController.updateButtonEnergy();
            this.addActionafter(combact.addEnergyTimeInterval, addEnergyAction);
            Debug.Log("successfully add energy");
        }
    }

    private CombactTimeQueue combactTimeQueue = null;

    /**
     * End: ����ÿ����������ѣ����߼�
     */

}

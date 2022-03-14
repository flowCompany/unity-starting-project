using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combact : MonoBehaviour
{
    public Vector3 SliderOffset; //血条相对角色的偏移量

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
            main_status.currentFrame++;
            TaskTree();
            CharacterProcess();
            return;
        }
        else if(mstatus.CheckSID(common.Combact_ReadyScene, "", "combact_update_ready"))
        {
            //characters.refreshMouseCharacter();
            return;
        }
        // 当胜利时需要
        //if (shengli)
        //{
        //    victory()
        //}
    }

    void TaskTree()
    {
        foreach (characters.CharacterObject character in characters.charObjList)
        {
            if (character.data.death) continue;
            if (character.data.Inprocess)//1.正在执行任务
            {
                
            }
            else //2.按任务优先级寻找任务
            {
                if (character.data.task != null)
                {
                    switch (character.data.task.taskID)
                    {
                        case common.Task_Attack: //2.1 最高优先级：执行任务
                            if (character.data.task.CharID != -1 &&characters.charIsDie(character.data.task.CharID)) // 目前没做好死亡时向攻击队列发送消息只能采取主动查询
                                break;
                            continue;
                        case common.Task_Alert:  //警戒范围内得判断有无更近的敌方单位
                            break;
                        case common.Task_Moving: //移动状态同样得判断
                            break;
                        default:
                            Debug.LogWarningFormat("Task ID {0} is wrong", character.data.task.taskID);
                            break;
                    }
                }
                //2.2\2.3 寻找距离最近单位 在攻击范围内执行攻击，在警戒范围内主动向其移动
                int ATKIdx = -1;
                double ATKRange = 1000000;//此处设一个极大值来模拟最大值
                for (int i = 0; i < characters.charObjList.Count; i++)
                {
                    //单位存活且不属于同意阵营
                    if (characters.charIsDie(i)||character.bothOwner(characters.charObjList[i])) continue;
                    double dis = characters.distanceTo(character, characters.charObjList[i]);
                    if (characters.distanceTo(character, characters.charObjList[i]) < ATKRange)
                    {
                            ATKIdx = i;
                            ATKRange = dis;
                    }
                }

                Debug.LogFormat("calculate AtkIdx:{0} atkIdx is {1}", character.data.dataIdx, ATKIdx);
                //2.2 攻击范围之内
                if (ATKIdx != -1 && characters.CharBinCharAATKRange(character, characters.charObjList[ATKIdx]))
                {
                    characters.CharAAttackCharB(character.data.dataIdx, ATKIdx);
                    //character.data.task = new characters.Task
                    //{
                    //    taskID = common.Task_Attack,
                    //    CharID = ATKIdx,
                    //    Target = characters.charObjList[ATKIdx].obj.transform.position
                    //};

                    //Action attactAction = () =>
                    //{
                    //    characters.CharAAttackCharB(character.data.dataIdx, ATKIdx);
                    //};
                    //Debug.LogFormat("{0} is attacked", ATKIdx);
                    //characters.evc.AddEventByTime(attactAction, character.type.ATKSpeed / 120f * 3f);
                }
                else if (ATKIdx != -1 && characters.CharBinCharAAlertRange(character, characters.charObjList[ATKIdx]))//2.3 警戒范围之内
                {
                    Debug.Log("is trying attack: " + character.type.ID);
                    character.moveByTask(characters.charObjList[ATKIdx].obj.transform.position);
                }
                else if(!character.arriveTarget())//2.4移动到目标地点
                {
                    character.moveToTarget(character.data.CurTarget);
                }
                else
                {
                    character.removeTask();
                }
            }
        }
    }

    void CharacterProcess()
    {
        Debug.LogFormat("characters.charObjList length is {0}", characters.charObjList.Count);
        foreach (characters.CharacterObject character in characters.charObjList)
        {
            if (character.data.death) continue;
            if (character.data.task == null)
            {
                Debug.LogFormat("TaskId:{0}th character have no task", character.data.dataIdx);
                continue;
            }
            Debug.LogFormat("TaskId:{0}th character taskId is {1}", character.data.dataIdx, character.data.task.taskID);
            switch (character.data.task.taskID)
            {
                case common.Task_Moving:
                    character.moving();
                    break;
                case common.Task_Attack:
                    //Debug.LogFormat("character.data.FreeTime:{0},character.type.ATKSpeed:{1}", character.data.FreeTime, character.type.ATKSpeed);
                    //if (character.data.FreeTime + character.type.ATKSpeed > main_status.currentFrame) break;
                    //if (characters.charObjList[character.data.task.CharID].data.death)
                    //{
                    //    Debug.LogError("编号" + character.data.task.CharID + "的对象死亡，角色应更换攻击目标");
                    //    continue;
                    //}
                    //characters.CharAAttackCharB(character.data.dataIdx, character.data.task.CharID);
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
        //渲染各按钮组件
        characters.loadCharacterlists();

        combactTimeQueue.addActionafter(0.1f, combactTimeQueue.addEnergyAction);

        combactList.loadCombact(combactId);

        inputController.loadCombactUI();
        return true;
    }
    
    

    public void enterIntoCombact(int combactId)
    {
        mstatus.setsceneID(common.Combact_Scene, "", "combact_enterIntoCombact");
        characters.loadCharacterlists();

        combactTimeQueue.addActionafter(0.1f, combactTimeQueue.addEnergyAction);

        combactList.loadCombact(combactId);

        inputController.loadCombactUI();
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
     * Start: 处理每秒加能量（费）的逻辑
     */
    public float addEnergyTimeInterval = 1f;
    public int energy = 10;

    private class CombactTimeQueue : TimeQueue
    {
        public Combact combact;
        public CombactTimeQueue(Combact __combact)
        {
            combact = __combact;
            //0.1 sec 后加费喽
            
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
     * End: 处理每秒加能量（费）的逻辑
     */

    public void onSomeOneDead()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class eventFormatSystem : MonoBehaviour
{
    public class eventFormatPoint
    {
        public int Frame;
        public int Idx; //在eventArr[Frame]里的索引

        public eventFormatPoint(int frame, int idx)
        {
            Frame = frame; Idx = idx;

        }
    }

    public class eventFormat
    {
        public bool vaild;
        public int sendObj; //发出Object编号
        public int receObj; //接受Object编号
        public int typeIdx; //事件类型
        public string extra;//额外参数
        public eventFormat(int sendobj, int receobj, int typeidx, string extr)
        {
            vaild = true;
            sendObj = sendobj;
            receObj = receobj;
            typeIdx = typeidx;
            extra = extr;
        }

    }

    public class eventAttack
    {
        public eventAttack(int damagenum)
        {
            damageNum = damagenum;
        }
        public int damageNum;
    }

    public class eventController
    {
        public const int EVENT_ATTACK = 1;
        const int eventArrLen = 1000; // 事件链表长度
        public List<eventFormat>[] eventArr = new List<eventFormat>[eventArrLen];
        public List<characters.CharacterObject> charObjList;
        public eventController(List<characters.CharacterObject> list)
        {
            charObjList = list;
            for (int i = 0; i < eventArrLen; i++)
                eventArr[i] = new List<eventFormat>();
        }

        public void TraverseEvent()
        {
            int idx = 0;
            foreach (eventFormat curEvent in eventArr[main_status.currentFrame % eventArrLen])
            {
                Debug.LogFormat("Frame is {0},event idx is {1},receObj is {2},sendObj is {3},vaild is{4}"
                   , main_status.currentFrame, curEvent.typeIdx, curEvent.receObj, curEvent.sendObj, curEvent.vaild);
                if (!curEvent.vaild) continue;
                bool ret;
                switch (curEvent.typeIdx)
                {
                    case common.EVENT_ATTACK:
                        charObjList[curEvent.receObj].PopEvent(idx);
                        ret = dealEventAttack(curEvent);
                        break;
                    case common.EVENT_DIE:
                        dealEventDie(curEvent);
                        break;
                    case common.EVENT_SUCCEED:
                        dealEventSucceed(curEvent);
                        break;
                    default:
                        Debug.LogErrorFormat("事件类型错误，事件类型：%d", curEvent.typeIdx);
                        break;
                }
                idx++;
            }
        }

        //伤害结算
        public bool dealEventAttack(eventFormat curEvent)
        {
            eventAttack eve = JsonConvert.DeserializeObject<eventAttack>(curEvent.extra);
            bool ret = charObjList[curEvent.receObj].UpdateHP(eve.damageNum);

            if (ret)
            {
                //角色死亡：遍历被攻击队列向所有攻击者发送消息
                foreach (eventFormatSystem.eventFormatPoint evp in charObjList[curEvent.receObj].data.AttackedQueue)
                {
                    SetEventUnvaild(evp);
                }
                //角色死亡：注册销毁事件
                eventFormat evef = new eventFormat(curEvent.receObj, curEvent.receObj, common.EVENT_DIE, "");
                AddEventByFrame(evef, common.DIE_FRAME);
                Debug.Log("traverse queue finished，size is " + charObjList[curEvent.receObj].data.AttackedQueue.Count);
                return true;
            }
            return false;
        }

        //死亡结算（执行销毁）
        public void dealEventDie(eventFormat curEvent)
        {
            charObjList[curEvent.receObj].Destroy();
        }
        public void dealEventSucceed(eventFormat curEvent)
        {
            foreach (var obj in charObjList)
            {
                if (obj.data.death || !obj.data.vaild) continue;
                obj.SetAnimatorStatus(obj.obj.GetComponent<Animator>(), common.Animator_Nothing);
            }
        }

        public eventFormatPoint AddEventByFrame(eventFormat eve, int bFrame)
        {
            int AddFrame = ((main_status.currentFrame + bFrame) % eventArrLen + eventArrLen) % eventArrLen;
            //二维数组偏移量(AddFrame,eventArr[AddFrame].Count)
            eventArr[AddFrame].Add(eve);

            eventFormatPoint evp = new eventFormatPoint(AddFrame, eventArr[AddFrame].Count - 1);

            Debug.LogFormat("AddEventByFrame common.Frame is {2},AddFrame is {0},eve.typeIdx is {1} receObj is {2}"
                , AddFrame, eve.typeIdx, main_status.currentFrame);

            return evp;

        }

        public void ClearCurrentEvent()
        {
            eventArr[main_status.currentFrame % eventArrLen].Clear();
        }

        public void SetEventUnvaild(eventFormatPoint evp)
        {
            //将事件置为失效
            eventArr[evp.Frame][evp.Idx].vaild = false;
            //更新单位状态
            charObjList[eventArr[evp.Frame][evp.Idx].sendObj].data.ATKIdx = -1;
            charObjList[eventArr[evp.Frame][evp.Idx].sendObj].data.FreeTime = 0;
            charObjList[eventArr[evp.Frame][evp.Idx].receObj].UnAttackAmination(charObjList[eventArr[evp.Frame][evp.Idx].receObj].obj.GetComponent<Animator>());
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

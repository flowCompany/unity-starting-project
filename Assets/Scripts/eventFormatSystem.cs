using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;


public class eventFormatSystem : MonoBehaviour
{
    public class eventFormatPoint
    {
        public int Frame;
        public int Idx; //��eventArr[Frame]�������

        public eventFormatPoint(int frame, int idx)
        {
            Frame = frame; Idx = idx;
        }
    }

    public class EventFormat
    {
        public bool vaild;
        public int sendObj; //����Object���
        public int receObj; //����Object���
        public int typeIdx; //�¼�����
        public string extra;//�������
        public EventFormat(int sendobj, int receobj, int typeidx, string extr)
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
        const int eventArrLen = 1000; // �¼�������
        public List<EventFormat>[] eventArr = new List<EventFormat>[eventArrLen];
        public List<characters.CharacterObject> charObjList;
        public eventController(characters __characters)
        {
            _characters = __characters;
            charObjList = _characters.charObjList;
            for (int i = 0; i < eventArrLen; i++)
                eventArr[i] = new List<EventFormat>();
        }

        public void TraverseEvent()
        {
            //int idx = 0;
            //foreach (EventFormat curEvent in eventArr[main_status.currentFrame % eventArrLen])
            //{
            //    Debug.LogFormat("Frame is {0},event idx is {1},receObj is {2},sendObj is {3},vaild is{4}"
            //       , main_status.currentFrame, curEvent.typeIdx, curEvent.receObj, curEvent.sendObj, curEvent.vaild);
            //    if (!curEvent.vaild) continue;
            //    bool ret = false;
            //    switch (curEvent.typeIdx)
            //    {
            //        case common.EVENT_ATTACK:
            //            charObjList[curEvent.receObj].PopEvent(idx);
            //            ret = dealEventAttack(curEvent);
            //            break;
            //        case common.EVENT_DIE:
            //            dealEventDie(curEvent);
            //            break;
            //        case common.EVENT_SUCCEED:
            //            dealEventSucceed(curEvent);
            //            break;
            //        default:
            //            Debug.LogErrorFormat("�¼����ʹ����¼����ͣ�%d", curEvent.typeIdx);
            //            break;
            //    }
            //    idx++;
            //}
            timeQueue.Update();
        }

        //�˺�����
        public bool dealEventAttack(EventFormat curEvent)
        {
            eventAttack eve = JsonConvert.DeserializeObject<eventAttack>(curEvent.extra);

            if (charObjList[curEvent.receObj].data.HP <= 0)
            {
                return false;
            }
            bool ret = charObjList[curEvent.receObj].UpdateHP(eve.damageNum);

            if (ret)
            {
                //��ɫ�������������������������й����߷�����Ϣ
                foreach (int evp in charObjList[curEvent.receObj].data.AttackedQueue)
                {
                    //SetEventUnvaild(evp);
                }
                //��ɫ������ע�������¼�
                EventFormat evef = new EventFormat(curEvent.receObj, curEvent.receObj, common.EVENT_DIE, "");
                AddEventByTime(evef, common.DIE_FRAME / 60f);
                Debug.Log("traverse queue finished��size is " + charObjList[curEvent.receObj].data.AttackedQueue.Count);
                return false;
            }

            Action attackAction = () =>
            {
                _characters.CharAAttackCharB(curEvent.sendObj, curEvent.receObj);
            };
            AddEventByTime(attackAction, charObjList[curEvent.sendObj].type.ATKSpeed / 120f);
            return true;
        }

        //�������㣨ִ�����٣�
        public void dealEventDie(EventFormat curEvent)
        {
            charObjList[curEvent.receObj].Destroy();
        }
        public void dealEventSucceed(EventFormat curEvent)
        {
            foreach (var obj in charObjList)
            {
                if (obj.data.death || !obj.data.vaild) continue;
                obj.SetAnimatorStatus(obj.obj.GetComponent<Animator>(), common.Animator_Nothing);
            }
        }

        public eventFormatPoint AddEventByFrame(EventFormat eve, int bFrame)
        {
            int AddFrame = ((main_status.currentFrame + bFrame) % eventArrLen + eventArrLen) % eventArrLen;
            //��ά����ƫ����(AddFrame,eventArr[AddFrame].Count)
            eventArr[AddFrame].Add(eve);

            eventFormatPoint evp = new eventFormatPoint(AddFrame, eventArr[AddFrame].Count - 1);

            Debug.LogFormat("AddEventByFrame common.Frame is {2},AddFrame is {0},eve.typeIdx is {1} receObj is {2}"
                , AddFrame, eve.typeIdx, main_status.currentFrame);

            return evp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventFormat"></param>
        /// <param name="bTime">���೤ʱ���ִ�У���λΪ sec</param>
        public void AddEventByTime (EventFormat curEvent, float bTime)
        {
            Action action = () => {
                if (!curEvent.vaild) return;
                bool ret = false;
                switch (curEvent.typeIdx)
                {
                    case common.EVENT_ATTACK:
                        charObjList[curEvent.receObj].PopEvent(0);
                        ret = dealEventAttack(curEvent);
                        break;
                    case common.EVENT_DIE:
                        dealEventDie(curEvent);
                        break;
                    case common.EVENT_SUCCEED:
                        dealEventSucceed(curEvent);
                        break;
                    default:
                        Debug.LogErrorFormat("�¼����ʹ����¼����ͣ�%d", curEvent.typeIdx);
                        break;
                }
            };
            timeQueue.addActionafter(bTime, action);
        }
        public void AddEventByTime(Action action, float bTime)
        {
            timeQueue.addActionafter(bTime, action);
        }
        public void ClearCurrentEvent()
        {
            eventArr[main_status.currentFrame % eventArrLen].Clear();
        }

        public void SetEventUnvaild(eventFormatPoint evp)
        {
            //���¼���ΪʧЧ
            eventArr[evp.Frame][evp.Idx].vaild = false;
            //���µ�λ״̬
            charObjList[eventArr[evp.Frame][evp.Idx].sendObj].data.ATKIdx = -1;
            charObjList[eventArr[evp.Frame][evp.Idx].sendObj].data.FreeTime = 0;
            charObjList[eventArr[evp.Frame][evp.Idx].receObj].UnAttackAmination(charObjList[eventArr[evp.Frame][evp.Idx].receObj].obj.GetComponent<Animator>());
        }

        TimeQueue timeQueue = new TimeQueue();
        characters _characters;
    }

    public eventController newEventController(characters __characters)
    {
        eventController evc = new eventController(__characters);
        evcList.Add(evc);
        return evc;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.LogFormat("evcList.Count is : {0}", evcList.Count);
        foreach(eventController evc in evcList)
        {
            evc.TraverseEvent();
            evc.ClearCurrentEvent();
        }
    }

    List<eventController> evcList = new List<eventController>();
}

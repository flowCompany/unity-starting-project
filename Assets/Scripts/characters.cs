using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class characters : MonoBehaviour
{
    public class Task
    {
        public int taskID;
        public int CharID;
        public string extra;
    }
    public class CharacterObject
    {
        public class CharacterData
        {
            public int HP;
            public int FreeTime;//上一次（暂为攻击）时间
            public bool Inprocess; //是否正在执行任务
            public Task task;
            public int ATKIdx;//-1表示没有攻击目标
            public bool death;
            public bool vaild; //必须加一个参数确定是否destroy
            public int Owner;
            public int dataIdx;    //这个指的是在List中的编号，不是type编号!!!

            public Vector3 CurTarget;//当前目标点
            public Vector3 FinalTarget;   //最终目标点
            public bool IsInway;     //是否到达目标点

            public Queue<eventFormatSystem.eventFormatPoint> AttackedQueue = new Queue<eventFormatSystem.eventFormatPoint>();
            public int status;   //动作状态
            public CharacterData(ref int num, ref CharacterType charType, ref int curtime, ref int owner, ref Vector3 finaltarget)
            {
                HP = charType.HP; FreeTime = curtime;Inprocess = false; task = null; ATKIdx = -1; death = false; Owner = owner; dataIdx = num;
                FinalTarget = finaltarget; status = 0; vaild = true; CurTarget = finaltarget;
            }
        }
        public class CharacterType
        {
            public int ID;
            public string Name;
            public int Type;
            public int Owner;
            public int HP;
            public int ATK;
            public string dir;
            public float ATKRange;
            public int ATKSpeed;
            public int hurtSpeed;
            public int moveSpeed;
            public float moveRange;
            public float alertRange;//警戒范围 need do
            public GameObject gameObject;
            public int cost;
        }

        public CharacterType type;
        public GameObject obj;
        private GameObject slider;
        public CharacterData data;
        public CharacterObject(GameObject gameObj, CharacterType charType, int curTime, int owner, Vector3 target, int dataIdx,GameObject paraslider)
        {
            slider = paraslider;
            obj = gameObj; type = charType; data = new CharacterData(ref dataIdx, ref charType, ref curTime, ref owner, ref target);
         //   slider = canvas.transform.Find("Slider").GetComponent<Slider>();
            Debug.Log("slider");
        }

        public void SetAnimatorStatus(Animator animator, int status)
        {
            switch (data.status)
            {
                case common.Animator_IsAttack:
                    UnAttackAmination(animator);
                    break;
                case common.Animator_IsMove:
                    UnMoveAmination(animator);
                    break;
                case common.Animator_IsDie:
                    UnDieAnimation(animator);
                    break;
                default:
                    break;
            }
            data.status = status;
            switch (data.status)
            {
                case common.Animator_IsAttack:
                    AttackAmination(animator);
                    break;
                case common.Animator_IsMove:
                    MoveAmination(animator);
                    break;
                case common.Animator_IsDie:
                    DieAnimation(animator);
                    break;
                default:
                    break;
            }
        }

        public void PopEvent(int idx)
        {
            eventFormatSystem.eventFormatPoint evp = data.AttackedQueue.Dequeue();
            if (idx != evp.Idx)
                Debug.Log("evp Is Error,charidx is " + idx + "evpidx is " + evp.Idx);
        }

        public bool bothOwner(CharacterObject charA)
        {
            return data.Owner == charA.data.Owner;
        }
        /******************** 攻击 ********************/
        //执行攻击 对象
        public void DoAttack()
        {
            //刷新最后空闲时间
            data.FreeTime = main_status.currentFrame;
            //调整角度为对应单位方向
            SetRotationToMove(data.CurTarget);
            //执行动画效果
            SetAnimatorStatus(obj.GetComponent<Animator>(), common.Animator_IsAttack);
        }
        //BeAttacked: 受到攻击对象 
        public void BeAttacked(int doAtkCharIdx)
        {
            //执行动画效果：受伤

            
        }

        public void AttackAmination(Animator animator)
        {
            animator.SetBool("IsAttack", true);
        }

        public void UnAttackAmination(Animator animator)
        {
            animator.SetBool("IsAttack", false);
        }

        /******************** 死亡 ********************/
        public bool UpdateHP(int num)
        {
            data.HP -= num;
            data.HP = Math.Min(data.HP, type.HP);
            data.HP = Math.Max(data.HP, 0);
            //执行动画效果
            //更新血条
            slider.GetComponent<Slider>().value = 100.0F * data.HP / type.HP;
            Debug.LogFormat("call updateHp,num is {0},slider.value is {1}", num, slider.GetComponent<Slider>().value);
            //检测有无死亡
            if (data.HP == 0)
            {
                data.death = true;
                //执行倒地动画
                SetAnimatorStatus(obj.GetComponent<Animator>(), common.Animator_IsDie);
                //传递死亡事件，回收变量

                return true;
            }
            return false;
        }

        public void Destroy()
        {
            GameObject.Destroy(obj);
            data.vaild = false;
        }

        public void DieAnimation(Animator animator)
        {
            animator.SetBool("IsDie", true);
        }

        public void UnDieAnimation(Animator animator)
        {
            Debug.Log("call UnDie" + data.dataIdx);
            animator.SetBool("IsDie", false);
        }
        /******************** 寻路 ********************/

        /*       public void FindTarget(Vector3 end)
               {
                   Vector3 beg = obj.transform.position;
                   Vector3 mid = (beg + end) / 2;
                   while (Vector3.Distance(mid, end) > common.eps)
                   {
                       if (ImpactChecking())
                       {
                           AstarFindWay(ref data.WayQueue);
                       }
                       else
                       {
                           data.WayQueue.Enqueue(mid);
                       }
                       beg = mid;
                       mid = (beg + end) / 2;
                   }
               }
        
        //碰撞检测：目前把单位视为原点，不执行碰撞检测
        public bool ImpactChecking()
        {
            return false;
        }

        //目前不需要考虑A*算法
        public void AstarFindWay(ref Queue<Vector3> que)
        {

        }*/
        public void SetRotationToMove(Vector3 target)
        {
            //通过tan计算旋转角度
            Vector3 vc = target - obj.transform.position;
            float Angle = Mathf.Atan2(vc.x, vc.z) * Mathf.Rad2Deg;
            obj.transform.rotation = Quaternion.Euler(Angle * Vector3.up);
        }

        public void moveByAnimator(Animator animator, Vector3 end)
        {
            //        Debug.LogFormat("character {0} isFindTarget,obj.Position is {1},Target is {2}"
            //            , data.dataIdx, obj.transform.position, end);
            SetRotationToMove(end);
            SetAnimatorStatus(animator, common.Animator_IsMove);
        }

        /*        //添加一个更新的目标
                public void AddTargetHead(Vector3 vec)
                {
                    data.WayStack.Enqueue(vec);
                }

        //模拟双端队列取值
               public void changeTarget()
               {
                   //没有额外目标，则朝终点行进
                   if (data.WayStack.Count == 0 && data.WayQueue.Count == 0)
                   {
                       data.CurTarget = data.FinalTarget;
                   }
                   if (data.WayStack.Count == 0)
                   {
                       data.CurTarget = data.WayStack.Dequeue();
                   }
                   else
                   {
                       data.CurTarget = data.WayQueue.Dequeue();
                   }
               }
        */
        public void moveToTarget(Vector3 target,float speed = 1.0F)
        {
            data.CurTarget = target;
            moveToTarget(speed);
        }

        public void moveToTarget(float speed = 1.0F)
        {
            data.task = new Task
            {
                taskID = common.Task_Moving
            };
            Animator ani = obj.GetComponent<Animator>();
            ani.speed = speed;
            moveByAnimator(ani, data.CurTarget);
        }

        public void moving()
        {
            Vector3 distance = data.CurTarget - obj.transform.position;

            float singleStep = type.moveSpeed * Time.deltaTime;

      //      Vector3 newDirection = Vector3.RotateTowards(obj.transform.forward, distance, singleStep, 0.0f);
     //       obj.transform.rotation = Quaternion.LookRotation(newDirection);

            Vector3 deltaV = distance.normalized *
                type.moveSpeed * Time.deltaTime;
            //Debug.LogFormat("{0} is moving,moving delta is {1} {2} {3} {4}", data.dataIdx, deltaV.magnitude, distance.normalized, type.moveSpeed, Time.deltaTime);

            if (deltaV.magnitude > (data.CurTarget - obj.transform.position).magnitude)
            {
                Debug.Log("Thats bug");
                obj.transform.position = data.CurTarget;
            }
            Vector3 v = deltaV + obj.transform.position;

            obj.transform.position = v;
        }

 /*       public void move(Animator animator)
        {
            //如果本帧能走到下一个目标点，则过度到再下一个
            double dis = Time.deltaTime * type.moveSpeed;
            while (dis > Vector3.Distance(obj.transform.position, data.WayQueue.Peek()))
            {
                dis -= Vector3.Distance(obj.transform.position, data.WayQueue.Peek());
                data.WayQueue.Dequeue();
            }
            //移动到对应位置:对应单位向量*dis

            //角色角度调整为单位向量的方向

            //执行移动动画
            SetAnimatorStatus(animator, common.Animator_IsMove);
        }*/

        public void MoveAmination(Animator animator)
        {
            animator.SetBool("IsMove", true);
        }

        public void UnMoveAmination(Animator animator)
        {
            animator.SetBool("IsMove", false);
        }
        public bool CheckFinishWay(Animator animator, Vector3 end)
        {
            if (Vector3.Distance(obj.transform.position, end) < common.arriveTargetEps)
            {
                //获取新Target（目前不会有障碍，暂不考虑），以及判断胜利条件
                return true;
            }
            return false;
        }

        public double distanceTo(ref CharacterObject atkObj)
        {
            double xdis = (obj.transform.position.x - atkObj.obj.transform.position.x);
            double ydis = (obj.transform.position.y - atkObj.obj.transform.position.y);
            double zdis = (obj.transform.position.z - atkObj.obj.transform.position.z);
            return Math.Sqrt(xdis * xdis + zdis * zdis);
        }

        
    }


    public void Init(Canvas paracanvas,Canvas paracanvaswithcamera)
    {
        canvas = paracanvas;
        canvasWithCamera = paracanvaswithcamera;
        buttonenemy = Resources.Load<GameObject>("CharacterButtom/buttonenemy");
        buttonfriend = Resources.Load<GameObject>("CharacterButtom/buttonfriend");
        sliderEnemy = Resources.Load<GameObject>("BloodSlider/SliderEnemy");
        sliderFriend = Resources.Load<GameObject>("BloodSlider/SliderFriend");
    }

    /**********角色单位相互作用**********/
    public void CharAAttackCharB(int CharAidx, int CharBidx)
    {
        //挂载idx编号Obj受到伤害事件
        eventFormatSystem.eventAttack eveAttack = new eventFormatSystem.eventAttack(charObjList[CharAidx].type.ATK);
        string jsonAttack = JsonConvert.SerializeObject(eveAttack);
        eventFormatSystem.eventFormat eve = new eventFormatSystem.eventFormat(CharAidx, CharBidx, common.EVENT_ATTACK, jsonAttack);
        eventFormatSystem.eventFormatPoint evp = evc.AddEventByFrame(eve, charObjList[CharAidx].type.hurtSpeed);
        
        charObjList[CharBidx].data.AttackedQueue.Enqueue(evp);
        charObjList[CharBidx].BeAttacked(CharAidx);
        charObjList[CharAidx].DoAttack();
    }
    public bool CharBinCharAATKRange(CharacterObject charA, CharacterObject charB)
    {
        if (distanceTo(charA,charB) <= charA.type.ATKRange) return true;
        return false;
    }

    public bool CharBinCharAAlertRange(CharacterObject charA, CharacterObject charB)
    {
        if (distanceTo(charA, charB) <= charA.type.alertRange) return true;
        return false;
    }

    public double distanceTo(CharacterObject charA,CharacterObject charB)
    {
        double xdis = (charA.obj.transform.position.x - charB.obj.transform.position.x);
        double ydis = (charA.obj.transform.position.y - charB.obj.transform.position.y);
        double zdis = (charA.obj.transform.position.z - charB.obj.transform.position.z);
        return Math.Sqrt(xdis * xdis + zdis * zdis);
    }

    public struct mapData
    {
        public Vector3 cameraPos;
        public struct Character
        {
            public int ID;
            public Vector3 Pos;
        }
        public List<Character> charlist;
    }
    mapData mapdata;

    public List<CharacterObject.CharacterType> characterList;
    public List<CharacterObject> charObjList = new List<CharacterObject>();
    public void InitCharacters(string id)
    {
        var textFile = Resources.Load<TextAsset>("characters/maps/" + id);
        var deserializer = new YamlDotNet.Serialization.Deserializer();

        mapdata = deserializer.Deserialize<mapData>(textFile.text);
        foreach (mapData.Character character in mapdata.charlist)
        {
            pushCharacter(character.ID, character.Pos);
        }
    }


    CharacterObject InstantCharObj(int typeIdx, Vector3 tr, Quaternion rota, int owner)
    {
        Debug.Log("Instantiate2");
        //Step 1.复制对象
        GameObject instObj = Instantiate(characterList[typeIdx].gameObject, tr, rota) as GameObject;
        //  Step 1.1 slider对象在canvas所以单独复制
        GameObject slider = null;
        if (owner == 0) slider = Instantiate(sliderFriend, canvas.transform);
        else if (owner == 1) slider = Instantiate(sliderEnemy, canvas.transform);
        slider.GetComponent<SliderController>().init(instObj,slider);
        //Step 2.装载对象
        CharacterObject charObj = new CharacterObject(instObj, characterList[typeIdx], 0, owner,  Vector3.zero, charObjList.Count,slider);
        charObj.UpdateHP(0);
        //Step 3.add进列表
        Debug.LogFormat("charObjList.Count is {0}", charObjList.Count);
        //charObjList.Add(charObj);
        return charObj;
    }

    public void addInstantCharObj(int typeIdx, Vector3 tr, Quaternion rota, int owner)
    {
        charObjList.Add(InstantCharObj(typeIdx, tr, rota, owner));
    }

    public void loadCharacterlists()
    {
        int friendIdx = 0, enemyIdx = 0;
        int screenWidth = 400;
        int screenHeight = 200;
        float buttonWidth = 75;
        float buttonCharScale = 22;
        float charDisplace = -15;
        float DisToBoundary = buttonWidth / 2;
        int buttonDis = 100;
        int charBulgeHeight = -10; //单位突起高度，防止被覆盖
        float buttonScale = 1.5F;  // 决定上面部分属性
        buttonWidth *= buttonScale; DisToBoundary *= buttonScale; charDisplace *= buttonScale;


        Debug.Log("loadCharacterlists");
        var textFile = Resources.Load<TextAsset>("characters/list");
        var deserializer = new YamlDotNet.Serialization.Deserializer();

        characterList = deserializer.Deserialize<List<CharacterObject.CharacterType>>(textFile.text);

        foreach (CharacterObject.CharacterType character in characterList)
        {
            character.gameObject = Resources.Load<GameObject>("characters/prefabs/" + character.dir);
            float buttonx = 0, buttony = 0, buttonz = 0,rotation = 0;
            float charx = 0,chary = charDisplace, charz = charBulgeHeight;
            GameObject button = null;
            if (character.Owner == 0)
            {
                buttonx = -(screenWidth - buttonDis * friendIdx - DisToBoundary);
                buttony = screenHeight - DisToBoundary;
                rotation = 135;
                friendIdx++;
                button = Instantiate(buttonfriend, canvasWithCamera.transform);

                charx = -charBulgeHeight;

                //JoyJ 的逻辑：
                input_controller inputController = GetComponent<input_controller>();
                inputController.instantiateButtonFriend(buttonfriend, character.gameObject, character);
            }
            else if (character.Owner == 1)
            {
                buttonx = screenWidth - buttonDis * enemyIdx - DisToBoundary;
                buttony = screenHeight - DisToBoundary;
                rotation = 225;
                enemyIdx++;
                button = Instantiate(buttonenemy, canvasWithCamera.transform);

                charx = charBulgeHeight;
            }
            Debug.Log("x:" + buttonx + ",y:" + buttony + ",Vector:" + new Vector3(buttonx, buttony, 0));
            Debug.Log("CombactButton transform.position is:" + button.transform.position.x + "," + button.transform.position.y + "," + button.transform.position.z);

            //给每个按钮初始化ID
            button.GetComponent<CharacterButtonClick>().init(character.ID);

            button.transform.localPosition = new Vector3(buttonx, buttony, 0);
            button.transform.localScale = new Vector3(buttonScale, buttonScale, buttonScale);

            GameObject charmodel = Instantiate(character.gameObject, button.transform);
            charmodel.transform.localScale = new Vector3(buttonCharScale, buttonCharScale, buttonCharScale);
            charmodel.transform.localRotation = Quaternion.Euler(0,rotation,0);
            charmodel.transform.localPosition = new Vector3(charx, chary, charz);
            Component.Destroy(charmodel.GetComponent<Rigidbody>());
            Debug.Log("character : " + character);


        }
    }

    // FixMe: 狗福这里为啥不销毁MouseChar
    public void pushCharacter(int ID, Vector3 Pos)
    {
        if(MouseChar == null)
        {
            return;
        }
        charObjList.Add(InstantCharObj(ID,Pos,Quaternion.identity,characterList[MouseCharID].Owner));
    }

    public int getCostByButton (int buttonId)
    {
        if (characterList[buttonId].ID == buttonId)
        {
            return characterList[buttonId].cost;
        } else
        {
            Debug.LogWarning("Characters.getCostByButton: Id is not equal to idx!");
            return -1;
        }
    }
    public void createCharByButton(int paraID)
    {
        if (characterList[paraID].ID == paraID)
        {
            if (paraID != MouseCharID)
            {
                GameObject.Destroy(MouseChar);
                MouseChar = CreateCharacterByID(paraID);
                MouseCharID = paraID;
            }            
            Debug.Log("create character,ID is :" + paraID);
        }   
        else
        {
            Debug.LogWarning("createCharByButton: ID is not equal to idx!");
        }

    }

    public void pushCharacter()
    {
        if (MouseChar == null)
        {
            return;
        }
        charObjList.Add(InstantCharObj(MouseCharID, Mouse.calcMousePos(), Quaternion.identity, characterList[MouseCharID].Owner));
        destroyMouseCharacter();
    }

    private GameObject CreateCharacterByID(int paraID)
    {
        Debug.Log("Instantiate");
        GameObject charmodel = Instantiate<GameObject>(characterList[paraID].gameObject);
        Component.Destroy(charmodel.GetComponent<Rigidbody>());
        Component.Destroy(charmodel.GetComponent<CapsuleCollider>());
        return charmodel;
    }

    //用于更新拖动的角色的位置
    public void refreshMouseCharacter()
    {
        if (MouseChar == null) return;
        //Debug.Log(Mouse.calcMousePos()[0] + " " + Mouse.calcMousePos()[1] + " " + Mouse.calcMousePos()[2]);
        MouseChar.transform.position = Mouse.calcMousePos();
    }

    public void destroyMouseCharacter()
    {
        GameObject.Destroy(MouseChar);
        MouseChar = null;
        MouseCharID = -1;
    }
    public class Mouse
    {        
        public static Vector3 calcMousePos()

        {
            Vector3 screenPos = Input.mousePosition;
            float lft = 0.1F, rht = 60, mid;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            while (rht - lft > common.eps)
            {
                mid = (lft + rht) / 2;
                screenPos.z = mid;
                worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                if (Mathf.Abs(worldPos.y - common.HorizonYxis) < common.eps)
                {
                    break;
                }
                if (worldPos.y > common.HorizonYxis)
                {
                    lft = mid;
                }
                else
                {
                    rht = mid;
                }
            }
            return worldPos;
        }

    }

    private eventFormatSystem.eventController evc;

    private int MouseCharID = -1;
    [HideInInspector]
    public GameObject MouseChar = null;
    private GameObject buttonfriend, buttonenemy;
    private GameObject sliderFriend, sliderEnemy;
    private Canvas canvasWithCamera;
    private Canvas canvas;

    // Begin: 下面处理统一移动逻辑

    public void togetherMoveOrder(Vector3 targetPosition, List<int> characterIds)
    {
        if (characterIds.Count == 0)
        {
            return;
        }
        characters.TogetherMove togetherMove = new characters.TogetherMove(this);
        togetherMove.togetherMoveOrder(targetPosition, characterIds);
    }

    private class TogetherMove
    {
        characters _characters;

        public TogetherMove(characters c)
        {
            _characters = c;
        }

        public void togetherMoveOrder(Vector3 targetPosition, List<int> characterIds)
        {
            targetPosition[1] = 0; // 限制移动不能上天入地
            List<int> orderCharacterIds = getReadyMoveCharacters(characterIds);
            calculateTargetPositions(orderCharacterIds, targetPosition);
        }
        private List<int> getReadyMoveCharacters(List<int> characterIds)
        {
            List<int> l = new List<int>();
            //Debug.Log(movingButtons.Count);
            for (int i = 0; i < _characters.charObjList.Count; i++) { 
                if (characterIds.Contains(_characters.charObjList[i].type.ID))
                {
                    Debug.Log("characterType: " + i + " " + _characters.charObjList[i].type.ID);
                    l.Add(i);
                }
            }

            Debug.Log("movingCharacters.Count = " + l.Count);
            return l;
        }

        private class TargetPositions
        {
            public Quaternion quaternion = new Quaternion();

            public class TargetPosition {
                public Vector3 position = new Vector3();
                public bool isOccupied = false;
            }
            public List<TargetPosition> positions = new List<TargetPosition>();
        }


        static float verticalMargin = 1.2f;
        static float horizontalMargin = 1.2f;
        /// <summary>
        /// 计算目的地的队伍阵列
        /// </summary>
        /// <param name="orderCharacterIds"></param>
        /// <param name="targetPosition"></param>
        /// 
        /// <ininparm name="verticalMargin"></ininparm>
        /// <ininparm name="horizontalMargin"></ininparm>
        /// <returns></returns>
        private TargetPositions calculateTargetPositions(List<int> orderCharacterIds, Vector3 targetPosition)
        {
            Vector3 originCenter = new Vector3();
            orderCharacterIds.ForEach(characterId =>
            {
                originCenter += _characters.charObjList[characterId].obj.transform.position;
            });
            originCenter /= orderCharacterIds.Count;

            TargetPositions targetPositions = new TargetPositions();
            targetPositions.quaternion.SetLookRotation(targetPosition - originCenter, new Vector3(0, 1, 0));

            int n = (int) Math.Sqrt(orderCharacterIds.Count);
            int m = orderCharacterIds.Count / n;
            float horizontalLength = (m - 1) * horizontalMargin;
            float verticalLength = (n - 1) * verticalMargin;

            Quaternion turnLeft = new Quaternion();
            turnLeft.eulerAngles = new Vector3(0, 0, 0);

            for (int i = 0; i < orderCharacterIds.Count; i++)
            {
                Vector3 v = new Vector3(i / n, 0, m / 2 - i % n);

                v = targetPositions.quaternion * turnLeft * v;

                Debug.Log("TargetPlace: " + v[0] + " " + v[1] + " " + v[2]);

                _characters.charObjList[orderCharacterIds[i]].moveToTarget(v + targetPosition);
                targetPositions.positions.Add(new TargetPositions.TargetPosition() { position = v, isOccupied = false });
            }
            return targetPositions;
        }
    }
    // End: 上方是统一移动逻辑
}

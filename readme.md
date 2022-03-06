# 克苏苏岛行动

## 作品链接

链接：https://pan.baidu.com/s/1aVZ-oJgN0G57zWOiZ1MoqA 
提取码：zjfp

## 目录

- [克苏苏岛行动](#克苏苏岛行动)
  - [程序结构](#程序结构)
    - [移动模块](#移动模块)
    - [剧情模块](#剧情模块)
    - [战斗模块](#战斗模块)
      - [单位类 CharacterObject](#单位类-characterobject)
      - [地图信息 mapData](#地图信息-mapdata)
      - [数据成员](#数据成员)
        - [兵种列表 characterList](#兵种列表-characterlist)
        - [单位列表 charObjList](#单位列表-charobjlist)
      - [函数列表](#函数列表)
        - [Init](#init)
        - [CharacterObject::CharacterObject](#characterobjectcharacterobject)
        - [CharacterObject::SetRotationToTarget](#lcharacterobjectsetrotationtotarget)
        - [CharacterObject::SetAnimatorStatus](#characterobjectsetanimatorstatus)
        - [DoAttack](#doattack)
        - [BeAttacked](#beattacked)
        - [UpdateHP](#updatehp)
        - [Destroy](#destroy)
        - [move](#move)
        - [CheckFinishWay](#checkfinishway)
        - [CharAAttackCharB](#charaattackcharb)
        - [InitCharacters](#initcharacters)
        - [InstantCharObj](#instantcharobj)
        - [DoAttack](#doattack)
        - [loadCharacterlists](#loadcharacterlists)
        - [createCharByButton](#createcharbybutton)
        - [pushCharacter](#pushcharacter)
        - [CreateCharacterByID](#createcharacterbyid)
        - [refreshMouseCharacter](#refreshmousecharacter)
        - [destroymousecharacter](#destroymousecharacter)
        - [Mouse::calcMousePos](#mousecalcmousepos)

## 程序结构

![](readmepic/structure.png)

游戏分为四种场景——剧情场景、移动场景、战斗准备场景以及战斗场景，同一时刻处于一种场景之中。

针对四种场景，分别设立剧情模块、移动模块、战斗模块（同时负责战斗准备场景以及战斗场景）

单位模块负责控制所有单位

主状态模块负责控制全局状态（例如现在处于哪种场景之中）

### 移动模块

### 剧情模块

### 战斗模块

#### 基本概念
>任务优先级：  
>1)执行任务：比如正在进行攻击\治疗  
>2)在攻击范围内有敌方单位(距离越近优先级越高)    
>3)在警戒范围内有敌方单位(距离越近优先级越高)  
1.任务队列：通过优先队列实现
2.警戒范围：如果有敌方目标在这个范围内，则会有攻击的意图  
3.攻击范围：目标可以直接执行攻击的范围

#### 战斗流程

1.执行任务   
2.按任务优先级寻找任务，将其加入任务队列  
3.执行任务队列中首个任务  
（因为当前的战斗流程是，如果在执行任务，那么继续执行；没有执行任务时，由于攻击范围和警戒范围都通过距离计算，所以通过找寻最近敌方单位来实现。）

### 单位模块

#### 单位类 CharacterObject

#### 地图信息 mapData  
战斗的地图信息，包含镜头位置，单位以及所在位置

#### 数据成员

##### 兵种列表 characterList
List<CharacterType>类型 存放兵种类型

##### 单位列表 charObjList
List<CharacterObject>类型 存放战斗单位
在点击放置按钮单位时，跟随鼠标的单位仅用于展示，不会加入单位列表

#### 函数列表 

##### Init
初始化

##### CharacterObject::CharacterObject
```cpp
CharacterObject(GameObject gameObj, 
	CharacterType charType,//单位类型
	int curTime,           //当前时间
	int owner,	         //属于哪一方
	Vector3 target,        //目标位置
	int dataIdx            //在单位列表里的编号
)
```
##### CharacterObject::SetRotationToTarget
将单位朝向与行动方向一致

##### CharacterObject::SetAnimatorStatus
设置单位的Animator,单位有以下几种animator
- 攻击 Animator_IsAttack
  - AttackAmination
  - UnAttackAmination
-  移动 Animator_IsMove
  - MoveAmination
  - UnMoveAmination
- 死亡 Animator_IsDie
  - DieAnimation
  - UnDieAnimation

##### DoAttack
单位执行攻击

##### BeAttacked
单位受到攻击

##### UpdateHP
更新单位血量，如果死亡，传递死亡事件

##### Destroy
销毁单位模型并标记

##### ~~FindTarget~~
寻找到目标地点

##### ~~moveByAnimator~~
向目标地点移动并调整Animator

##### ~~AddTargetHead~~
添加一个更新的目标

##### ~~changeTarget~~
更新目的地

##### moveToTarget
向目标移动

##### ~~move~~
移动

##### CheckFinishWay
检查是否到达目的地

##### CharAAttackCharB
单位A对单位B发起一次攻击

##### InitCharacters
读取[地图信息](#地图信息-mapdata)，在场上放置单位

##### InstantCharObj
根据ID创建单位，并将其加入[单位列表](#单位列表-charobjlist)

##### loadCharacterlists
读取[兵种列表](#兵种列表-characterlist)，存到characterList数组，生成放兵按钮

##### createCharByButton
通过按钮创建对应单位，并绑定到MouseChar对象上，用来跟随鼠标

##### pushCharacter
接上条，鼠标点击后，即在点击位置放置单位，并将其加入单位列表

##### CreateCharacterByID
通过兵种ID创建对象

##### refreshMouseCharacter
刷新MouseChar对象的位置，让其跟随鼠标

##### destroyMouseCharacter
销毁MouseChar对象，比如会在有对象跟随鼠标时点击开始战斗时触发

##### Mouse::calcMousePos
计算鼠标所在的位置在3D系统里的投射
由于2D（屏幕）转3D（系统）会有一条直线都是可行解，这里的投射是指单位正好放置在地面的解（目前没考虑地形问题，那么就是Y为0得情况）

### 主状态模块


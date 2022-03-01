using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class common : MonoBehaviour
{
    public const int Combact_Scene = 2;
    public const int Combact_ReadyScene = 3;
    public static float eps = 0.1F;
    public static float HorizonYxis = 0.23F;
    public const int EnterButtonType = 0;
    public const int ExitButtonType = 1;

    public const int EVENT_ATTACK = 1;
    public const int EVENT_DIE = 2;
    public const int EVENT_SUCCEED = 3;

    public const int Animator_Nothing = 0;
    public const int Animator_IsAttack = 1;
    public const int Animator_IsMove = 2;
    public const int Animator_IsDie = 3;

    public const int Task_Attack = 0;
    public const int Task_Alert  = 1;
    public const int Task_Moving = 2;
    public static float arriveTargetEps = 0.1F;

    public static Vector3 SliderOffset;
    public const int DIE_FRAME = 50; //À¿Õˆªÿ ’÷° ˝
}

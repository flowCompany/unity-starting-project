using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void init(GameObject fatherObj,GameObject myObj)
    {
        father = fatherObj;
        me = myObj;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        var col = father.GetComponent<Collider>();
        var topAhcor = new Vector3(col.bounds.center.x, col.bounds.max.y, col.bounds.center.z);
        var viewPos = Camera.main.WorldToViewportPoint(topAhcor); //得到视窗坐标


        Vector2 screenPos;

        if (viewPos.z > 0f && viewPos.x > 0f && viewPos.x < 1f && viewPos.y > 0f && viewPos.y < 1f)
        {

            //获取屏幕坐标
            screenPos = Camera.main.WorldToScreenPoint(topAhcor + common.SliderOffset); //加上头顶偏移量
        }
        else
        {
            //不在可视窗口的模型，把名字移动到视线外
            screenPos = Vector3.up * 3000f;
        }

        me.transform.position = screenPos;
    }
    private GameObject father;
    private GameObject me;
}

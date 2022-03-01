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
        var viewPos = Camera.main.WorldToViewportPoint(topAhcor); //�õ��Ӵ�����


        Vector2 screenPos;

        if (viewPos.z > 0f && viewPos.x > 0f && viewPos.x < 1f && viewPos.y > 0f && viewPos.y < 1f)
        {

            //��ȡ��Ļ����
            screenPos = Camera.main.WorldToScreenPoint(topAhcor + common.SliderOffset); //����ͷ��ƫ����
        }
        else
        {
            //���ڿ��Ӵ��ڵ�ģ�ͣ��������ƶ���������
            screenPos = Vector3.up * 3000f;
        }

        me.transform.position = screenPos;
    }
    private GameObject father;
    private GameObject me;
}

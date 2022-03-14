using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ���ڴ�����ʱ���󣬱���ÿ��ӷѣ��ӷѱ���ȵ�һ������ʵ�֣�������ܿ���ͨ�� TimeQueue ʵ��
 */
public class TimeQueue
{
    SortedDictionary<float, List<Action>> tsQueue = new SortedDictionary<float, List<Action>>();
    float nowTime = 0f; //��λΪ sec
    float fakeTime = 0; //��λΪ sec

    /// <summary>
    /// TimeQueue ������һ�� monoBehavior������Ҳ����ÿ֡���� Update()
    /// Update() �а����жϵ�ǰ֡���޻���Լ�����ǰ֡���н��ٶ�
    /// ��������ֻ�������ҽ���һ������ Update()
    /// </summary>
    public void Update()
    {
        nowTime += Time.deltaTime;
        //Debug.Log("timeQueue: now count = " + tsQueue.Count);
        handleScheduled();
        handleProceedSpeed();
    }

    private void handleScheduled()
    {
        Debug.Log("handling energy");
        while (tsQueue.Count > 0)
        {
            var i = tsQueue.GetEnumerator();
            i.MoveNext();
            float scheduledTime = i.Current.Key;
            //Debug.Log("energy: " + scheduledTime + " " + (fakeTime) + " " + nowTime);
            if (scheduledTime > nowTime - fakeTime)
            {
                return;
            }
            foreach (Action a in i.Current.Value)
            {
                a();
            }
            tsQueue.Remove(scheduledTime);
        }
    }

    /** �����ǰ֡�н��ٶ�Ϊ 0�� ��ô��ǰ֡��ʱ���ȫ���������� fakeTs
     *  ���     �н��ٶ�Ϊ 1�� ��ôfakeTs������
     */
    private void handleProceedSpeed()
    {
        float speed = proceedSpeed();
        if (speed == 1f)
        {
            return;
        } else if (speed == 0f)
        {
            fakeTime += Time.deltaTime;
        }
    }

    //��ǰ֡���н��ٶ�
    virtual protected float proceedSpeed()
    {
        return 1f;
    }

    // duration ��λΪ��
    public void addActionafter(float duration, Action action)
    {
        //Debug.Log("energy: adding" + nowTime);
        if (!tsQueue.ContainsKey(nowTime + duration - fakeTime))
        {
            tsQueue[nowTime + duration - fakeTime] = new List<Action>();
        }
        tsQueue[nowTime + duration - fakeTime].Add(action);
    }
}

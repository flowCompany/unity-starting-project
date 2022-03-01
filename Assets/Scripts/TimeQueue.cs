using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 用于处理延时需求，比如每秒加费，加费必须等到一秒后才能实现，这个功能可以通过 TimeQueue 实现
 */
public class TimeQueue
{
    SortedDictionary<float, List<Action>> tsQueue = new SortedDictionary<float, List<Action>>();
    float nowTime = 0f;
    float fakeTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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

    /** 如果当前帧行进速度为 0， 那么当前帧的时间会全部用于增加 fakeTs
     *  如果     行进速度为 1， 那么fakeTs不增加
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

    //当前帧的行进速度
    virtual protected float proceedSpeed()
    {
        return 1f;
    }

    // duration 单位为秒
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

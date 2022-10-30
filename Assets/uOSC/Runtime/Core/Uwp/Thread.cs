#if NETFX_CORE

using UnityEngine;
using System;
using System.Threading.Tasks;

namespace uOSC.Uwp
{

public class Thread : uOSC.Thread
{
    Task task_;
    bool isRunning_ = false;
    Action loopFunc_ = null;

    public override void Start(Action loopFunc)
    {
        if (isRunning_ || loopFunc == null) return;

        isRunning_ = true;
        loopFunc_ = loopFunc;

        task_ =  new Task(ThreadLoop);
        task_.Start();
    }

    async void ThreadLoop()
    {
        while (isRunning_)
        {
            try
            {
                loopFunc_();
                await Task.Delay(IntervalMillisec);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
        }
    }

    public override void Stop(int timeoutMilliseconds = 3000)
    {
        if (!isRunning_) return;

        isRunning_ = false;

        if (!task_.IsCompleted)
        {
            task_.Wait(timeoutMilliseconds);
        }
    }
}

}

#endif
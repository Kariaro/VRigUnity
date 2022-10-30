#if !NETFX_CORE

using UnityEngine;
using System;

namespace uOSC.DotNet
{

public class Thread : uOSC.Thread
{
    System.Threading.Thread thread_;
    bool isRunning_ = false;
    Action loopFunc_ = null;

    public override void Start(Action loopFunc)
    {
        if (isRunning_ || loopFunc == null) return;

        isRunning_ = true;
        loopFunc_ = loopFunc;

        thread_ = new System.Threading.Thread(ThreadLoop);
        thread_.Start();
    }

    void ThreadLoop()
    {
        while (isRunning_)
        {
            try
            {
                loopFunc_();
                System.Threading.Thread.Sleep(IntervalMillisec);
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

        if (thread_.IsAlive)
        {
            thread_.Join(timeoutMilliseconds);
            if (thread_.IsAlive)
            {
                thread_.Abort();
            }
        }
    }
}

}

#endif
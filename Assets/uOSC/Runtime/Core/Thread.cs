namespace uOSC
{

public abstract class Thread
{
    protected const int IntervalMillisec = 1;

    public abstract void Start(System.Action loopFunc);
    public abstract void Stop(int timeoutMilliseconds = 3000);
}

}
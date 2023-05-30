namespace TCP.Listener.Task_1._TcpListener.Models;

public class TimerActionResult<T>
{
    public TimeSpan ElapsedTime { init; get; }
    public T ActionResult { init; get; }

    public void Deconstruct(out T actionResult, out TimeSpan elapsedTime)
    {
        actionResult = ActionResult;
        elapsedTime = ElapsedTime;
    }
}

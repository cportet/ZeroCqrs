namespace ZeroCqrs.Tests;

public class HandlerSpy
{
    public bool Executed { get; private set; }

    public void MarkAsExecuted() => Executed = true;
}
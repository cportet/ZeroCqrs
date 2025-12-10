namespace ZeroCqrs;

public interface IQuery<out TResponse> { }
public interface ICommand { }
public interface ICommand<out TResponse> { }

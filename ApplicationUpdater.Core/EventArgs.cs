namespace ApplicationUpdater.Core;

public delegate void EventHandlerT<T>(T value);
public delegate void EventHandlerT<T, K>(T value1, K value2);
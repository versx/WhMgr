namespace WhMgr.Net
{
    using System;

    public sealed class DataReceivedEventArgs<T> : EventArgs
    {
        public T Data { get; }

        public DataReceivedEventArgs(T data)
        {
            Data = data;
        }
    }
}
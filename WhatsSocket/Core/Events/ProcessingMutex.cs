﻿namespace WhatsSocket.Core.Events
{
    public class ProcessingMutex
    {
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public ProcessingMutex()
        {

        }

        public async Task Mutex(Action action)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                action();
            }
            catch (Exception)
            {
            }
            semaphoreSlim.Release();
        }
    }
}

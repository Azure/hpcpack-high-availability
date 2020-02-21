namespace Microsoft.Hpc.HighAvailabilityModule.Interface
{
    using System;
    using System.Threading.Tasks;

    public interface IMembershipAlgorithm
    {
        Task RunAsync(Func<Task> onStartAsync, Func<Task> onErrorAsync);

        void Stop();
    }
}

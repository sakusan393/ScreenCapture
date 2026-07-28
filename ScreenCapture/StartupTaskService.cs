using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace ScreenCapture
{
    internal static class StartupTaskService
    {
        public const string TaskId = "ScreenCaptureStartup";

        public static bool IsAvailable
        {
            get
            {
                try
                {
                    _ = Package.Current.Id;
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        public static async Task<StartupTaskState> GetStateAsync()
        {
            var startupTask = await StartupTask.GetAsync(TaskId);
            return startupTask.State;
        }

        public static async Task<StartupTaskState> SetEnabledAsync(bool enabled)
        {
            var startupTask = await StartupTask.GetAsync(TaskId);
            if (enabled)
            {
                return await startupTask.RequestEnableAsync();
            }

            startupTask.Disable();
            return startupTask.State;
        }
    }
}

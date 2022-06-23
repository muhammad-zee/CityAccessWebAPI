using System;
using TwilioAutomationService;

namespace MDRoute.TwilioAutomationService
{
    class Program
    {
        static void Main(string[] args)
        {
            AutomationService.TriggerQueues();
        }
    }
}

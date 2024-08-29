#region Using directives
using System;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using UAManagedCore;
#endregion

public class AlarmsGenerator : BaseNetLogic
{
    private PeriodicTask alarmsGeneratorTask;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        alarmsGeneratorTask = new PeriodicTask(AlarmsGeneratorMethod, 10000, LogicObject);
        alarmsGeneratorTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        alarmsGeneratorTask?.Dispose();
    }

    private void AlarmsGeneratorMethod()
    {
        Random rnd = new Random(DateTime.Now.Millisecond);
        for (int i = 1; i <= 9; i++)
        {
            if (rnd.Next(0, 2) == 1)
            {
                Project.Current.GetVariable("Model/Alarms/DemoA/Variable" + i).Value = true;

            }
            else
            {
                Project.Current.GetVariable("Model/Alarms/DemoA/Variable" + i).Value = false;
            }
        }
    }
}

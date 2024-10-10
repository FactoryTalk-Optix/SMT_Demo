#region Using directives
using System;
using System.Threading;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using UAManagedCore;
#endregion

public class PickPlaceSimulation : BaseNetLogic
{
    private PeriodicTask pickPlaceTask;
    Random rnd;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        if(!Project.Current.GetVariable("Model/EnableSimulations").Value)
        {
            Log.Info(this.GetType().Name, "Simulation is disabled");
            return;
        }

        rnd = new Random();
        pickPlaceTask = new PeriodicTask(PickPlaceProgressTask, 750, LogicObject);
        pickPlaceTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        pickPlaceTask?.Dispose();
    }

    private void PickPlaceProgressTask()
    {
        var pickPlaceProgress1 = Project.Current.GetVariable("Model/Cycle/LiveData/PickPlaceProgress1");
        var pickPlaceProgress2 = Project.Current.GetVariable("Model/Cycle/LiveData/PickPlaceProgress2");
        var pickPlaceProgress3 = Project.Current.GetVariable("Model/Cycle/LiveData/PickPlaceProgress3");
        var pickPlaceOverall = Project.Current.GetVariable("Model/Cycle/LiveData/PickPlaceOverall");
        if (Project.Current.GetVariable("Model/Cycle/Start").Value)
        {
            if (pickPlaceProgress1.Value < 100)
            {
                pickPlaceProgress1.Value += rnd.Next(0, 3);
                Thread.Sleep(1);
                if (pickPlaceProgress1.Value >= 100)
                {
                    pickPlaceProgress1.Value = 100;
                }
            }
            if (pickPlaceProgress2.Value < 100)
            {
                pickPlaceProgress2.Value += rnd.Next(0, 3);
                Thread.Sleep(1);
                if (pickPlaceProgress2.Value >= 100)
                {
                    pickPlaceProgress2.Value = 100;
                }
            }
            if (pickPlaceProgress3.Value < 100)
            {
                pickPlaceProgress3.Value += rnd.Next(0, 3);
                Thread.Sleep(1);
                if (pickPlaceProgress3.Value >= 100)
                {
                    pickPlaceProgress3.Value = 100;
                }
            }
            pickPlaceOverall.Value = ((int)pickPlaceProgress1.Value.Value + (int)pickPlaceProgress2.Value.Value + (int)pickPlaceProgress3.Value.Value) / 3;
            if (pickPlaceOverall.Value >= 100)
            {
                pickPlaceProgress3.Value = 0;
                pickPlaceProgress2.Value = 0;
                pickPlaceProgress1.Value = 0;
                pickPlaceOverall.Value = 0;
            }
        }
    }
}

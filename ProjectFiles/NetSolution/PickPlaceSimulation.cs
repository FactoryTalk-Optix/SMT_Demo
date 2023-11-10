#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Alarm;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Retentivity;
using FTOptix.Core;
using System.Threading;
using FTOptix.OPCUAServer;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.WebUI;
#endregion

public class PickPlaceSimulation : BaseNetLogic
{
    private PeriodicTask PickPlaceTask;
    Int32 incrementPickPlace1 = 0;
    Int32 incrementPickPlace2 = 0;
    Int32 incrementPickPlace3 = 0;
    Random rnd;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        rnd = new Random();
        PickPlaceTask = new PeriodicTask(PickPlaceProgressTask, 750, LogicObject);
        PickPlaceTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        PickPlaceTask?.Dispose();
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
                pickPlaceProgress1.Value = pickPlaceProgress1.Value + rnd.Next(0, 3);
                Thread.Sleep(1);
                if (pickPlaceProgress1.Value >= 100)
                {
                    pickPlaceProgress1.Value = 100;
                }
            }
            if (pickPlaceProgress2.Value < 100)
            {
                pickPlaceProgress2.Value = pickPlaceProgress2.Value + rnd.Next(0, 3);
                Thread.Sleep(1);
                if (pickPlaceProgress2.Value >= 100)
                {
                    pickPlaceProgress2.Value = 100;
                }
            }
            if (pickPlaceProgress3.Value < 100)
            {
                pickPlaceProgress3.Value = pickPlaceProgress3.Value + rnd.Next(0, 3);
                Thread.Sleep(1);
                if (pickPlaceProgress3.Value >= 100)
                {
                    pickPlaceProgress3.Value = 100;
                }
            }
            pickPlaceOverall.Value = ((Int32)pickPlaceProgress1.Value.Value + (Int32)pickPlaceProgress2.Value.Value + (Int32)pickPlaceProgress3.Value.Value) / 3;
            if (pickPlaceOverall.Value >= 100)
            {
                pickPlaceProgress3.Value = 0;
                pickPlaceProgress2.Value = 0;
                pickPlaceProgress1.Value = 0;
                incrementPickPlace1 = 0;
                incrementPickPlace2 = 0;
                incrementPickPlace3 = 0;
                pickPlaceOverall.Value = 0;
            }
        }
        else
        {
            // pickPlaceProgress3.Value = 0;
            // pickPlaceProgress2.Value = 0;
            // pickPlaceProgress1.Value = 0;
            // incrementPickPlace1 = 0;
            // incrementPickPlace2 = 0;
            // incrementPickPlace3 = 0;
            // pickPlaceOverall.Value = 0;
        }
    }
}

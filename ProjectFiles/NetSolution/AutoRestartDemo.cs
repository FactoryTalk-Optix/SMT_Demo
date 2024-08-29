#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.EventLogger;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.WebUI;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Alarm;
using FTOptix.Core;
#endregion

public class AutoRestartDemo : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        cycleStatus = Project.Current.GetVariable("Model/Cycle/Start");
        idleCounterTask = new PeriodicTask(IdleCounter, 1000, LogicObject);
        idleCounterTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        idleCounterTask?.Dispose();
    }

    private void IdleCounter()
    {
        if (!(bool)cycleStatus.Value.Value)
        {
            idleCounter++;
            if (idleCounter > 60)
            {
                idleCounter = 0;
                Project.Current.GetVariable("Model/Cycle/Start").Value = true;
            }
        }
        else
        {
            idleCounter = 0;
        }
    }

    private IUAVariable cycleStatus;
    private int idleCounter = 0;
    private PeriodicTask idleCounterTask;
}

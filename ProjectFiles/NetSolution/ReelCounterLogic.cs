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

public class ReelCounterLogic : BaseNetLogic
{
    private PeriodicTask ReelCounter;
    private Random rnd;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        ReelCounter = new PeriodicTask(ComponentsCounter, 150, LogicObject);
        rnd = new Random();
        ReelCounter.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        ReelCounter?.Dispose();
    }

    private void ComponentsCounter()
    {
        if (Project.Current.GetVariable("Model/Cycle/Start").Value)
        {
            var reelCounter1 = Project.Current.GetVariable("Model/Cycle/LiveData/ReelCounter1");
            var reelCounter2 = Project.Current.GetVariable("Model/Cycle/LiveData/ReelCounter2");
            var reelCounter3 = Project.Current.GetVariable("Model/Cycle/LiveData/ReelCounter3");
            var reelCounter4 = Project.Current.GetVariable("Model/Cycle/LiveData/ReelCounter4");
            if (rnd.Next(0, 3) == 1)
            {
                if (reelCounter1.Value > 0)
                {
                    --reelCounter1.Value;
                }
            }
            Thread.Sleep(1);
            if (rnd.Next(0, 3) == 1)
            {
                if (reelCounter2.Value > 0)
                {
                    --reelCounter2.Value;
                }
            }
            Thread.Sleep(1);
            if (rnd.Next(0, 3) == 1)
            {
                if (reelCounter3.Value > 0)
                {
                    --reelCounter3.Value;
                }
            }
            Thread.Sleep(1);
            if (rnd.Next(0, 3) == 1)
            {
                if (reelCounter4.Value > 0)
                {
                    --reelCounter4.Value;
                }
            }
            Thread.Sleep(1);
            if (reelCounter1.Value < 2500) {
                Project.Current.GetVariable("Model/Alarms/ErrorPP1").Value = true;
                Project.Current.GetVariable("Model/Alarms/WarningPP1").Value = false;
            } else if (reelCounter1.Value < 5000) {
                Project.Current.GetVariable("Model/Alarms/ErrorPP1").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP1").Value = true;
            } else {
                Project.Current.GetVariable("Model/Alarms/ErrorPP1").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP1").Value = false;
            }
            if (reelCounter2.Value < 2500) {
                Project.Current.GetVariable("Model/Alarms/WarningPP2").Value = false;
                Project.Current.GetVariable("Model/Alarms/ErrorPP2").Value = true;
            } else if (reelCounter2.Value < 5000) {
                Project.Current.GetVariable("Model/Alarms/ErrorPP2").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP2").Value = true;
            } else {
                Project.Current.GetVariable("Model/Alarms/ErrorPP2").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP2").Value = false;
            }
            if (reelCounter3.Value < 2500) {
                Project.Current.GetVariable("Model/Alarms/WarningPP3").Value = false;
                Project.Current.GetVariable("Model/Alarms/ErrorPP3").Value = true;
            } else if (reelCounter3.Value < 5000) {
                Project.Current.GetVariable("Model/Alarms/ErrorPP3").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP3").Value = true;
            } else {
                Project.Current.GetVariable("Model/Alarms/ErrorPP3").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP3").Value = false;
            }
            if (reelCounter4.Value < 2500) {
                Project.Current.GetVariable("Model/Alarms/WarningPP4").Value = false;
                Project.Current.GetVariable("Model/Alarms/ErrorPP4").Value = true;
            } else if (reelCounter4.Value < 5000) {
                Project.Current.GetVariable("Model/Alarms/ErrorPP4").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP4").Value = true;
            } else {
                Project.Current.GetVariable("Model/Alarms/ErrorPP4").Value = false;
                Project.Current.GetVariable("Model/Alarms/WarningPP4").Value = false;
            }
        }
    }
}

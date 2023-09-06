#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.EventLogger;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.Core;
#endregion

public class AirPressureGauges : BaseNetLogic
{
    private PeriodicTask pressureTask1;
    private PeriodicTask pressureTask2;
    private PeriodicTask pressureTask3;
    float targetPres1;
    float targetPres2;
    float targetPres3;
    Random rnd;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        targetPres1 = LogicObject.GetVariable("targetPress1").Value;
        targetPres2 = LogicObject.GetVariable("targetPress2").Value;
        targetPres3 = LogicObject.GetVariable("targetPress3").Value;
        rnd = new Random();
        if (targetPres1 > 0.0) {
            pressureTask1 = new PeriodicTask(task1, 550, LogicObject);
            pressureTask1.Start();
        }
        if (targetPres2 > 0.0) {
            pressureTask2 = new PeriodicTask(task2, 550, LogicObject);
            pressureTask2.Start();
        }
        if (targetPres3 > 0.0) {
            pressureTask3 = new PeriodicTask(task3, 550, LogicObject);
            pressureTask3.Start();
        }
    }

    private void task1 (){
        double newVal = rnd.NextSingle() * ((targetPres1 + 0.01) - (targetPres1 - 0.01)) + (targetPres1 - 0.01);
        Project.Current.GetVariable("Model/Settings/AnalogVariable1").Value = Math.Round(newVal, 2);
    }
    private void task2 (){
        double newVal = rnd.NextSingle() * ((targetPres2 + 0.01) - (targetPres2 - 0.01)) + (targetPres2 - 0.01);
        Project.Current.GetVariable("Model/Settings/AnalogVariable2").Value = Math.Round(newVal, 2);
    }
    private void task3 (){
        double newVal = rnd.NextSingle() * ((targetPres3 + 0.01) - (targetPres3 - 0.01)) + (targetPres3 - 0.01);
        Project.Current.GetVariable("Model/Settings/AnalogVariable3").Value = Math.Round(newVal, 2);
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        pressureTask1?.Dispose();
        pressureTask2?.Dispose();
        pressureTask3?.Dispose();
    }
}

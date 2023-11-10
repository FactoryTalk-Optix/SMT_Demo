#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.OPCUAServer;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.EventLogger;
using FTOptix.Core;
using System.Linq;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.WebUI;
#endregion

public class DemoA_AlarmsWidgetLogic : BaseNetLogic
{
    private PeriodicTask alarmsGeneratorTask;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        alarmsGeneratorTask = new PeriodicTask(AlarmsGenerator, 750, LogicObject);
        alarmsGeneratorTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        alarmsGeneratorTask?.Dispose();
    }

    private void AlarmsGenerator() {
        Random rnd = new Random(DateTime.Now.Millisecond);
        int activeAlarms = 0;
        for (int i = 1; i <= 9; i++)
        {
            if (Project.Current.GetVariable("Model/Alarms/DemoA/Variable" + i).Value) {
                ++activeAlarms;
                var objectInPage = Owner.Get<AlarmWidgetType>("VerticalLayout1/Alarm" + i) ?? null;
                if (objectInPage == null) {
                    objectInPage = InformationModel.Make<AlarmWidgetType>("Alarm" + i);
                    objectInPage.GetVariable("InputAlarm").Value = Project.Current.Get<DigitalAlarm>("Alarms/LineOverview/DigitalAlarm" + i).NodeId;
                    objectInPage.GetVariable("Time").Value = DateTime.Now;
                    Owner.Get<ColumnLayout>("VerticalLayout1").Add(objectInPage);
                }
                
            } else {
                Owner.Get("VerticalLayout1/Alarm" + i)?.Delete();
            }
        }
        Owner.Get<ColumnLayout>("VerticalLayout1").Height = activeAlarms * 35;
    }
}

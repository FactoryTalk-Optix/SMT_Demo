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
using System.Linq;
using FTOptix.WebUI;
#endregion

public class AlarmIconLogic : BaseNetLogic
{
    private PeriodicTask alarmsMonitoringTask;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        alarmsMonitoringTask = new PeriodicTask(bellLogic, 750, LogicObject);
        alarmsMonitoringTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        alarmsMonitoringTask?.Dispose();
    }

    private void bellLogic() {
        var outVar = LogicObject.GetVariable("Status");
        var retainedAlarms = LogicObject.Context.GetNode(FTOptix.Alarm.Objects.RetainedAlarms);
        var localizedAlarms = InformationModel.Get(retainedAlarms.GetVariable("LocalizedAlarms").Value);
        int tempSeverity = 0;
        foreach (var child in localizedAlarms.Children)
        {
            int severity = child.GetVariable("Severity").Value ?? 0;
            if (severity > 0) {
                if (tempSeverity < severity) {
                    tempSeverity = severity;
                }
            }
        }
        outVar.Value = tempSeverity;
    }
}

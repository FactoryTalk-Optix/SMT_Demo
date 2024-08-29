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

public class ChatLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        if (Owner.Get<Button>("Close").Enabled)
        {
            autoDeleteMessageTask = new DelayedTask(AutoDeleteMessage, 60000, LogicObject);
            autoDeleteMessageTask.Start();
        }
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        autoDeleteMessageTask?.Dispose();
    }

    private void AutoDeleteMessage()
    {
        Owner.Delete();
    }

    private DelayedTask autoDeleteMessageTask;
}

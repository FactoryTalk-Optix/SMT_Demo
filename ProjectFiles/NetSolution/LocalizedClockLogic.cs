#region Using directives
using System;
using CoreBase = FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using FTOptix.UI;
using FTOptix.NetLogic;
using FTOptix.OPCUAServer;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.WebUI;
#endregion

public class LocalizedClockLogic : BaseNetLogic
{
	public override void Start()
	{
		periodicTask = new PeriodicTask(UpdateTime, 1000, LogicObject);
		periodicTask.Start();
	}

	public override void Stop()
	{
		periodicTask?.Dispose();
	}

	private void UpdateTime()
	{
		LogicObject.GetVariable("Time").Value = DateTime.Now;
		LogicObject.GetVariable("UTCTime").Value = DateTime.UtcNow;
		bool curLocale = LogicObject.GetVariable("CurrentLocale").Value;
		if (curLocale) {
			//Log.Info(LogicObject.GetVariable("CurrentLocale").Value.ToString());
			string newTime = DateTime.Now.ToString("hh:mm tt");
			// if (!newTime.Contains("AM")) {
			// 	newTime += " PM";
			// }
			LogicObject.GetVariable("LocalizedTime").Value = newTime;
			LogicObject.GetVariable("LocalizedDate").Value = DateTime.Now.ToString("MM.dd.yyyy");
		} else {
			LogicObject.GetVariable("LocalizedTime").Value = DateTime.Now.ToString("HH:mm");
			LogicObject.GetVariable("LocalizedDate").Value = DateTime.Now.ToString("dd.MM.yyyy");
		}
		//LogicObject.GetVariable("LocalizedTime").Value = DateTime.Now;
		//LogicObject.GetVariable("LocalizedDate").Value = DateTime.UtcNow;
	}

	private PeriodicTask periodicTask;
}

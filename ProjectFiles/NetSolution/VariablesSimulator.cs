#region Using directives
using System;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using UAManagedCore;
#endregion

public class VariablesSimulator : BaseNetLogic
{

    private PeriodicTask MyTask;
    private int iCounter;
    private double dCounter;

    public override void Start()
    {
        if(!Project.Current.GetVariable("Model/EnableSimulations").Value)
        {
            Log.Info(this.GetType().Name, "Simulation is disabled");
            return;
        }
        MyTask = new PeriodicTask(Simulation, 250, LogicObject);
        iCounter = 0;
        dCounter = 0;
        MyTask.Start();
    }

    public void Simulation()
    {
        bool bRun;
        bRun = LogicObject.GetVariable("bRunSimulation").Value;
        if (bRun)
        {
            if (iCounter <= 99)
            {
                iCounter++;
            }
            else
            {
                iCounter = 0;
            }
            dCounter = dCounter + 0.05;
            LogicObject.GetVariable("iRamp").Value = iCounter;
            LogicObject.GetVariable("iSin").Value = Math.Sin(dCounter) * 100;
            LogicObject.GetVariable("iCos").Value = Math.Cos(dCounter) * 50;
        }

    }

    public override void Stop()
    {
        if (MyTask != null)
        {
            MyTask.Dispose();
            MyTask = null;
        }
    }
}

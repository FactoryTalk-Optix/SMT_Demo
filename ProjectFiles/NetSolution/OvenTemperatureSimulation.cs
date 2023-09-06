#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Alarm;
using FTOptix.NetLogic;
using FTOptix.Core;
using FTOptix.OPCUAServer;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
#endregion

public class OvenTemperatureSimulation : BaseNetLogic
{
    private PeriodicTask TemperatureOscillationTask;
    Int32 TemperatureTarget1;
    Int32 TemperatureTarget2;
    Int32 TemperatureTarget3;
    Int32 TemperatureTarget4;
    Int32 TemperatureTarget5;
    Int32 TemperatureOscillation1 = 0;
    Int32 TemperatureOscillation2 = 0;
    Int32 TemperatureOscillation3 = 0;
    Int32 TemperatureOscillation4 = 0;
    Int32 TemperatureOscillation5 = 0;
    Random rnd = new Random();
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        TemperatureTarget1 = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature1").Value;
        TemperatureTarget2 = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature2").Value;
        TemperatureTarget3 = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature3").Value;
        TemperatureTarget4 = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature4").Value;
        TemperatureTarget5 = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature5").Value;
        TemperatureOscillationTask = new PeriodicTask(OvenTemperatureGenerator, 1000, LogicObject);
        TemperatureOscillationTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        TemperatureOscillationTask?.Dispose();
    }

    private void OvenTemperatureGenerator() {
        var tempTolerance = LogicObject.GetVariable("TemperatureTolerance");
        Int32 tempToleranceMin = tempTolerance.Value * -1;
        Int32 tempToleranceMax = tempTolerance.Value;
        var OvenTempValue = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature1");
        if (OvenTempValue.Value == TemperatureTarget1 + TemperatureOscillation1) {
            TemperatureOscillation1 = rnd.Next(tempToleranceMin, tempToleranceMax);
        } else {
            if (OvenTempValue.Value > TemperatureTarget1 + TemperatureOscillation1) {
                --OvenTempValue.Value;
            } else {
                ++OvenTempValue.Value;
            }
        }
        OvenTempValue = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature2");
        if (OvenTempValue.Value == TemperatureTarget2 + TemperatureOscillation2) {
            TemperatureOscillation2 = rnd.Next(tempToleranceMin, tempToleranceMax);
        } else {
            if (OvenTempValue.Value > TemperatureTarget2 + TemperatureOscillation2) {
                --OvenTempValue.Value;
            } else {
                ++OvenTempValue.Value;
            }
        }
        OvenTempValue = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature3");
        if (OvenTempValue.Value == TemperatureTarget3 + TemperatureOscillation3) {
            TemperatureOscillation3 = rnd.Next(tempToleranceMin, tempToleranceMax);
        } else {
            if (OvenTempValue.Value > TemperatureTarget3 + TemperatureOscillation3) {
                --OvenTempValue.Value;
            } else {
                ++OvenTempValue.Value;
            }
        }
        OvenTempValue = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature4");
        if (OvenTempValue.Value == TemperatureTarget4 + TemperatureOscillation4) {
            TemperatureOscillation4 = rnd.Next(tempToleranceMin, tempToleranceMax);
        } else {
            if (OvenTempValue.Value > TemperatureTarget4 + TemperatureOscillation4) {
                --OvenTempValue.Value;
            } else {
                ++OvenTempValue.Value;
            }
        }
        OvenTempValue = Project.Current.GetVariable("Model/Cycle/LiveData/OvenTemperature5");
        if (OvenTempValue.Value == TemperatureTarget5 + TemperatureOscillation5) {
            TemperatureOscillation5 = rnd.Next(tempToleranceMin, tempToleranceMax);
        } else {
            if (OvenTempValue.Value > TemperatureTarget5 + TemperatureOscillation5) {
                --OvenTempValue.Value;
            } else {
                ++OvenTempValue.Value;
            }
        }
    }
}

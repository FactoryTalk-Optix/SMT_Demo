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
using FTOptix.OPCUAServer;
using FTOptix.Retentivity;
using FTOptix.EventLogger;
using System.Threading;
using FTOptix.Core;
using FftSharp;
using System.Linq;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.WebUI;
#endregion

public class FftLogic : BaseNetLogic
{
    private LongRunningTask squareGenerationTask;
    private LongRunningTask sineGenerationTask;
    private LongRunningTask fftAnalysis;
    float maxVal = 10;
    float noiseLevel = 10;
    bool addNoise = true;
    Int32 dataPoints = 256;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        fftAnalysis = new LongRunningTask(CalculateFftToXY, LogicObject);
        //SquareGen();
    }

    [ExportMethod]
    public void SquareGen() {
        UpdateParams();
        squareGenerationTask = new LongRunningTask(GenerateSquare, LogicObject);
        squareGenerationTask.Start();
        fftAnalysis?.Dispose();
    }

    [ExportMethod]
    public void SineGen() {
        UpdateParams();
        sineGenerationTask = new LongRunningTask(GenerateSine, LogicObject);
        sineGenerationTask.Start();
        fftAnalysis?.Dispose();
    }

    private void UpdateParams() {
        addNoise = !(LogicObject.GetVariable("NoiseFilter").Value);
        maxVal = (float)(LogicObject.GetVariable("Intensity").Value);
        noiseLevel = (float)(LogicObject.GetVariable("Accuracy").Value);
        noiseLevel = 15 - noiseLevel;
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        fftAnalysis?.Dispose();
        sineGenerationTask?.Dispose();
        squareGenerationTask?.Dispose();
    }

    public void InsertData(object[,] values) {
        Log.Info("Inserting " + values.GetLength(0) + " values in DB");
        var myStore = LogicObject.GetVariable("InputData");
        double[,] tempVal = new double[dataPoints,2];
        for (int i = 0; i < dataPoints; i++)
        {
            tempVal[i,0] = i;
            tempVal[i,1] = Convert.ToDouble(values[i, 1]);
        }
        myStore.SetValue(tempVal);
    }

    private static Random random = new Random(DateTime.Now.Millisecond);

    private void GenerateSquare() {
        var myTrend = Owner.Get<XYChart>("Frequency2");
        myTrend.Enabled = false;
        LogicObject.GetVariable("Ready").Value = false;
        UInt64 numberOfPoints = Convert.ToUInt64(Math.Pow(2, 10));
        // Prepare new data
        var values = new object[numberOfPoints, 2];
        double noiseVal = (maxVal / 100.0) * noiseLevel;
        bool toggle = false;
        for (UInt64 i = 0; i < numberOfPoints; i++) {
            if (i % 10 == 0) {
                toggle = !toggle;
            }
            values[i, 0] = DateTime.Now;
            if (toggle) {
                values[i, 1] = (double)maxVal;
            } else {
                values[i, 1] = (double)maxVal * -1.0;
            }
            if (addNoise) {
                if (random.Next(0, 10) == 0) {
                    values[i, 1] = (double)values[i, 1] + (random.NextDouble() * maxVal * noiseVal) * (double)random.Next(-1, +1);
                }
            }
            Thread.Sleep(1);
        }
        InsertData(values);
        Log.Info("Data added, refreshing...");
        //myTrend.XAxis.Time = DateTime.Now;
        myTrend.Refresh();
        myTrend.Enabled = true;
        LogicObject.GetVariable("Ready").Value = true;
        fftAnalysis.Start();
        squareGenerationTask?.Dispose();
    }

    private void GenerateSine() {
        LogicObject.GetVariable("Ready").Value = false;
        var myTrend = Owner.Get<XYChart>("Frequency2");
        myTrend.Enabled = false;
        UInt64 numberOfPoints = Convert.ToUInt64(dataPoints);
        // Prepare new data
        var values = new object[numberOfPoints, 2];
        double noiseVal = (maxVal / 100.0) * noiseLevel;
        double dCounter = 0;
        for (UInt64 i = 0; i < numberOfPoints; i++) {
            dCounter = dCounter + 0.1;
            values[i, 0] = DateTime.Now;
            values[i, 1] = (maxVal * Math.Sin(dCounter));
            if (addNoise) {
                if (random.Next(0, 10) == 0) {
                    values[i, 1] = (double)values[i, 1] + (random.NextDouble() * maxVal * noiseVal) * random.Next(-1, +1);
                }
            }
            Thread.Sleep(1);
        }
        InsertData(values);
        Log.Info("Data added, refreshing...");
        //myTrend.XAxis.Time = DateTime.Now;
        myTrend.Refresh();
        myTrend.Enabled = true;
        LogicObject.GetVariable("Ready").Value = true;
        fftAnalysis.Start();
        sineGenerationTask?.Dispose();
    }

    public double[] GetSignalFromDB() {
        var myStore = LogicObject.GetVariable("InputData");
        double[,] tempVar = new double[dataPoints,2];
        tempVar = (double[,])myStore.Value.Value;
        double[] signal = new double[dataPoints];
        for (int i = 0; i < dataPoints; i++) {
            signal[i] = tempVar[i, 1];
        }
        return signal;
    }
    private double[] CleanData(double[] inputData) {
        double minVal = 0.0;
        double maxVal = 0.0;
        for (int i = 0; i < inputData.Length; i++) {
            if (inputData[i] == Double.PositiveInfinity) {
                inputData[i] = maxVal;
            } else if (inputData[i] == Double.NegativeInfinity) {
                inputData[i] = minVal;
            } else {
                if (inputData[i] > maxVal) {
                    maxVal = inputData[i];
                } else if (inputData[i] < minVal) {
                    minVal = inputData[i];
                }
            }
        }
        return inputData;
    }

    const int sampleRate = 16000;
    // Mathematical output variables
    double MathMinPow;
    double MathMaxPow;
    double MathAvgPow;
    double MathFundamentalFreq;
    double MathMinFreq;
    double MathMaxFreq;

    private void CalculateFftToXY() {
        XYChart myChart = Owner.Get<XYChart>("Frequency1");
        myChart.Enabled = false;
        double[] signal = GetSignalFromDB();
        // calculate the power spectral density using FFT
        double[] psd = Transform.FFTpower(signal);
        double[] freq = Transform.FFTfreq(sampleRate, psd.Length);
        // Create variable with same output size
        double[,] tempData = new double[psd.Length, 2];
        CleanData(psd);
        CleanData(freq);
        for (int i = 0; i < psd.Length; i++) {
            tempData[i, 1] = psd[i] * 100;
            tempData[i, 0] = freq[i];
        }
        var myInput = myChart.GetVariable("Input");
        myInput.SetValue(tempData);
        MathMaxPow = psd.Max() * 100;
        MathMinFreq = freq.Min();
        MathMaxFreq = freq.Max();
        MathMinPow = psd.Min() * 100;
        MathAvgPow = psd.Average() * 100;
        MathFundamentalFreq = freq[psd.ToList().IndexOf(psd.Max())];
        PlotMax(myChart);
        PlotMin(myChart);
        PlotAvg(myChart);
        PlotFundamental(myChart);
        UpdateMathPanel();
        myChart.Enabled= true;
        myChart.CenterX = freq[freq.Length / 2];
        myChart.CenterY = ((psd.Max() - Math.Abs(psd.Min())) / 2) * 100;
        //myChart.Zoom = 0.04;
        fftAnalysis?.Dispose();
    }

    private void PlotMax(XYChart outGraph) {
        var minPen = outGraph.GetVariable("MaxPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = MathMaxPow;
        tempVal[0, 0] = MathMinFreq;
        tempVal[1, 1] = MathMaxPow;
        tempVal[1, 0] = MathMaxFreq;
        minPen.SetValue(tempVal);
    }
    private void PlotMin(XYChart outGraph) {
        var minPen = outGraph.GetVariable("MinPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = MathMinPow;
        tempVal[0, 0] = MathMinFreq;
        tempVal[1, 1] = MathMinPow;
        tempVal[1, 0] = MathMaxFreq;
        minPen.SetValue(tempVal);
    }
    private void PlotAvg(XYChart outGraph) {
        var minPen = outGraph.GetVariable("AvgPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = MathAvgPow;
        tempVal[0, 0] = MathMinFreq;
        tempVal[1, 1] = MathAvgPow;
        tempVal[1, 0] = MathMaxFreq;
        minPen.SetValue(tempVal);
    }
    private void PlotFundamental(XYChart outGraph) {
        var minPen = outGraph.GetVariable("PeakPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = MathMaxPow;
        tempVal[0, 0] = MathFundamentalFreq;
        tempVal[1, 1] = MathMinPow;
        tempVal[1, 0] = MathFundamentalFreq;
        minPen.SetValue(tempVal);
    }
    private void UpdateMathPanel() {
        ColumnLayout dstPanel = Owner.Get<ColumnLayout>("AnalysisResultsList");
        dstPanel.Children.Clear();
        // Add max frequency
        var tempElement = InformationModel.Make<DataElement>("MathMinFreq");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MinFreq");
        tempElement.GetVariable("Value").Value = MathMinFreq;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add min frequency
        tempElement = InformationModel.Make<DataElement>("MathMaxFreq");
        dstPanel.Height += tempElement.Height;
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MaxFreq");;
        tempElement.GetVariable("Value").Value = MathMaxFreq;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add carrier frequency
        tempElement = InformationModel.Make<DataElement>("CarrierFreq");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "CarrierFreq");
        tempElement.GetVariable("Value").Value = MathFundamentalFreq;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add min power
        tempElement = InformationModel.Make<DataElement>("MathMinPow");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MinPower");;
        tempElement.GetVariable("Value").Value = ((float)(MathMinPow/100)).ToString("n2");
        tempElement.GetVariable("Unit").Value = "dB";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add max power
        tempElement = InformationModel.Make<DataElement>("MathMaxPow");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MaxPower");;
        tempElement.GetVariable("Value").Value = ((float)(MathMaxPow/100)).ToString("n2");
        tempElement.GetVariable("Unit").Value = "dB";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add average power
        tempElement = InformationModel.Make<DataElement>("MathAvgPow");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "AvgPower");;
        tempElement.GetVariable("Value").Value = ((float)(MathAvgPow/100)).ToString("n2");
        tempElement.GetVariable("Unit").Value = "dB";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add sample rate
        tempElement = InformationModel.Make<DataElement>("SampleRate");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "SampleRate");;
        tempElement.GetVariable("Value").Value = sampleRate;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
    }
}

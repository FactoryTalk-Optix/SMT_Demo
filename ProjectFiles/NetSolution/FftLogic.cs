#region Using directives
using System;
using System.Linq;
using System.Threading;
using FftSharp;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
#endregion

public class FftLogic : BaseNetLogic
{
    private LongRunningTask squareGenerationTask;
    private LongRunningTask sineGenerationTask;
    private LongRunningTask fftAnalysis;
    private float maxVal = 10;
    private float noiseLevel = 10;
    private bool addNoise = true;
    private readonly int dataPoints = 256;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        fftAnalysis = new LongRunningTask(CalculateFftToXY, LogicObject);
    }

    [ExportMethod]
    public void SquareGen()
    {
        UpdateParams();
        squareGenerationTask = new LongRunningTask(GenerateSquare, LogicObject);
        squareGenerationTask.Start();
        fftAnalysis?.Dispose();
    }

    [ExportMethod]
    public void SineGen()
    {
        UpdateParams();
        sineGenerationTask = new LongRunningTask(GenerateSine, LogicObject);
        sineGenerationTask.Start();
        fftAnalysis?.Dispose();
    }

    private void UpdateParams()
    {
        addNoise = ! LogicObject.GetVariable("NoiseFilter").Value;
        maxVal = (float) LogicObject.GetVariable("Intensity").Value;
        noiseLevel = (float) LogicObject.GetVariable("Accuracy").Value;
        noiseLevel = 15 - noiseLevel;
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        fftAnalysis?.Dispose();
        sineGenerationTask?.Dispose();
        squareGenerationTask?.Dispose();
    }

    private void InsertData(object[,] values)
    {
        Log.Info("Inserting " + values.GetLength(0) + " values in DB");
        var myStore = LogicObject.GetVariable("InputData");
        double[,] tempVal = new double[dataPoints, 2];
        for (int i = 0; i < dataPoints; i++)
        {
            tempVal[i, 0] = i;
            tempVal[i, 1] = Convert.ToDouble(values[i, 1]);
        }
        myStore.SetValue(tempVal);
    }

    private static readonly Random random = new Random(DateTime.Now.Millisecond);

    private void GenerateSquare()
    {
        var myTrend = Owner.Get<XYChart>("Frequency2");
        myTrend.Enabled = false;
        LogicObject.GetVariable("Ready").Value = false;
        UInt64 numberOfPoints = Convert.ToUInt64(Math.Pow(2, 10));
        // Prepare new data
        var values = new object[numberOfPoints, 2];
        double noiseVal = (maxVal / 100.0) * noiseLevel;
        bool toggle = false;
        for (UInt64 i = 0; i < numberOfPoints; i++)
        {
            if (i % 10 == 0)
                toggle = !toggle;

            values[i, 0] = DateTime.Now;

            if (toggle)
                values[i, 1] = (double)maxVal;
            else
                values[i, 1] = (double)maxVal * -1.0;

            if (addNoise && random.Next(0, 10) == 0)
                values[i, 1] = (double)values[i, 1] + (random.NextDouble() * maxVal * noiseVal) * (double)random.Next(-1, +1);

            Thread.Sleep(1);
        }
        InsertData(values);
        Log.Info("Data added, refreshing...");
        myTrend.Refresh();
        myTrend.Enabled = true;
        LogicObject.GetVariable("Ready").Value = true;
        fftAnalysis.Start();
        squareGenerationTask?.Dispose();
    }

    private void GenerateSine()
    {
        LogicObject.GetVariable("Ready").Value = false;
        var myTrend = Owner.Get<XYChart>("Frequency2");
        myTrend.Enabled = false;
        UInt64 numberOfPoints = Convert.ToUInt64(dataPoints);
        // Prepare new data
        var values = new object[numberOfPoints, 2];
        double noiseVal = (maxVal / 100.0) * noiseLevel;
        double dCounter = 0;
        for (UInt64 i = 0; i < numberOfPoints; i++)
        {
            dCounter += 0.1;
            values[i, 0] = DateTime.Now;
            values[i, 1] = maxVal * Math.Sin(dCounter);
            if (addNoise && random.Next(0, 10) == 0)
            {
                values[i, 1] = (double)values[i, 1] + (random.NextDouble() * maxVal * noiseVal) * random.Next(-1, +1);
            }
            Thread.Sleep(1);
        }
        InsertData(values);
        Log.Info("Data added, refreshing...");
        myTrend.Refresh();
        myTrend.Enabled = true;
        LogicObject.GetVariable("Ready").Value = true;
        fftAnalysis.Start();
        sineGenerationTask?.Dispose();
    }

    public double[] GetSignalFromDB()
    {
        var myStore = LogicObject.GetVariable("InputData");
        double[,] tempVar = (double[,])myStore.Value.Value;
        double[] signal = new double[dataPoints];

        for (int i = 0; i < dataPoints; i++)
        {
            signal[i] = tempVar[i, 1];
        }

        return signal;
    }
    private static void CleanData(double[] inputData)
    {
        double minVal = 0.0;
        double maxVal = 0.0;
        for (int i = 0; i < inputData.Length; i++)
        {
            if (double.IsPositiveInfinity(inputData[i]))
                inputData[i] = maxVal;
            else if (double.IsNegativeInfinity(inputData[i]))
                inputData[i] = minVal;
            else
                if (inputData[i] > maxVal)
                    maxVal = inputData[i];
                else if (inputData[i] < minVal)
                    minVal = inputData[i];
        }
    }

    const int SampleRate = 16000;
    // Mathematical output variables
    double mathMinPow;
    double mathMaxPow;
    double mathAvgPow;
    double mathFundamentalFreq;
    double mathMinFreq;
    double mathMaxFreq;

    private void CalculateFftToXY()
    {
        XYChart myChart = Owner.Get<XYChart>("Frequency1");
        myChart.Enabled = false;
        double[] signal = GetSignalFromDB();
        // calculate the power spectral density using FFT
        double[] psd = Transform.FFTpower(signal);
        double[] freq = Transform.FFTfreq(SampleRate, psd.Length);
        // Create variable with same output size
        double[,] tempData = new double[psd.Length, 2];
        CleanData(psd);
        CleanData(freq);
        for (int i = 0; i < psd.Length; i++)
        {
            tempData[i, 1] = psd[i] * 100;
            tempData[i, 0] = freq[i];
        }
        var myInput = myChart.GetVariable("Input");
        myInput.SetValue(tempData);
        mathMaxPow = psd.Max() * 100;
        mathMinFreq = freq.Min();
        mathMaxFreq = freq.Max();
        mathMinPow = psd.Min() * 100;
        mathAvgPow = psd.Average() * 100;
        mathFundamentalFreq = freq[psd.ToList().IndexOf(psd.Max())];
        PlotMax(myChart);
        PlotMin(myChart);
        PlotAvg(myChart);
        PlotFundamental(myChart);
        UpdateMathPanel();
        myChart.Enabled = true;
        myChart.CenterX = freq[freq.Length / 2];
        myChart.CenterY = ((psd.Max() - Math.Abs(psd.Min())) / 2) * 100;
        fftAnalysis?.Dispose();
    }

    private void PlotMax(XYChart outGraph)
    {
        var minPen = outGraph.GetVariable("MaxPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = mathMaxPow;
        tempVal[0, 0] = mathMinFreq;
        tempVal[1, 1] = mathMaxPow;
        tempVal[1, 0] = mathMaxFreq;
        minPen.SetValue(tempVal);
    }
    private void PlotMin(XYChart outGraph)
    {
        var minPen = outGraph.GetVariable("MinPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = mathMinPow;
        tempVal[0, 0] = mathMinFreq;
        tempVal[1, 1] = mathMinPow;
        tempVal[1, 0] = mathMaxFreq;
        minPen.SetValue(tempVal);
    }
    private void PlotAvg(XYChart outGraph)
    {
        var minPen = outGraph.GetVariable("AvgPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = mathAvgPow;
        tempVal[0, 0] = mathMinFreq;
        tempVal[1, 1] = mathAvgPow;
        tempVal[1, 0] = mathMaxFreq;
        minPen.SetValue(tempVal);
    }
    private void PlotFundamental(XYChart outGraph)
    {
        var minPen = outGraph.GetVariable("PeakPen");
        double[,] tempVal = new double[2, 2];
        tempVal[0, 1] = mathMaxPow;
        tempVal[0, 0] = mathFundamentalFreq;
        tempVal[1, 1] = mathMinPow;
        tempVal[1, 0] = mathFundamentalFreq;
        minPen.SetValue(tempVal);
    }
    private void UpdateMathPanel()
    {
        ColumnLayout dstPanel = Owner.Get<ColumnLayout>("AnalysisResultsList");

        if (dstPanel == null)
            return;

        dstPanel.Children.Clear();
        // Add max frequency
        var tempElement = InformationModel.Make<DataElement>("MathMinFreq");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MinFreq");
        tempElement.GetVariable("Value").Value = mathMinFreq;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add min frequency
        tempElement = InformationModel.Make<DataElement>("MathMaxFreq");
        dstPanel.Height += tempElement.Height;
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MaxFreq");
        tempElement.GetVariable("Value").Value = mathMaxFreq;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add carrier frequency
        tempElement = InformationModel.Make<DataElement>("CarrierFreq");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "CarrierFreq");
        tempElement.GetVariable("Value").Value = mathFundamentalFreq;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add min power
        tempElement = InformationModel.Make<DataElement>("MathMinPow");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MinPower");
        tempElement.GetVariable("Value").Value = ((float)(mathMinPow / 100)).ToString("n2");
        tempElement.GetVariable("Unit").Value = "dB";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add max power
        tempElement = InformationModel.Make<DataElement>("MathMaxPow");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "MaxPower");
        tempElement.GetVariable("Value").Value = ((float)(mathMaxPow / 100)).ToString("n2");
        tempElement.GetVariable("Unit").Value = "dB";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add average power
        tempElement = InformationModel.Make<DataElement>("MathAvgPow");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "AvgPower");
        tempElement.GetVariable("Value").Value = ((float)(mathAvgPow / 100)).ToString("n2");
        tempElement.GetVariable("Unit").Value = "dB";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
        // Add sample rate
        tempElement = InformationModel.Make<DataElement>("SampleRate");
        tempElement.GetVariable("Description").Value = new LocalizedText(tempElement.NodeId.NamespaceIndex, "SampleRate");
        tempElement.GetVariable("Value").Value = SampleRate;
        tempElement.GetVariable("Unit").Value = "Hz";
        dstPanel.Add(tempElement);
        dstPanel.Height += tempElement.Height;
    }
}

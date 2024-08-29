#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using FTOptix.Core;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json.Linq;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;

#endregion

public class MQTT_Publisher : BaseNetLogic
{
    public override void Start()
    {
        if (Project.Current.GetVariable("Model/BrokerSettings/EnablePublisher").Value)
        {
            // Import MQTT parameters
            mqttAddress = Project.Current.GetVariable("Model/BrokerSettings/Address").Value;
            mqttPort = Project.Current.GetVariable("Model/BrokerSettings/Port").Value;
            Log.Info("MQTT_Publisher.Start", $"Starting publisher for: \"{Environment.MachineName}\" - Topic: \"ASEM_demo/to_cloud/{Environment.MachineName}\" - Address: {mqttAddress}:{mqttPort}");
            // Start MQTT publisher
            sendPayloadTask = new PeriodicTask(SendPayload, 2000, LogicObject);
            sendPayloadTask.Start();
        }
        else
        {
            Log.Info("MQTT_Publisher", "MQTT Publisher was disabled by Model configuration");
        }
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        sendPayloadTask?.Dispose();
    }

    private async void SendPayload()
    {
        try
        {
            // Get payload from model (assuming it's a JSON string)
            string payload = $"{PayloadFromModel(Project.Current.Get("Model"))}";
            payload = "{" + payload.Substring(0, payload.Length - 1) + "}";

            // Deserialize the payload string into a JObject
            JObject payloadObject = JObject.Parse(payload);

            // Create a new JObject for the modified JSON
            var jsonObject = new JObject
            {
                // Add properties to the JObject
                ["machine"] = Environment.MachineName,
                ["msgType"] = "status",
                ["payload"] = payloadObject
            };

            // Serialize the JObject to a JSON string
            string modifiedJson = jsonObject.ToString();

            // Assuming you have a method to publish the message
            await Publish_Application_Message(mqttAddress, mqttPort, modifiedJson);

            // Update variables or perform other actions
            Project.Current.GetVariable("Model/BrokerSettings/PublisherConnected").Value = true;
            Project.Current.GetVariable("Model/BrokerSettings/ConnectionAvailable").Value = true;

            if (!connStatus)
            {
                connStatus = true;
                Log.Info("MQTT_Publisher.ConnectionStatus", $"Connected to the Broker at {mqttAddress}:{mqttPort}");
            }
        }
        catch (Exception ex)
        {
            Log.Error("MQTT_Publisher.SendPayload", ex.Message);
            connStatus = false;
            failedAttempts++;
            Project.Current.GetVariable("Model/BrokerSettings/PublisherConnected").Value = false;
        }
        if (failedAttempts > 20)
            Project.Current.GetVariable("Model/BrokerSettings/ConnectionAvailable").Value = false;
    }

    private static async Task Publish_Application_Message(string address, int port, string payload)
    {
        Log.Debug("MQTT_Publisher", "Starting MQTT factory");
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        Log.Debug("MQTT_Publisher", "Building MQTT connection");
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(address, port) // Replace with your MQTT broker address
            .WithCommunicationTimeout(TimeSpan.FromMilliseconds(1500)) // Choose a client ID
            .Build();
        Log.Debug("MQTT_Publisher", "Connecting");
        await mqttClient.ConnectAsync(options);
        Log.Debug("MQTT_Publisher", "Building message");
        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"ASEM_demo/SMT") // Replace with the topic you want to publish to
            .WithPayload(payload)
            .WithExactlyOnceQoS()
            .Build();
        Log.Debug("MQTT_Publisher", "Publishing message");
        await mqttClient.PublishAsync(message);
        Log.Debug("MQTT_Publisher", "Disconnecting");
        await mqttClient.DisconnectAsync();
    }

    private string PayloadFromModel(IUANode startingNode, string prefix = "")
    {
        string payload = "";
        foreach (var item in startingNode.Children)
        {
            string fullPath = prefix + "/" + item.BrowseName; // Full path of the current item

            // Add value if variable
            if (item is IUAVariable && ((IUAVariable)item).ArrayDimensions.Length == 0)
            {
                payload += $"\"{fullPath}\": ";
                if (((IUAVariable)item).DataType != OpcUa.DataTypes.String)
                {
                    payload += $"{((IUAVariable)item).Value.Value.ToString().Replace(",", ".").ToLower()},";
                }
                else
                {
                    payload += $"\"{((IUAVariable)item).Value.Value.ToString().Replace(",", ".").ToLower()}\",";
                }
            }
            // Recursively process folders
            else if (item is Folder && item.Children.OfType<IUAVariable>().Any())
            {
                payload += PayloadFromModel(item, fullPath); // Recursively process child items with updated prefix
            }
        }
        // Return payload
        return payload;
    }

    //private string PayloadFromModel(IUANode startingNode)
    //{
    //    string payload = "{";
    //    foreach (var item in startingNode.Children)
    //    {
    //        string lastChar = payload.Substring(payload.Length - 1, 1);
    //        // Add comma if new element of the list
    //        if (((item is IUAVariable && (((IUAVariable)item).ArrayDimensions.Length == 0)) ||
    //            (item is Folder && item.Children.OfType<IUAVariable>().Any())) &&
    //            lastChar == "\"" || lastChar == "}")
    //        {
    //            payload += ",";
    //        }
    //        // Add value if variable or name if folder
    //        if (item is IUAVariable && (((IUAVariable)item).ArrayDimensions.Length == 0))
    //        {
    //            payload += $"\n\"{item.BrowseName}\": \"{((IUAVariable)item).Value.Value.ToString().Replace(",", ".").ToLower()}\"";
    //        }
    //        else if (item is Folder && item.Children.OfType<IUAVariable>().Any())
    //        {
    //            payload += $"\n\"{item.BrowseName}\":\n{PayloadFromModel(item)}";
    //        }
    //    }
    //    // Return payload
    //    return payload += "\n}";
    //}

    private PeriodicTask sendPayloadTask;
    private string mqttAddress;
    private int mqttPort;
    private bool connStatus = false;
    private int failedAttempts = 0;
}

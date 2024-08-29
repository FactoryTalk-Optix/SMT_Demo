#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
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

public class MQTT_Subscriber : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        if (Project.Current.GetVariable("Model/BrokerSettings/EnableSubscriber").Value)
        {
            // Import MQTT parameters
            mqttAddress = Project.Current.GetVariable("Model/BrokerSettings/Address").Value;
            mqttPort = Project.Current.GetVariable("Model/BrokerSettings/Port").Value;
            Log.Info("MQTT_Subscriber.Start", $"Starting subscriber for: \"{Environment.MachineName}\" - Topic: \"ASEM_demo/from_cloud/{Environment.MachineName}\" - Address: {mqttAddress}:{mqttPort}");
            subscriberTask = new LongRunningTask(Main, LogicObject);
            subscriberTask.Start();
        }
        else
        {
            Log.Info("MQTT_Subscriber", "MQTT Subscriber was disabled by Model configuration");
        }
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        subscriberTask?.Dispose();
    }

    private async void Main()
    {
        await SubscribeToTopic(mqttAddress, mqttPort);
    }

    public static async Task SubscribeToTopic(string address, int port)
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.UseDisconnectedHandler(async e =>
        {
            Log.Info("MQTT_Subscriber", "Disconnected from the broker. Trying to reconnect...");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await mqttClient.ConnectAsync(new MqttClientOptionsBuilder()
                    .WithTcpServer(address, port)
                    .WithCommunicationTimeout(TimeSpan.FromSeconds(10)) // Adjust as needed
                    .Build());

                // Resubscribe to the topic after reconnecting
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic($"ASEM_demo/SMT").Build());

                Log.Info("MQTT_Subscriber", "Reconnected and subscribed to topic successfully.");
            }
            catch (Exception ex)
            {
                Log.Error("MQTT_Subscriber", $"Failed to reconnect to the broker: {ex.Message}");
            }
        });

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(address, port)
            .WithCommunicationTimeout(TimeSpan.FromSeconds(10)) // Adjust as needed
            .Build();

        mqttClient.UseApplicationMessageReceivedHandler(e =>
        {
            string payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Log.Debug("MQTT_Subscriber.Incoming", $"Received message from topic \"{e.ApplicationMessage.Topic}\": {payload}");
            // Extract the JSON payload
            JObject jsonObject = JObject.Parse(payload);
            // Extract the MachineName value
            string machineName = ((string)jsonObject["machine"]).ToUpper();
            Log.Verbose1("MQTT_Subscriber.Incoming.MachineName", machineName);
            string commandType = ((string)jsonObject["msgType"]).ToUpper();
            Log.Verbose1("MQTT_Subscriber.Incoming.Type", commandType);
            if (machineName == Environment.MachineName.ToUpper() && commandType == "COMMAND")
            {
                Log.Debug("MQTT_Subscriber.Incoming.Current", $"Received a message for the current machine: {payload}");
                foreach (JProperty property in jsonObject.Descendants().OfType<JProperty>().Where(t => (t.Name != "machine" && t.Name != "msgType")))
                {
                    var targetVar = Project.Current.GetVariable($"Model{property.Name}");
                    if (targetVar != null)
                    {
                        Log.Info("MQTT_Subscriber.Incoming.Set", $"Trying to set \"{targetVar.BrowseName}\" variable to \"{property.Value}\" as received via MQTT");
                        if (targetVar.DataType == OpcUa.DataTypes.Boolean)
                        {
                            if (bool.TryParse(property.Value.ToString(), out bool parsedBool))
                            {
                                targetVar.Value = parsedBool;
                            }
                            else
                            {
                                Log.Warning("MQTT_Subscriber.Incoming.Parse", $"Value \"{property.Value}\" for \"{targetVar.BrowseName}\" cannot be parsed to \"{targetVar.DataType}\"");
                            }
                        }
                        else if (targetVar.DataType == OpcUa.DataTypes.Int32)
                        {
                            if (Int32.TryParse(property.Value.ToString(), out Int32 parsedInt32))
                            {
                                targetVar.Value = parsedInt32;
                            }
                            else
                            {
                                Log.Warning("MQTT_Subscriber.Incoming.Parse", $"Value \"{property.Value}\" for \"{targetVar.BrowseName}\" cannot be parsed to \"{targetVar.DataType}\"");
                            }
                        }
                        else if (targetVar.DataType == OpcUa.DataTypes.Float)
                        {
                            if (float.TryParse(property.Value.ToString(), out float parsedFloat))
                            {
                                targetVar.Value = parsedFloat;
                            }
                            else
                            {
                                Log.Warning("MQTT_Subscriber.Incoming.Parse", $"Value \"{property.Value}\" for \"{targetVar.BrowseName}\" cannot be parsed to \"{targetVar.DataType}\"");
                            }
                        }
                        else
                        {
                            Log.Warning("MQTT_Subscriber.Incoming.Set", $"Datatype {targetVar.DataType.ToString()} is not yet implemented, cannot parse {property.Name}");
                        }
                    }
                    else
                    {
                        Log.Warning("MQTT_Subscriber.Incoming.Parse", $"Cannot find any IUAVariable to match property name \"{property.Name}\"");
                    }
                }
            }
        });

        try
        {
            Log.Info("MQTT_Subscriber", "Connecting to the broker...");
            await mqttClient.ConnectAsync(options);
            Log.Info("MQTT_Subscriber", "Connected to the broker.");

            Log.Info("MQTT_Subscriber", "Subscribing to topic...");
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic($"ASEM_demo/SMT").Build());
            Log.Info("MQTT_Subscriber", "Subscribed to topic successfully.");
        }
        catch (Exception ex)
        {
            Log.Error("MQTT_Subscriber", $"Failed to connect to the broker: {ex.Message}");
        }
    }

    private LongRunningTask subscriberTask;
    private string mqttAddress;
    private int mqttPort;
}

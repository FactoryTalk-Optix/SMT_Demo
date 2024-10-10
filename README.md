# SMT Demo

Demo representing Asem's SMT line based in Artegna (UD) - Italy. The application is meant to be used for tradeshows or events, it is not for training purposes.

Multiple resolutions are available from 4.3" to 21", just change the MainWindow loaded by the NativePresentationEngine

## Simulation

The simulation must be enabled by setting the variable at `SMT_Demo/Model/EnableSimulations`

## MQTT

Both an MQTT publisher and subscriber are included in the application, these must be enabled and configured in the `SMT_Demo/Model/BrokerSettings` object. The supported protocol is only `mqtt://`, support for WebSocket is not implemented.

### Sample publisher payload

```json
{
  "machine": "asem-bm110-00004",
  "msgType": "status",
  "payload": {
    "/Cycle/Start": true,
    "/Cycle/LiveData/OvenTemperature1": 119,
    "/Cycle/LiveData/OvenTemperature2": 172,
    "/Cycle/LiveData/OvenTemperature3": 216,
    "/Cycle/LiveData/OvenTemperature4": 90,
    "/Cycle/LiveData/OvenTemperature5": 27,
    "/Cycle/LiveData/PickPlaceProgress1": 75,
    "/Cycle/LiveData/PickPlaceProgress2": 81,
    "/Cycle/LiveData/PickPlaceProgress3": 72,
    "/Cycle/LiveData/PickPlaceOverall": 76,
    "/Cycle/LiveData/ReelCounter1": 9433,
    "/Cycle/LiveData/ReelCounter2": 9366,
    "/Cycle/LiveData/ReelCounter3": 9453,
    "/Cycle/LiveData/ReelCounter4": 9409,
    "/Statistics/TrendVariable1": 68,
    "/Statistics/TrendVariable2": 87,
    "/Statistics/TrendVariable3": -37,
    "/Alarms/WarningPP1": false,
    "/Alarms/WarningPP2": false,
    "/Alarms/WarningPP3": false,
    "/Alarms/WarningPP4": false,
    "/Alarms/ErrorPP1": false,
    "/Alarms/ErrorPP2": false,
    "/Alarms/ErrorPP3": false,
    "/Alarms/ErrorPP4": false,
    "/Alarms/DemoA/Variable1": false,
    "/Alarms/DemoA/Variable2": false,
    "/Alarms/DemoA/Variable3": false,
    "/Alarms/DemoA/Variable4": true,
    "/Alarms/DemoA/Variable5": false,
    "/Alarms/DemoA/Variable6": true,
    "/Alarms/DemoA/Variable7": true,
    "/Alarms/DemoA/Variable8": true,
    "/Alarms/DemoA/Variable9": false,
    "/Settings/AnalogVariable3": 0.27,
    "/Settings/AnalogVariable2": 0.12,
    "/Settings/AnalogVariable1": 0,
    "/Locale": true,
    "/MenuOpenedAux": false
  }
}
```

### MQTT subscriber payload

A command from an external application can write new values in the Model folder, this is a sample payload to start the production

```json
{
  "machine": "asem-bm110-00004",
  "msgType": "command",
  "payload": {
    "/Cycle/Start": true
	}
}
```

## Disclaimer

Rockwell Automation maintains these repositories as a convenience to you and other users. Although Rockwell Automation reserves the right at any time and for any reason to refuse access to edit or remove content from this Repository, you acknowledge and agree to accept sole responsibility and liability for any Repository content posted, transmitted, downloaded, or used by you. Rockwell Automation has no obligation to monitor or update Repository content

The examples provided are to be used as a reference for building your own application and should not be used in production as-is. It is recommended to adapt the example for the purpose, observing the highest safety standards.

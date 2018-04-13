# LifeSaver IoT Subscriber
This sample utilizes an Azure Function to subscribe to an Event Grid topic in order to invoke a method call on an IoT device to change the
LED light on the IoT DevKit between an active and inactive state. For the full sample to work, you will need an MXChip IoT DevKit.
1. Follow the [Get Started](https://microsoft.github.io/azure-iot-developer-kit/docs/get-started/) example. This project will provision
the appropriate Azure resources needed for the IoT device to be connected to the Azure IoT Hub for bidirectional communication.
2. From the [DevKit State](https://microsoft.github.io/azure-iot-developer-kit/docs/projects/devkit-state/) example, [upload the Arduino
code to the IoT DevKit](https://microsoft.github.io/azure-iot-developer-kit/docs/projects/devkit-state/#uploade-arduino-code-to-devkit).
This code allows the Azure Function to control the LED light via Azure IoT Hub device twins.
3. Create and deploy the Azure Function in the FunctionToIoTDevice project
4. Create the Event Grid event subscription to the appropriate domain topic

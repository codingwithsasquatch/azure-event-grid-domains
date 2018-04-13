# Azure Event Grid School Alert

Demonstrate how to use Event Grid to create an alert system for a school.  The functions and Logic App each have an Event Grid subscription.  For each event, an email is sent and a text is sent.  Depending on the event type (Active Shooter or Resolved), the IOT device is activated or not (representing locking a door, or unlocking a door).

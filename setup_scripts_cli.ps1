######### PARAMETERS #########

$resourceGroup = "AzureEventGridHackathon"

$standardTopic = "hacktopic"
$standardSubscription = "hacksub"

$domain = "auction"
$domainTopics = "ACURA","AUDI","BMW","BUICK","CADILLAC","CHEVROLET","CHRYSLER","DANE","DODGE","FIAT","FORD","FREIGHTLINER","GMC","HONDA","HUMMER","HYUNDAI","INFINITI","JAGUAR","JEEP","KIA","LEXUS","LINCOLN","MAZDA","MERCEDES-BENZ","MERCURY","MITSUBISHI","NISSAN","PONTIAC","RAM","SATURN","SCION","SMART","SUBARU","SUZUKI","TOYOTA","VOLKSWAGEN","VOLVO"

$auctionIdKey = "auctionId"
$makeKey = "make"
$eventTypeKey = ""
$yearKey = "year"

$defaultEventType = "VehicleAvailable"
$defaultSubject = "Unknown"

#After creation of the domain, capture the Domain Resource ID
$domainResourceId = ""
$destinationStorageEndpoint = ""

######### TRADITIONAL TOPICS #########

# Create Standard Topic
az eventgrid topic create -n $standardTopic -l eastus2euap -g $resourceGroup 
  \ --input-mapping-fields id=$auctionIdKey subject=$yearKey
  \ --input-mapping-default-values topic=$standardTopic subject=$defaultSubject dataVersion=1.0

# Create Standard Subscription
az eventgrid event-subscription create --topic-name $standardTopic -g $resourceGroup --name $standardSubscription 
  \ --event-delivery-schema inputeventschema --endpoint-type storagequeue 
  \ --endpoint $destinationStorageEndpoint

# Get Keys
az eventgrid topic key list -n $standardTopic -g $resourceGroup

######### DOMAIN TOPICS #########

# Create Domain
az eventgrid domain create -n $domain -l eastus2euap -g $resourceGroup 
  \ --input-mapping-fields topic=$makeKey id=$auctionIdKey subject=$yearKey 
  \ --input-mapping-default-values subject=$defaultSubject eventType=$defaultEventType dataVersion=1.0

# Create Domain Subscriptions
$domainTopics | foreach {
    az eventgrid event-subscription create 
      \ --resource-id $domainResourceId/topics/$_ 
      \ --name $_.ToLower() --event-delivery-schema inputeventschema --endpoint-type storagequeue 
      \ --endpoint $destinationStorageEndpoint
}

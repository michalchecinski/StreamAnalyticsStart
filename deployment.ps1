$location="westeurope"
$rgName="rg-stream-analytics-start01"

$eventHubNamespaceName="evhn-stream-analytics-start01"

$eventHubName="evh-input"

$eventHubPolicyName="SendPolicy"

az group create -l $location -n $rgName

az eventhubs namespace create --name $eventHubNamespaceName `
                              --resource-group $rgName `
                              --location $location `
                              --sku Standard

az eventhubs eventhub create --name $eventHubName `
                             --namespace-name $eventHubNamespaceName `
                             --resource-group $rgName `
                             --partition-count 2 `
                             --message-retention 1

az eventhubs eventhub authorization-rule create --eventhub-name $eventHubName `
                             --name $eventHubPolicyName `
                             --namespace-name $eventHubNamespaceName `
                             --resource-group $rgName `
                             --rights Send

$authPolicyJson = az eventhubs eventhub authorization-rule keys list --eventhub-name $eventHubName `
                                                   --name $eventHubPolicyName `
                                                   --namespace-name $eventHubNamespaceName `
                                                   --resource-group $rgName

$authPolicy = $authPolicyJson | ConvertFrom-Json

$sendConnString = $authPolicy.primaryConnectionString

$appsettingsJson = "{
    `"EventHubConnectionString`": `"$sendConnString`"
}"

$appsettingsJson | Out-File .\StreamAnalyticsStart\src\EventGenerator\appsettings.json


dotnet run  --project .\StreamAnalyticsStart\src\EventGenerator\EventGenerator.csproj


$eventHubName="evh-output"

$eventHubPolicyName="ReceivePolicy"

$azStorageName="ststreamanalyticsstart01"

az eventhubs eventhub create --name $eventHubName `
                             --namespace-name $eventHubNamespaceName `
                             --resource-group $rgName `
                             --partition-count 2 `
                             --message-retention 1

az eventhubs eventhub authorization-rule create --eventhub-name $eventHubName `
                             --name $eventHubPolicyName `
                             --namespace-name $eventHubNamespaceName `
                             --resource-group $rgName `
                             --rights Listen

$authPolicyJson = az eventhubs eventhub authorization-rule keys list --eventhub-name $eventHubName `
                                                   --name $eventHubPolicyName `
                                                   --namespace-name $eventHubNamespaceName `
                                                   --resource-group $rgName

$authPolicy = $authPolicyJson | ConvertFrom-Json

$sendConnString = $authPolicy.primaryConnectionString

az storage account create -n $azStorageName -g $rgName -l $location `
                            --sku "Standard_LRS"

$azStKeys = az storage account keys list -n $azStorageName | ConvertFrom-Json
$stKey = $azStKeys[0].value

az storage container create -n evh `
                            --account-name $azStorageName `
                            --account-key $stKey

$appsettingsJson = "{
    `"EventHubConnectionString`": `"$sendConnString`",
    `"StorageConnectionString`": `"DefaultEndpointsProtocol=https;AccountName=$azStorageName;AccountKey=$stKey;EndpointSuffix=core.windows.net`",
}"

$appsettingsJson | Out-File .\StreamAnalyticsStart\src\EventReceiverWebApp\appsettings.Development.json
@minLength(1)
@maxLength(64)
@description('Name of the resource group that will contain all the resources')
param resourceGroupName string = resourceGroup().name

@minLength(1)
@description('Primary location for all resources')
param location string = resourceGroup().location

@minLength(3)
@description('Environment for ASP.NET Core. Like "Development", "Production", ..')
param aspnetcoreEnvironment string

param containerRegistryUrl string
param managedIdentityName string
param managedIdentityScope string

param apiserviceContainerImage string
param webfrontendContainerImage string

param deployTimestamp string = utcNow()

var resourceToken = toLower(uniqueString(subscription().id, resourceGroupName, location))

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = {
  name: managedIdentityName
  scope: resourceGroup(managedIdentityScope)
}

// log analytics
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'logs${resourceToken}'
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai${resourceToken}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-11-02-preview' = {
  name: 'acae${resourceToken}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

module apiservice 'containerapp.bicep' = {
  name: 'apiservice-${deployTimestamp}'
  params: {
    location: location
    appName: 'apiservice'
    aspnetcoreEnvironment: aspnetcoreEnvironment
    containerAppsEnvironmentId: containerAppsEnvironment.id
    containerImage: apiserviceContainerImage
    containerRegistryUrl: containerRegistryUrl
    managedIdentityClientId: managedIdentity.properties.clientId
    managedIdentityId: managedIdentity.id
    appIngressAllowInsecure: true
    applicationInsightsConnectionString: applicationInsights.properties.ConnectionString
  }
}

var webfrontendAppName = 'webfrontend'
module webfrontend 'containerapp.bicep' = {
  name: 'webfrontend-${deployTimestamp}'
  params: {
    location: location
    appName: webfrontendAppName
    aspnetcoreEnvironment: aspnetcoreEnvironment
    containerAppsEnvironmentId: containerAppsEnvironment.id
    containerImage: webfrontendContainerImage
    containerRegistryUrl: containerRegistryUrl
    managedIdentityClientId: managedIdentity.properties.clientId
    managedIdentityId: managedIdentity.id
    appIngressExternal: true
    applicationInsightsConnectionString: applicationInsights.properties.ConnectionString
  }
}

output webfrontendUrl string = '${webfrontendAppName}.${containerAppsEnvironment.properties.defaultDomain}'

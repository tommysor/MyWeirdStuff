param location string = resourceGroup().location

param managedIdentityId string
param managedIdentityClientId string

param containerAppsEnvironmentId string
param appName string
param appIngressExternal bool = false
param appIngressAllowInsecure bool = false
param appScaleMinReplicas int = 0
param appScaleMaxReplicas int = 1

param containerRegistryUrl string
param containerImage string
param containerCpu string = '0.25'
param containerMemory string = '0.5Gi'

param aspnetcoreEnvironment string
param applicationInsightsConnectionString string = ''

param environmentVars object[] = [
  
]

var environmentVarsStandard = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: aspnetcoreEnvironment
  }
  {
    name: 'AZURE_CLIENT_ID'
    value: managedIdentityClientId
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: applicationInsightsConnectionString
  }
  {
    name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
    value: 'true'
  }
  {
    name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
    value: 'true'
  }
]

var env = concat(environmentVarsStandard, environmentVars)

resource app 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    environmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: appIngressExternal
        targetPort: 8080
        transport: 'http'
        allowInsecure: appIngressAllowInsecure
      }
      registries: [
        {
          server: containerRegistryUrl
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          image: containerImage
          name: appName
          env: env
          resources: {
            cpu: json(containerCpu)
            memory: containerMemory
          }
          probes: [
            {
              type: 'Startup'
              tcpSocket: {
                port: 8080
              }
              timeoutSeconds: 3
              periodSeconds: 1
              initialDelaySeconds: 3
              successThreshold: 1
              failureThreshold: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                port: 8080
                path: '/health'
                scheme: 'HTTP'
              }
              timeoutSeconds: 5
              periodSeconds: 5
              initialDelaySeconds: 5
              successThreshold: 1
              failureThreshold: 10
            }
            {
              type: 'Liveness'
              httpGet: {
                port: 8080
                path: '/alive'
                scheme: 'HTTP'
              }
              timeoutSeconds: 2
              periodSeconds: 10
              initialDelaySeconds: 5
              successThreshold: 1
              failureThreshold: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: appScaleMinReplicas
        maxReplicas: appScaleMaxReplicas
      }
    }
  }
}

output latestRevisionFqdn string = app.properties.latestRevisionFqdn

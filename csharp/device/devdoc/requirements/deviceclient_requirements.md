﻿# DeviceClient Requirements

## Overview

DeviceClient class allows devices to communicate with an Azure IoTHub.  It can send event messages and receive messages.  The client handles communication with IoTHub through a transport protocol specified.  It uses Http for UWP or PCL and Amqp otherwise. 
 
## References


##Exposed API
```csharp

public sealed class DeviceClient
#if !WINDOWS_UWP && !PCL
    , IDisposable
#endif
{
    DeviceClient(IotHubConnectionString iotHubConnectionString, ITransportSettings[] transportSettings)

    DefaultDelegatingHandler CreateTransportHandler(IotHubConnectionString iotHubConnectionString, ITransportSettings transportSetting)

    public static DeviceClient Create(string hostname, IAuthenticationMethod authenticationMethod)
    public static DeviceClient Create(string hostname, IAuthenticationMethod authenticationMethod, TransportType transportType)
    public static DeviceClient Create(string hostname, IAuthenticationMethod authenticationMethod, [System.Runtime.InteropServices.WindowsRuntime.ReadOnlyArrayAttribute] ITransportSettings[] transportSettings)

    public static DeviceClient CreateFromConnectionString(string connectionString)
    public static DeviceClient CreateFromConnectionString(string connectionString, string deviceId)
    public static DeviceClient CreateFromConnectionString(string connectionString, TransportType transportType)
    public static DeviceClient CreateFromConnectionString(string connectionString, string deviceId, TransportType transportType)
    public static DeviceClient CreateFromConnectionString(string connectionString, [System.Runtime.InteropServices.WindowsRuntime.ReadOnlyArrayAttribute] ITransportSettings[] transportSettings)
    public static DeviceClient CreateFromConnectionString(string connectionString, string deviceId, [System.Runtime.InteropServices.WindowsRuntime.ReadOnlyArrayAttribute] ITransportSettings[] transportSettings)

    public AsyncTask OpenAsync()

    public AsyncTask CloseAsync()

    public AsyncTaskOfMessage ReceiveAsync()
    public AsyncTaskOfMessage ReceiveAsync(TimeSpan timeout)

    public AsyncTask CompleteAsync(string lockToken)
    public AsyncTask CompleteAsync(Message message)

    public AsyncTask AbandonAsync(string lockToken)
    public AsyncTask AbandonAsync(Message message)

    public AsyncTask RejectAsync(string lockToken)
    public AsyncTask RejectAsync(Message message)

    public AsyncTask SendEventAsync(Message message)
    public AsyncTask SendEventBatchAsync(IEnumerable<Message> messages)

    public AsyncTask UploadToBlobAsync(String blobName, System.IO.Stream source)

    public void Dispose()

    public RetryStrategyType RetryStrategy

    public int OperationTimeoutInMilliseconds

    public event ConnectEventHandler OnConnect;

    static ITransportSettings[] PopulateCertificateInTransportSettings(IotHubConnectionStringBuilder connectionStringBuilder, TransportType transportType)
    static ITransportSettings[] PopulateCertificateInTransportSettings(IotHubConnectionStringBuilder connectionStringBuilder, ITransportSettings[] transportSettings)
}
```


### OpenAsync
```csharp
public AsyncTask OpenAsync()
```

**SRS_DEVICECLIENT_28_006: [** The async operation shall retry using retry policy specified in the RetryStrategy property. **]**

**SRS_DEVICECLIENT_28_007: [** The async operation shall retry until time specified in OperationTimeoutInMilliseconds property expire or unrecoverable error occurs. **]**


### CloseAsync
```csharp
public AsyncTask CloseAsync()
```

**SRS_DEVICECLIENT_28_008: [** The async operation shall retry using retry policy specified in the RetryStrategy property. **]**

**SRS_DEVICECLIENT_28_009: [** The async operation shall retry until time specified in OperationTimeoutInMilliseconds property expire or unrecoverable error occurs. **]**


### ReceiveAsync
```csharp
public AsyncTaskOfMessage ReceiveAsync()
public AsyncTaskOfMessage ReceiveAsync(TimeSpan timeout)
```

**SRS_DEVICECLIENT_28_010: [** The async operation shall retry using retry policy specified in the RetryStrategy property. **]**

**SRS_DEVICECLIENT_28_011: [** The async operation shall retry until time specified in OperationTimeoutInMilliseconds property expire or unrecoverable error occurs. **]**


### CompleteAsync
```csharp
public AsyncTask CompleteAsync(string lockToken)
public AsyncTask CompleteAsync(Message message)
```

**SRS_DEVICECLIENT_28_012: [** The async operation shall retry using retry policy specified in the RetryStrategy property. **]**

**SRS_DEVICECLIENT_28_013: [** The async operation shall retry until time specified in OperationTimeoutInMilliseconds property expire or unrecoverable error occurs. **]**


### AbandonAsync
```csharp
public AsyncTask AbandonAsync(string lockToken)
public AsyncTask AbandonAsync(Message message)
```

**SRS_DEVICECLIENT_28_014: [** The async operation shall retry using retry policy specified in the RetryStrategy property. **]**

**SRS_DEVICECLIENT_28_015: [** The async operation shall retry until time specified in OperationTimeoutInMilliseconds property expire or unrecoverable error occurs. **]**


### RejectAsync
```csharp
public AsyncTask RejectAsync(string lockToken)
public AsyncTask RejectAsync(Message message)
```

**SRS_DEVICECLIENT_28_016: [** The async operation shall retry using retry policy specified in the RetryStrategy property. **]**

**SRS_DEVICECLIENT_28_017: [** The async operation shall retry until time specified in OperationTimeoutInMilliseconds property expire or unrecoverable error occurs. **]**


### SendEventAsync
```csharp
public AsyncTask SendEventAsync(Message message)
public AsyncTask SendEventBatchAsync(IEnumerable<Message> messages)
```

**SRS_DEVICECLIENT_28_018: [** The async operation shall retry using retry policy specified in the RetryStrategy property. **]**

**SRS_DEVICECLIENT_28_019: [** The async operation shall retry until time specified in OperationTimeoutInMilliseconds property expire or unrecoverable error occurs. **]**


### RetryStrategy
```csharp
public RetryStrategyType RetryStrategy
```

**SRS_DEVICECLIENT_28_001: [** This property shall be defaulted to the exponential retry strategy with backoff parameters for calculating delay in between retries. **]** 


### OperationTimeoutInMilliseconds
```csharp
public int OperationTimeoutInMilliseconds
```

**SRS_DEVICECLIENT_28_002: [** This property shall be defaulted to 240000 (4 minute). **]**

**SRS_DEVICECLIENT_28_003: [** If this property is set to 0, subsequent operations shall be retried indefinitely until successful or until an unrecoverable error is detected **]**

**SRS_DEVICECLIENT_28_004: [** If setting this property to a negative value, the property setter shall throw an ArgumentException. **]**





# openapi.api.AlertsApi

## Load the API package
```dart
import 'package:openapi/api.dart';
```

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**acknowledgeAlert**](AlertsApi.md#acknowledgealert) | **POST** /api/v1/alerts/{id}/ack | 
[**listAlerts**](AlertsApi.md#listalerts) | **GET** /api/v1/alerts | 
[**registerPushDevice**](AlertsApi.md#registerpushdevice) | **POST** /api/v1/alerts/devices | 
[**revokePushDevice**](AlertsApi.md#revokepushdevice) | **POST** /api/v1/alerts/devices/revoke | 


# **acknowledgeAlert**
> AlertResponse acknowledgeAlert(id)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getAlertsApi();
final String id = 38400000-8cf0-11bd-b23e-10b96e4ef00d; // String | 

try {
    final response = api.acknowledgeAlert(id);
    print(response);
} on DioException catch (e) {
    print('Exception when calling AlertsApi->acknowledgeAlert: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**|  | 

### Return type

[**AlertResponse**](AlertResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **listAlerts**
> BuiltList<AlertResponse> listAlerts(unacknowledgedOnly, limit)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getAlertsApi();
final bool unacknowledgedOnly = true; // bool | 
final int limit = 56; // int | 

try {
    final response = api.listAlerts(unacknowledgedOnly, limit);
    print(response);
} on DioException catch (e) {
    print('Exception when calling AlertsApi->listAlerts: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **unacknowledgedOnly** | **bool**|  | [optional] 
 **limit** | **int**|  | [optional] 

### Return type

[**BuiltList&lt;AlertResponse&gt;**](AlertResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **registerPushDevice**
> RegisterDeviceResponse registerPushDevice(registerDeviceRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getAlertsApi();
final RegisterDeviceRequest registerDeviceRequest = ; // RegisterDeviceRequest | 

try {
    final response = api.registerPushDevice(registerDeviceRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling AlertsApi->registerPushDevice: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **registerDeviceRequest** | [**RegisterDeviceRequest**](RegisterDeviceRequest.md)|  | 

### Return type

[**RegisterDeviceResponse**](RegisterDeviceResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **revokePushDevice**
> revokePushDevice(registerDeviceRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getAlertsApi();
final RegisterDeviceRequest registerDeviceRequest = ; // RegisterDeviceRequest | 

try {
    api.revokePushDevice(registerDeviceRequest);
} on DioException catch (e) {
    print('Exception when calling AlertsApi->revokePushDevice: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **registerDeviceRequest** | [**RegisterDeviceRequest**](RegisterDeviceRequest.md)|  | 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: Not defined

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


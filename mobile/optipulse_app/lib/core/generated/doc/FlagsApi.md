# openapi.api.FlagsApi

## Load the API package
```dart
import 'package:openapi/api.dart';
```

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**changeFlagStatus**](FlagsApi.md#changeflagstatus) | **POST** /api/v1/flags/{key}/status | 
[**createFlag**](FlagsApi.md#createflag) | **POST** /api/v1/flags | 
[**getFlag**](FlagsApi.md#getflag) | **GET** /api/v1/flags/{key} | 
[**listFlags**](FlagsApi.md#listflags) | **GET** /api/v1/flags | 
[**setKillSwitch**](FlagsApi.md#setkillswitch) | **POST** /api/v1/flags/{key}/kill-switch | 
[**updateFlag**](FlagsApi.md#updateflag) | **PUT** /api/v1/flags/{key} | 


# **changeFlagStatus**
> FlagResponse changeFlagStatus(key, changeStatusRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getFlagsApi();
final String key = key_example; // String | 
final ChangeStatusRequest changeStatusRequest = ; // ChangeStatusRequest | 

try {
    final response = api.changeFlagStatus(key, changeStatusRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling FlagsApi->changeFlagStatus: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **key** | **String**|  | 
 **changeStatusRequest** | [**ChangeStatusRequest**](ChangeStatusRequest.md)|  | 

### Return type

[**FlagResponse**](FlagResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **createFlag**
> FlagResponse createFlag(createFlagRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getFlagsApi();
final CreateFlagRequest createFlagRequest = ; // CreateFlagRequest | 

try {
    final response = api.createFlag(createFlagRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling FlagsApi->createFlag: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **createFlagRequest** | [**CreateFlagRequest**](CreateFlagRequest.md)|  | 

### Return type

[**FlagResponse**](FlagResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **getFlag**
> FlagResponse getFlag(key)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getFlagsApi();
final String key = key_example; // String | 

try {
    final response = api.getFlag(key);
    print(response);
} on DioException catch (e) {
    print('Exception when calling FlagsApi->getFlag: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **key** | **String**|  | 

### Return type

[**FlagResponse**](FlagResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **listFlags**
> BuiltList<FlagResponse> listFlags()



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getFlagsApi();

try {
    final response = api.listFlags();
    print(response);
} on DioException catch (e) {
    print('Exception when calling FlagsApi->listFlags: $e\n');
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**BuiltList&lt;FlagResponse&gt;**](FlagResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **setKillSwitch**
> FlagResponse setKillSwitch(key, killSwitchRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getFlagsApi();
final String key = key_example; // String | 
final KillSwitchRequest killSwitchRequest = ; // KillSwitchRequest | 

try {
    final response = api.setKillSwitch(key, killSwitchRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling FlagsApi->setKillSwitch: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **key** | **String**|  | 
 **killSwitchRequest** | [**KillSwitchRequest**](KillSwitchRequest.md)|  | 

### Return type

[**FlagResponse**](FlagResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **updateFlag**
> FlagResponse updateFlag(key, updateFlagRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getFlagsApi();
final String key = key_example; // String | 
final UpdateFlagRequest updateFlagRequest = ; // UpdateFlagRequest | 

try {
    final response = api.updateFlag(key, updateFlagRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling FlagsApi->updateFlag: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **key** | **String**|  | 
 **updateFlagRequest** | [**UpdateFlagRequest**](UpdateFlagRequest.md)|  | 

### Return type

[**FlagResponse**](FlagResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


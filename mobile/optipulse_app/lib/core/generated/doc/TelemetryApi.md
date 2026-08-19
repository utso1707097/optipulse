# openapi.api.TelemetryApi

## Load the API package
```dart
import 'package:openapi/api.dart';
```

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**getFlagExposures**](TelemetryApi.md#getflagexposures) | **GET** /api/v1/telemetry/flags/{key}/exposures | 
[**recordConversion**](TelemetryApi.md#recordconversion) | **POST** /api/v1/telemetry/conversions | 


# **getFlagExposures**
> FlagExposureResponse getFlagExposures(key)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getTelemetryApi();
final String key = key_example; // String | 

try {
    final response = api.getFlagExposures(key);
    print(response);
} on DioException catch (e) {
    print('Exception when calling TelemetryApi->getFlagExposures: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **key** | **String**|  | 

### Return type

[**FlagExposureResponse**](FlagExposureResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **recordConversion**
> ConversionResponse recordConversion(conversionRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getTelemetryApi();
final ConversionRequest conversionRequest = ; // ConversionRequest | 

try {
    final response = api.recordConversion(conversionRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling TelemetryApi->recordConversion: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **conversionRequest** | [**ConversionRequest**](ConversionRequest.md)|  | 

### Return type

[**ConversionResponse**](ConversionResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


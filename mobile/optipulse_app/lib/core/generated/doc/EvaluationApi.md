# openapi.api.EvaluationApi

## Load the API package
```dart
import 'package:openapi/api.dart';
```

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**evaluate**](EvaluationApi.md#evaluate) | **POST** /api/v1/evaluate | 
[**evaluateBatch**](EvaluationApi.md#evaluatebatch) | **POST** /api/v1/evaluate/batch | 
[**getSnapshotVersion**](EvaluationApi.md#getsnapshotversion) | **GET** /api/v1/snapshot/version | 


# **evaluate**
> EvaluateResponse evaluate(evaluateRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getEvaluationApi();
final EvaluateRequest evaluateRequest = ; // EvaluateRequest | 

try {
    final response = api.evaluate(evaluateRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling EvaluationApi->evaluate: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **evaluateRequest** | [**EvaluateRequest**](EvaluateRequest.md)|  | 

### Return type

[**EvaluateResponse**](EvaluateResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **evaluateBatch**
> BatchEvaluateResponse evaluateBatch(batchEvaluateRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getEvaluationApi();
final BatchEvaluateRequest batchEvaluateRequest = ; // BatchEvaluateRequest | 

try {
    final response = api.evaluateBatch(batchEvaluateRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling EvaluationApi->evaluateBatch: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **batchEvaluateRequest** | [**BatchEvaluateRequest**](BatchEvaluateRequest.md)|  | 

### Return type

[**BatchEvaluateResponse**](BatchEvaluateResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **getSnapshotVersion**
> SnapshotVersionResponse getSnapshotVersion()



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getEvaluationApi();

try {
    final response = api.getSnapshotVersion();
    print(response);
} on DioException catch (e) {
    print('Exception when calling EvaluationApi->getSnapshotVersion: $e\n');
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**SnapshotVersionResponse**](SnapshotVersionResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


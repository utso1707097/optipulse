# openapi.api.ExperimentsApi

## Load the API package
```dart
import 'package:openapi/api.dart';
```

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**changeExperimentStatus**](ExperimentsApi.md#changeexperimentstatus) | **POST** /api/v1/experiments/{id}/status | 
[**createExperiment**](ExperimentsApi.md#createexperiment) | **POST** /api/v1/experiments | 
[**getExperiment**](ExperimentsApi.md#getexperiment) | **GET** /api/v1/experiments/{id} | 
[**listExperiments**](ExperimentsApi.md#listexperiments) | **GET** /api/v1/experiments | 
[**updateExperiment**](ExperimentsApi.md#updateexperiment) | **PUT** /api/v1/experiments/{id} | 


# **changeExperimentStatus**
> ExperimentResponse changeExperimentStatus(id, changeStatusRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getExperimentsApi();
final String id = 38400000-8cf0-11bd-b23e-10b96e4ef00d; // String | 
final ChangeStatusRequest changeStatusRequest = ; // ChangeStatusRequest | 

try {
    final response = api.changeExperimentStatus(id, changeStatusRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling ExperimentsApi->changeExperimentStatus: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**|  | 
 **changeStatusRequest** | [**ChangeStatusRequest**](ChangeStatusRequest.md)|  | 

### Return type

[**ExperimentResponse**](ExperimentResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **createExperiment**
> ExperimentResponse createExperiment(createExperimentRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getExperimentsApi();
final CreateExperimentRequest createExperimentRequest = ; // CreateExperimentRequest | 

try {
    final response = api.createExperiment(createExperimentRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling ExperimentsApi->createExperiment: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **createExperimentRequest** | [**CreateExperimentRequest**](CreateExperimentRequest.md)|  | 

### Return type

[**ExperimentResponse**](ExperimentResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **getExperiment**
> ExperimentResponse getExperiment(id)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getExperimentsApi();
final String id = 38400000-8cf0-11bd-b23e-10b96e4ef00d; // String | 

try {
    final response = api.getExperiment(id);
    print(response);
} on DioException catch (e) {
    print('Exception when calling ExperimentsApi->getExperiment: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**|  | 

### Return type

[**ExperimentResponse**](ExperimentResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **listExperiments**
> BuiltList<ExperimentResponse> listExperiments(flagKey)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getExperimentsApi();
final String flagKey = flagKey_example; // String | 

try {
    final response = api.listExperiments(flagKey);
    print(response);
} on DioException catch (e) {
    print('Exception when calling ExperimentsApi->listExperiments: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **flagKey** | **String**|  | [optional] 

### Return type

[**BuiltList&lt;ExperimentResponse&gt;**](ExperimentResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **updateExperiment**
> ExperimentResponse updateExperiment(id, updateExperimentRequest)



### Example
```dart
import 'package:openapi/api.dart';

final api = Openapi().getExperimentsApi();
final String id = 38400000-8cf0-11bd-b23e-10b96e4ef00d; // String | 
final UpdateExperimentRequest updateExperimentRequest = ; // UpdateExperimentRequest | 

try {
    final response = api.updateExperiment(id, updateExperimentRequest);
    print(response);
} on DioException catch (e) {
    print('Exception when calling ExperimentsApi->updateExperiment: $e\n');
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**|  | 
 **updateExperimentRequest** | [**UpdateExperimentRequest**](UpdateExperimentRequest.md)|  | 

### Return type

[**ExperimentResponse**](ExperimentResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json, application/problem+json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


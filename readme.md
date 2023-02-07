# ServiceControl exporter
This tool provides prometheus style metrics for information provided by the ServiceControl api. This is useful if you want an existing monitoring/observability stack to include metrics that only ServiceControl can provide. Careful, this tool uses internal undocumented api calls to the ServiceControl api. So there is no guarante that this will continue to work.

## Available metrics

| Metric | Description | Source |
| ------ | ------ | ---- |
| ErrorMessagesPerEndpoint_Messages | Contains the messages that were sent to the error queue and retrieved by ServiceControl for further review. The messages are grouped by the target endpoint name. Metric is the amount of messages that are ready to be reviewed in ServicePulse.| api/recoverability/groups/Endpoint%20Name |
| ErrorMessagesPerType_Messages | Contains the messages that were sent to the error queue and retrieved by ServiceControl for further review. The messages are grouped by the message type. Metric is the amount of messages that are ready to be reviewed in ServicePulse. | api/recoverability/groups/Message%20Type |

## Metrics endpoint
The tool will expose a `/metrics` endpoint on its configured base url. The default is `http://localhost:5002`.

### Sample output
```text
# TYPE ErrorMessagesPerEndpoint_Messages gauge
# UNIT ErrorMessagesPerEndpoint_Messages Messages
# HELP ErrorMessagesPerEndpoint_Messages Shows the amount of error messages per endpoint that require manual review.
ErrorMessagesPerEndpoint_Messages{endpoint="SomeEndpoint"} 3 1675784613469
ErrorMessagesPerEndpoint_Messages{endpoint="SomeOtherendpoint"} 1 1675784613469

# TYPE ErrorMessagesPerType_Messages gauge
# UNIT ErrorMessagesPerType_Messages Messages
# HELP ErrorMessagesPerType_Messages Shows the amount of error messages per message type that require manual review.
ErrorMessagesPerType_Messages{endpoint="Some.Namespace.SomeCommand"} 3 1675784613469
ErrorMessagesPerType_Messages{endpoint="Some.Other.Namespace.SomeOtherCommand"} 1 1675784613469

# EOF
```

## Limitations
- Currently only unauthenticated calls to ServiceControl are supported.
- Authentication for the provided metrics endpoint is not implemented.
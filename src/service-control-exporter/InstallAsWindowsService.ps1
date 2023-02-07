sc.exe create "ServiceControlExporter" binpath="$(pwd)/service-control-exporter.exe"
# sc.exe failure "ServiceControlExporter" reset=0 actions=restart/60000/restart/60000/run/1000
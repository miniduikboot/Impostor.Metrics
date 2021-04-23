# Impostor.Metrics

This plugin exports data to Prometheus. A Grafana dashboard is included.



To set this up, you need to add the following to your server's config:

For windows:

```json
"PluginLoader": {
    "LibraryPaths": [
      "C:\\Program Files\\dotnet\\shared\\Microsoft.AspNetCore.App\\5.0.1"
    ]
  }
```

For linux:

```json
"PluginLoader": {
    "LibraryPaths": [
      "/opt/dotnet_core/shared/Microsoft.AspNetCore.App/5.0.5/"
    ]
  }
```

Then, just set up Prometheus to query on port 8080.


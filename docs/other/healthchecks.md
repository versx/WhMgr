# Health Checks UI & API  

Application health checks user interface and API.  

**Available Endpoints**:  

- http://ip:port/health  
- http://ip:port/health-ui  

### Health Checks API Example  
```json
{
	"status": "Healthy",
	"totalDuration": "00:00:00.0283632",
	"entries": {
		"Subscriptions Database": {
			"data": {},
			"duration": "00:00:00.0082672",
			"status": "Healthy",
			"tags": []
		},
		"Scanner Database": {
			"data": {},
			"duration": "00:00:00.0092902",
			"status": "Healthy",
			"tags": []
		},
		"Nests Database": {
			"data": {},
			"duration": "00:00:00.0077391",
			"status": "Healthy",
			"tags": []
		},
		"Process": {
			"data": {},
			"duration": "00:00:00.0132519",
			"status": "Healthy",
			"tags": []
		},
		"Allocated Memory": {
			"data": {},
			"description": "Allocated megabytes in memory: 160 mb",
			"duration": "00:00:00.0001172",
			"status": "Healthy",
			"tags": []
		},
		"Local Disk Storage": {
			"data": {},
			"duration": "00:00:00.0010851",
			"status": "Healthy",
			"tags": []
		},
		"Discord Status": {
			"data": {},
			"duration": "00:00:00.0281373",
			"status": "Healthy",
			"tags": []
		}
	}
}
```
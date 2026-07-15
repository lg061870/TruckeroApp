# FreightBidSimulator API Documentation

## Overview

The FreightBidSimulator API allows you to control the simulation of driver bids for freight requests during development and testing. This API is intended to be used in development environments only.

## API Endpoints

### Start Simulator

Starts generating simulated bids for a freight request.

```
POST /api/diagnostics/freight-bid-simulator/start
```

#### Request Body

```json
{
  "freightRequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"  // Optional. If not provided, uses most recent freight request
}
```

#### Response

```json
{
  "message": "Simulator started for freight request 3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Status Codes

- `200 OK`: Simulator started successfully
- `500 Internal Server Error`: An error occurred starting the simulator

### Get Simulated Bids

Retrieves all simulated bids or bids for a specific freight request.

```
GET /api/diagnostics/freight-bid-simulator/bids
```

#### Query Parameters

- `freightRequestId` (optional): Filter bids by freight request ID

#### Response

```json
[
  {
    "bidId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "driverId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "driverName": "John Smith",
    "driverAvatarUrl": "https://ui-avatars.com/api/?name=John+Smith",
    "priceTotal": 99.99,
    "priceBase": 79.99,
    "serviceFee": 20.00,
    "tax": 0,
    "ratingCount": 45,
    "rating": 4.7,
    "distanceMiles": 5.3,
    "estimatedArrivalMinutes": 22,
    "vehicleType": "Box Truck",
    "category": "Express",
    "size": "Medium",
    "help": "Loading & Unloading",
    "comment": "I can help you with this delivery!",
    "createdAt": "2023-06-01T12:34:56Z"
  }
  // ... more bids
]
```

#### Status Codes

- `200 OK`: Retrieved bids successfully
- `500 Internal Server Error`: An error occurred retrieving the bids

### Clear Simulated Bids

Clears all simulated bids from memory.

```
DELETE /api/diagnostics/freight-bid-simulator/bids
```

#### Response

```json
{
  "message": "All simulated bids cleared"
}
```

#### Status Codes

- `200 OK`: Bids cleared successfully
- `500 Internal Server Error`: An error occurred clearing the bids

## Data Models

### BidDetailsDto

Properties representing a driver bid.

| Property | Type | Description |
| -------- | ---- | ----------- |
| bidId | guid | Unique identifier for the bid |
| driverId | guid | ID of the driver who placed the bid |
| driverName | string | Name of the driver |
| driverAvatarUrl | string | URL to the driver's avatar image |
| priceTotal | decimal | Total price of the bid |
| priceBase | decimal | Base price of the bid |
| serviceFee | decimal | Service fee portion of the price |
| tax | decimal | Tax portion of the price |
| ratingCount | integer | Number of ratings for the driver |
| rating | double | Average rating (1.0-5.0) of the driver |
| distanceMiles | double | Distance in miles from the driver to the pickup location |
| estimatedArrivalMinutes | integer | Estimated time in minutes until the driver can arrive |
| vehicleType | string | Type of vehicle (e.g., "Box Truck", "Pickup Truck") |
| category | string | Category of service (e.g., "Express", "Standard") |
| size | string | Size of the vehicle (e.g., "Small", "Medium", "Large") |
| help | string | Type of help offered (e.g., "Loading Only", "Loading & Unloading") |
| comment | string | Comment from the driver about the bid |
| createdAt | datetime | When the bid was created |
# Currency Trend Converter

Full-stack currency converter that pairs a React frontend with a .NET backend. The backend calls exchangerate.host to provide the latest rate and a 7-day historical trend, and the frontend surfaces those insights, including a chart of the past week.

## Backend (ASP.NET Core Web API)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Environment variable overrides (optional): `ExchangeRateApi:BaseUrl`, `ExchangeRateApi:CacheDurationSeconds`, `ExchangeRateApi:TimeoutSeconds`

### Run locally
```bash
cd backend
dotnet restore
dotnet run
```

By default the API listens on `http://localhost:5000`. The following endpoints are available:

| Endpoint | Description |
| --- | --- |
| `GET /api/rates/latest` | Query string: `baseCurrency`, `targetCurrency`, `amount`. Returns the latest rate and converted figure. |
| `GET /api/rates/history` | Query string: `baseCurrency`, `targetCurrency`, `days` (1-30). Returns the 7-day time series that the frontend charts. |
| `GET /api/rates/currencies` | Returns all supported currencies for the dropdowns. |

## Frontend (Create React App)

### Prerequisites
- Node.js 20+ / npm 10+

### Run locally
```bash
cd frontend
npm install
npm start
```

The app uses `REACT_APP_API_BASE_URL` (defaults to `http://localhost:5000`) to reach the backend. Adjust the `.env` file or environment variables if the API runs elsewhere.

## Notes
- The backend caches responses for 5 minutes so repeated requests are fast.
- The frontend shows the historical trend using Chart.js and keeps the UX responsive on desktop/mobile.

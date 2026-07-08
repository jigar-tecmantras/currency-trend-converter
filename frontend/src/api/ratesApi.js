const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || "http://localhost:5000";

async function handleResponse(response) {
  const text = await response.text();
  if (!response.ok) {
    throw new Error(text || "Unable to reach backend service.");
  }
  return text ? JSON.parse(text) : null;
}

export async function fetchCurrencies() {
  const response = await fetch(`${API_BASE_URL}/api/rates/currencies`);
  return handleResponse(response);
}

export async function fetchLatestRate(baseCurrency, targetCurrency, amount) {
  const params = new URLSearchParams({
    baseCurrency,
    targetCurrency,
    amount: amount.toString()
  });
  const response = await fetch(`${API_BASE_URL}/api/rates/latest?${params.toString()}`);
  return handleResponse(response);
}

export async function fetchHistoricalRates(baseCurrency, targetCurrency, days = 7) {
  const params = new URLSearchParams({
    baseCurrency,
    targetCurrency,
    days: days.toString()
  });
  const response = await fetch(`${API_BASE_URL}/api/rates/history?${params.toString()}`);
  return handleResponse(response);
}

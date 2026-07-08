import { useEffect, useMemo, useState } from "react";
import TrendChart from "./components/TrendChart";
import RateSummary from "./components/RateSummary";
import {
  fetchCurrencies,
  fetchHistoricalRates,
  fetchLatestRate
} from "./api/ratesApi";
import "./App.css";

const DEFAULT_CURRENCIES = ["USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "INR"];

function App() {
  const [currencies, setCurrencies] = useState(DEFAULT_CURRENCIES);
  const [baseCurrency, setBaseCurrency] = useState("USD");
  const [targetCurrency, setTargetCurrency] = useState("EUR");
  const [amount, setAmount] = useState("1");
  const [latestRate, setLatestRate] = useState(null);
  const [history, setHistory] = useState([]);
  const [status, setStatus] = useState({ loading: false, error: null });
  const [historyStatus, setHistoryStatus] = useState({ loading: false, error: null });

  useEffect(() => {
    const loadCurrencies = async () => {
      try {
        const data = await fetchCurrencies();
        if (Array.isArray(data) && data.length > 0) {
          setCurrencies(data.sort());
        }
      } catch (error) {
        console.warn("Failed to load currencies", error);
      }
    };

    loadCurrencies();
  }, []);

  useEffect(() => {
    fetchLatest();
    fetchHistory();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [baseCurrency, targetCurrency]);

  const fetchLatest = async () => {
    setStatus({ loading: true, error: null });
    try {
      const numericAmount = Number(amount) || 1;
      const data = await fetchLatestRate(baseCurrency, targetCurrency, numericAmount);
      setLatestRate(data);
      setStatus({ loading: false, error: null });
    } catch (error) {
      setStatus({ loading: false, error: error instanceof Error ? error.message : "Unable to reach API" });
    }
  };

  const fetchHistory = async () => {
    setHistoryStatus({ loading: true, error: null });
    try {
      const data = await fetchHistoricalRates(baseCurrency, targetCurrency, 7);
      if (data?.rates) {
        setHistory(data.rates);
      }
      setHistoryStatus({ loading: false, error: null });
    } catch (error) {
      setHistoryStatus({ loading: false, error: error instanceof Error ? error.message : "Unable to reach API" });
    }
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    fetchLatest();
  };

  const handleSwap = () => {
    setBaseCurrency(targetCurrency);
    setTargetCurrency(baseCurrency);
  };

  const trendSubtitle = useMemo(() => {
    if (!history || history.length === 0) {
      return "Loading trend...";
    }
    const firstDate = new Date(history[0].date).toLocaleDateString();
    const lastDate = new Date(history[history.length - 1].date).toLocaleDateString();
    return `Showing ${history.length} days from ${firstDate} to ${lastDate}`;
  }, [history]);

  return (
    <div className="app-shell">
      <header className="app-header">
        <p className="eyebrow">Live data & historical context</p>
        <h1>Currency Trend Converter</h1>
        <p className="subtitle">
          Compare the latest rate, forecast value, and a 7-day trend so you can make confident international
          decisions.
        </p>
      </header>

      <main>
        <section className="converter-card">
          <div className="card-header">
            <div>
              <p className="eyebrow">Instant conversion</p>
              <h2>Convert currencies</h2>
            </div>
            <button type="button" className="ghost-button" onClick={handleSwap}>
              Swap currencies
            </button>
          </div>

          <form className="converter-form" onSubmit={handleSubmit}>
            <label>
              Base currency
              <select value={baseCurrency} onChange={(event) => setBaseCurrency(event.target.value)}>
                {currencies.map((currency) => (
                  <option key={currency} value={currency}>
                    {currency}
                  </option>
                ))}
              </select>
            </label>

            <label>
              Target currency
              <select value={targetCurrency} onChange={(event) => setTargetCurrency(event.target.value)}>
                {currencies.map((currency) => (
                  <option key={currency} value={currency}>
                    {currency}
                  </option>
                ))}
              </select>
            </label>

            <label>
              Amount
              <input
                value={amount}
                onChange={(event) => setAmount(event.target.value)}
                type="number"
                min="0"
                step="0.01"
              />
            </label>

            <button type="submit" className="cta" disabled={status.loading}>
              {status.loading ? "Updating…" : "Convert"}
            </button>
          </form>

          {status.error && <p className="error">{status.error}</p>}
          {latestRate && <RateSummary latestRate={latestRate} />}
        </section>

        <section className="trend-card">
          <div className="card-header">
            <div>
              <p className="eyebrow">Trend</p>
              <h2>Last 7 days</h2>
            </div>
            <p className="trend-subtitle">{trendSubtitle}</p>
          </div>

          <div className="trend-content">
            {historyStatus.error && <p className="error">{historyStatus.error}</p>}
            <TrendChart history={history} />
          </div>

          <div className="history-list">
            {history.slice().reverse().map((entry) => (
              <div key={entry.date} className="history-row">
                <span>{new Date(entry.date).toLocaleDateString()}</span>
                <strong>{entry.rate.toFixed(6)}</strong>
              </div>
            ))}
          </div>
        </section>
      </main>
    </div>
  );
}

export default App;

import React from "react";

const RateSummary = ({ latestRate }) => {
  if (!latestRate) {
    return null;
  }

  const formattedAmount = Number(latestRate.amount).toLocaleString(undefined, { maximumFractionDigits: 2 });
  const formattedConverted = Number(latestRate.convertedAmount).toLocaleString(undefined, { maximumFractionDigits: 4 });
  const formattedRate = Number(latestRate.rate).toLocaleString(undefined, { maximumFractionDigits: 6 });
  const asOf = new Date(latestRate.asOf).toLocaleDateString();

  return (
    <div className="rate-summary">
      <div>
        <strong>Latest conversion</strong>
        <p>
          {formattedAmount} {latestRate.baseCurrency} → {formattedConverted} {latestRate.targetCurrency}
        </p>
      </div>
      <div>
        <small>1 {latestRate.baseCurrency} = {formattedRate} {latestRate.targetCurrency}</small>
        <br />
        <small>Rates as of {asOf}</small>
      </div>
    </div>
  );
};

export default RateSummary;

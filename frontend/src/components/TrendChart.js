import React from "react";
import "chart.js/auto";
import { Line } from "react-chartjs-2";

const TrendChart = ({ history }) => {
  if (!history || history.length === 0) {
    return <p className="trend-placeholder">No historical trend available.</p>;
  }

  const labels = history.map((entry) => new Date(entry.date).toLocaleDateString());
  const dataPoints = history.map((entry) => entry.rate);

  const data = {
    labels,
    datasets: [
      {
        label: "Rate",
        data: dataPoints,
        borderColor: "#5b78ff",
        backgroundColor: "rgba(91, 120, 255, 0.2)",
        fill: true,
        tension: 0.35,
        pointRadius: 4,
        pointHoverRadius: 6
      }
    ]
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        ticks: {
          precision: 4
        }
      }
    },
    plugins: {
      legend: {
        display: false
      }
    }
  };

  return (
    <div className="trend-chart">
      <Line data={data} options={options} />
    </div>
  );
};

export default TrendChart;

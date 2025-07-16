import React from 'react';
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import {
  Bar,
  BarChart,
  CartesianGrid,
  XAxis,
  YAxis,
  Line,
  LineChart,
  Area,
  AreaChart
} from "recharts";
import './style.css';

// Sample data for the charts
const userGrowthData = [
  { month: "Jan", users: 186 },
  { month: "Feb", users: 305 },
  { month: "Mar", users: 237 },
  { month: "Apr", users: 273 },
  { month: "May", users: 209 },
  { month: "Jun", users: 325 },
];

const portfolioData = [
  { month: "Jan", published: 12, draft: 8 },
  { month: "Feb", published: 19, draft: 12 },
  { month: "Mar", published: 15, draft: 6 },
  { month: "Apr", published: 22, draft: 9 },
  { month: "May", published: 18, draft: 4 },
  { month: "Jun", published: 28, draft: 7 },
];

const revenueData = [
  { month: "Jan", revenue: 4500 },
  { month: "Feb", revenue: 5200 },
  { month: "Mar", revenue: 4800 },
  { month: "Apr", revenue: 6100 },
  { month: "May", revenue: 5800 },
  { month: "Jun", revenue: 7200 },
];

const chartConfig = {
  users: {
    label: "Users",
    color: "#3b82f6",
  },
  published: {
    label: "Published",
    color: "#10b981",
  },
  draft: {
    label: "Draft",
    color: "#f59e0b",
  },
  revenue: {
    label: "Revenue",
    color: "#8b5cf6",
  },
} satisfies ChartConfig;

const AdminCharts: React.FC = () => {
  return (
    <div className="charts-section">
      <div className="charts-header">
        <h2>Analytics Overview</h2>
        <p>Performance metrics and trends</p>
      </div>
      
      <div className="charts-grid">
        {/* User Growth Chart */}
        <div className="chart-card">
          <div className="chart-header">
            <h3>User Growth</h3>
            <p>Monthly user registrations</p>
          </div>
          <ChartContainer config={chartConfig} className="chart-container">
            <LineChart data={userGrowthData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Line 
                type="monotone" 
                dataKey="users" 
                stroke="var(--color-users)" 
                strokeWidth={2}
                dot={{ fill: "var(--color-users)" }}
              />
            </LineChart>
          </ChartContainer>
        </div>

        {/* Portfolio Statistics */}
        <div className="chart-card">
          <div className="chart-header">
            <h3>Portfolio Statistics</h3>
            <p>Published vs Draft portfolios</p>
          </div>
          <ChartContainer config={chartConfig} className="chart-container">
            <BarChart data={portfolioData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Bar dataKey="published" fill="var(--color-published)" />
              <Bar dataKey="draft" fill="var(--color-draft)" />
            </BarChart>
          </ChartContainer>
        </div>

        {/* Revenue Chart */}
        <div className="chart-card chart-card-wide">
          <div className="chart-header">
            <h3>Revenue Trends</h3>
            <p>Monthly revenue in USD</p>
          </div>
          <ChartContainer config={chartConfig} className="chart-container">
            <AreaChart data={revenueData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Area 
                type="monotone" 
                dataKey="revenue" 
                stroke="var(--color-revenue)" 
                fill="var(--color-revenue)"
                fillOpacity={0.3}
              />
            </AreaChart>
          </ChartContainer>
        </div>
      </div>
    </div>
  );
};

export default AdminCharts;
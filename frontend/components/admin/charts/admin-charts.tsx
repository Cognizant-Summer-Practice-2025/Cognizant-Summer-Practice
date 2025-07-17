"use client"

import React from 'react';
import { 
  ChartContainer, 
  ChartTooltip, 
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
  type ChartConfig 
} from '@/components/ui/chart';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { 
  Bar, 
  BarChart, 
  Line, 
  LineChart, 
  Area,
  AreaChart,
  Pie,
  PieChart,
  Cell,
  XAxis, 
  YAxis, 
  CartesianGrid,
  ResponsiveContainer
} from 'recharts';

// Sample data for different charts
const userGrowthData = [
  { month: 'Jan', users: 400, portfolios: 240 },
  { month: 'Feb', users: 600, portfolios: 380 },
  { month: 'Mar', users: 800, portfolios: 520 },
  { month: 'Apr', users: 1000, portfolios: 680 },
  { month: 'May', users: 1200, portfolios: 850 },
  { month: 'Jun', users: 1247, portfolios: 892 },
];

const projectTypesData = [
  { type: 'Web Development', count: 1456, fill: '#3b82f6' },
  { type: 'Mobile Apps', count: 890, fill: '#1d4ed8' },
  { type: 'Design', count: 678, fill: '#1e40af' },
  { type: 'Data Science', count: 432, fill: '#1e3a8a' },
];

const dailyActivityData = [
  { day: 'Mon', logins: 145, posts: 23 },
  { day: 'Tue', logins: 132, posts: 18 },
  { day: 'Wed', logins: 178, posts: 35 },
  { day: 'Thu', logins: 156, posts: 28 },
  { day: 'Fri', logins: 189, posts: 42 },
  { day: 'Sat', logins: 98, posts: 15 },
  { day: 'Sun', logins: 87, posts: 12 },
];

// Chart configurations with blue theme
const userGrowthConfig = {
  users: {
    label: "Users",
    color: "#3b82f6", // Blue-500
  },
  portfolios: {
    label: "Portfolios", 
    color: "#1e40af", // Blue-700
  },
} satisfies ChartConfig;

const projectTypesConfig = {
  web: {
    label: "Web Development",
    color: "#3b82f6", // Blue-500
  },
  mobile: {
    label: "Mobile Apps",
    color: "#1d4ed8", // Blue-600
  },
  design: {
    label: "Design",
    color: "#1e40af", // Blue-700
  },
  data: {
    label: "Data Science",
    color: "#1e3a8a", // Blue-800
  },
} satisfies ChartConfig;

const activityConfig = {
  logins: {
    label: "Daily Logins",
    color: "#3b82f6", // Blue-500
  },
  posts: {
    label: "New Posts",
    color: "#60a5fa", // Blue-400
  },
} satisfies ChartConfig;

export const UserGrowthChart: React.FC = () => {
  return (
    <Card>
      <CardHeader>
        <CardTitle>User Growth</CardTitle>
        <CardDescription>Monthly user and portfolio growth</CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={userGrowthConfig}>
          <LineChart accessibilityLayer data={userGrowthData}>
            <CartesianGrid vertical={false} />
            <XAxis
              dataKey="month"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
            />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={8}
            />
            <ChartTooltip cursor={false} content={<ChartTooltipContent />} />
            <ChartLegend content={<ChartLegendContent />} />
            <Line
              dataKey="users"
              type="monotone"
              stroke="var(--color-users)"
              strokeWidth={2}
              dot={false}
            />
            <Line
              dataKey="portfolios"
              type="monotone"
              stroke="var(--color-portfolios)"
              strokeWidth={2}
              dot={false}
            />
          </LineChart>
        </ChartContainer>
      </CardContent>
    </Card>
  );
};

export const ProjectTypesChart: React.FC = () => {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Project Distribution</CardTitle>
        <CardDescription>Projects by category</CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={projectTypesConfig}>
          <PieChart>
            <ChartTooltip
              cursor={false}
              content={<ChartTooltipContent hideLabel />}
            />
            <Pie
              data={projectTypesData}
              dataKey="count"
              nameKey="type"
              innerRadius={60}
              strokeWidth={5}
            >
              {projectTypesData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.fill} />
              ))}
            </Pie>
            <ChartLegend
              content={<ChartLegendContent nameKey="type" />}
              className="-translate-y-2 flex-wrap gap-2 [&>*]:basis-1/4 [&>*]:justify-center"
            />
          </PieChart>
        </ChartContainer>
      </CardContent>
    </Card>
  );
};

export const DailyActivityChart: React.FC = () => {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Weekly Activity</CardTitle>
        <CardDescription>Daily logins and new posts</CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={activityConfig}>
          <BarChart accessibilityLayer data={dailyActivityData}>
            <CartesianGrid vertical={false} />
            <XAxis
              dataKey="day"
              tickLine={false}
              tickMargin={10}
              axisLine={false}
            />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={8}
            />
            <ChartTooltip
              cursor={false}
              content={<ChartTooltipContent indicator="dashed" />}
            />
            <ChartLegend content={<ChartLegendContent />} />
            <Bar dataKey="logins" fill="var(--color-logins)" radius={4} />
            <Bar dataKey="posts" fill="var(--color-posts)" radius={4} />
          </BarChart>
        </ChartContainer>
      </CardContent>
    </Card>
  );
};

export const TrendChart: React.FC = () => {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Growth Trend</CardTitle>
        <CardDescription>User acquisition over time</CardDescription>
      </CardHeader>
      <CardContent>
        <ChartContainer config={userGrowthConfig}>
          <AreaChart accessibilityLayer data={userGrowthData}>
            <CartesianGrid vertical={false} />
            <XAxis
              dataKey="month"
              tickLine={false}
              axisLine={false}
              tickMargin={8}
            />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={8}
            />
            <ChartTooltip cursor={false} content={<ChartTooltipContent />} />
            <defs>
              <linearGradient id="fillUsers" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="var(--color-users)" stopOpacity={0.8}/>
                <stop offset="95%" stopColor="var(--color-users)" stopOpacity={0.1}/>
              </linearGradient>
            </defs>
            <Area
              dataKey="users"
              type="natural"
              fill="url(#fillUsers)"
              fillOpacity={0.4}
              stroke="var(--color-users)"
              stackId="a"
            />
          </AreaChart>
        </ChartContainer>
      </CardContent>
    </Card>
  );
}; 
"use client"

import * as React from "react"
import * as RechartsPrimitive from "recharts"
import { cn } from "@/lib/utils"

const ChartContainer = React.forwardRef<
  HTMLDivElement,
  React.ComponentProps<"div"> & {
    config: Record<string, any>
    children: React.ComponentProps<typeof RechartsPrimitive.ResponsiveContainer>["children"]
  }
>(({ id, className, children, config, ...props }, ref) => {
  const uniqueId = React.useId()
  const chartId = `chart-${id || uniqueId.replace(/:/g, "")}`

  return (
    <div
      data-chart={chartId}
      ref={ref}
      className={cn(
        "flex aspect-video justify-center text-xs",
        className
      )}
      {...props}
    >
      <ChartStyle id={chartId} config={config} />
      <RechartsPrimitive.ResponsiveContainer>
        {children}
      </RechartsPrimitive.ResponsiveContainer>
    </div>
  )
})
ChartContainer.displayName = "Chart"

const ChartStyle = ({ id, config }: { id: string; config: Record<string, any> }) => {
  const colorConfig = Object.entries(config).filter(
    ([_, config]) => config.color
  )

  if (!colorConfig.length) {
    return null
  }

  return (
    <style
      dangerouslySetInnerHTML={{
        __html: `
[data-chart=${id}] {
  ${colorConfig
    .map(([key, itemConfig]) => {
      return `  --color-${key}: ${itemConfig.color};`
    })
    .join("\n")}
}
        `,
      }}
    />
  )
}

const ChartTooltip = RechartsPrimitive.Tooltip

const ChartTooltipContent = React.forwardRef<
  HTMLDivElement,
  React.ComponentProps<typeof RechartsPrimitive.Tooltip> &
    React.ComponentProps<"div">
>(({ active, payload, className, ...props }, ref) => {
  if (!active || !payload?.length) {
    return null
  }

  return (
    <div
      ref={ref}
      className={cn(
        "grid min-w-[8rem] items-start gap-1.5 rounded-lg border bg-white px-2.5 py-1.5 text-xs shadow-xl",
        className
      )}
      {...props}
    >
      {payload.map((item, index) => (
        <div key={index} className="flex items-center gap-1.5">
          <div
            className="h-2.5 w-2.5 rounded-sm"
            style={{ backgroundColor: item.color }}
          />
          <span className="font-medium">{item.name}:</span>
          <span>{item.value}</span>
        </div>
      ))}
    </div>
  )
})
ChartTooltipContent.displayName = "ChartTooltipContent"

type ChartConfig = {
  [k in string]: {
    label?: React.ReactNode
    color?: string
  }
}

export {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
}
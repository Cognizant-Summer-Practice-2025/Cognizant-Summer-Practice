import React, { useEffect, useState } from 'react';

interface AnimatedNumberProps {
  value: number | string;
  duration?: number;
  className?: string;
  style?: React.CSSProperties;
}

interface AnimatedProgressBarProps {
  percentage: number;
  duration?: number;
  style?: React.CSSProperties;
  className?: string;
}

export function AnimatedNumber({ 
  value, 
  duration = 2000, 
  className = '',
  style = {}
}: AnimatedNumberProps) {
  const [animatedValue, setAnimatedValue] = useState<number>(0);

  useEffect(() => {
    const numValue = typeof value === 'string' ? parseInt(value) || 0 : value;
    
    if (numValue === 0) {
      setAnimatedValue(0);
      return;
    }

    const steps = 60;
    const stepDuration = duration / steps;
    let currentStep = 0;

    const timer = setInterval(() => {
      currentStep++;
      const progress = currentStep / steps;
      const currentValue = Math.floor(numValue * progress);
      
      setAnimatedValue(currentValue);

      if (currentStep >= steps) {
        clearInterval(timer);
        setAnimatedValue(numValue);
      }
    }, stepDuration);

    return () => clearInterval(timer);
  }, [value, duration]);

  return (
    <span className={className} style={style}>
      {animatedValue}
    </span>
  );
}

export function AnimatedProgressBar({ 
  percentage, 
  duration = 2000, 
  style = {},
  className = ''
}: AnimatedProgressBarProps) {
  const [animatedPercentage, setAnimatedPercentage] = useState<number>(0);

  useEffect(() => {
    if (percentage === 0) {
      setAnimatedPercentage(0);
      return;
    }

    const steps = 60;
    const stepDuration = duration / steps;
    let currentStep = 0;

    const timer = setInterval(() => {
      currentStep++;
      const progress = currentStep / steps;
      const currentPercentage = Math.floor(percentage * progress);
      
      setAnimatedPercentage(currentPercentage);

      if (currentStep >= steps) {
        clearInterval(timer);
        setAnimatedPercentage(percentage);
      }
    }, stepDuration);

    return () => clearInterval(timer);
  }, [percentage, duration]);

  return (
    <div 
      className={className}
      style={{ 
        ...style,
        width: `${animatedPercentage}%`
      }}
    />
  );
}

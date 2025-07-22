'use client';

import React from 'react';
import { Button, Tooltip } from 'antd';
import { ArrowLeftOutlined, HomeOutlined } from '@ant-design/icons';
import { usePortfolioNavigation } from '@/lib/contexts/use-portfolio-navigation';
import './back-to-home-button.css';

interface BackToHomeButtonProps {
  className?: string;
  style?: React.CSSProperties;
  size?: 'small' | 'middle' | 'large';
  type?: 'primary' | 'default' | 'link' | 'text';
  showText?: boolean;
}

const BackToHomeButton: React.FC<BackToHomeButtonProps> = ({
  className = '',
  style,
  size = 'middle',
  type = 'default',
  showText = true
}) => {
  const { navigateBackToHome, hasReturnContext } = usePortfolioNavigation();

  const handleClick = () => {
    navigateBackToHome();
  };

  const buttonText = hasReturnContext() ? 'Back to Results' : 'Back to Home';
  const tooltipText = hasReturnContext() 
    ? 'Return to your previous search results and position'
    : 'Go back to the home page';

  return (
    <Tooltip title={tooltipText}>
      <Button
        icon={hasReturnContext() ? <ArrowLeftOutlined /> : <HomeOutlined />}
        onClick={handleClick}
        size={size}
        type={type}
        className={`back-to-home-button ${className}`}
        style={style}
      >
        {showText && buttonText}
      </Button>
    </Tooltip>
  );
};

export default BackToHomeButton;

import React from 'react';
import { Button as AntButton } from 'antd';
import './button.module.css';

interface ButtonProps {
  text: string;
  onClick: () => void;
  className?: string;
}

const Button: React.FC<ButtonProps> = ({ text, onClick, className }) => {
  return (
    <AntButton onClick={onClick} className={className}>
      {text}
    </AntButton>
  );
};

export default Button;
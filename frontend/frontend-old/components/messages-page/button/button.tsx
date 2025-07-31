import React from 'react';
import { Button as ShadcnButton } from '@/components/ui/button';

interface ButtonProps {
  text: string;
  onClick: () => void;
  className?: string;
  children?: React.ReactNode;
}

const Button: React.FC<ButtonProps> = ({ text, onClick, className, children }) => {
  return (
    <ShadcnButton onClick={onClick} className={className}>
      {children || text}
    </ShadcnButton>
  );
};

export default Button;
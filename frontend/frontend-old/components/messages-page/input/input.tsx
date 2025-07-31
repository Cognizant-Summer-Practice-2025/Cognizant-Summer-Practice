import React from 'react';
import { Input as ShadcnInput } from '@/components/ui/input';

interface InputProps {
  placeholder?: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  className?: string;
}

const Input: React.FC<InputProps> = ({ placeholder, value, onChange, className }) => {
  return (
    <ShadcnInput 
      placeholder={placeholder} 
      value={value} 
      onChange={onChange} 
      className={className || 'w-full'}
    />
  );
};

export default Input;
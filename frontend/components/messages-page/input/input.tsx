import React from 'react';
import { Input as AntInput } from 'antd';
import 'antd/dist/antd.css';

interface InputProps {
  placeholder?: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const Input: React.FC<InputProps> = ({ placeholder, value, onChange }) => {
  return (
    <AntInput 
      placeholder={placeholder} 
      value={value} 
      onChange={onChange} 
      style={{ width: '100%' }} 
    />
  );
};

export default Input;
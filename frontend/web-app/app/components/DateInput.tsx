import 'react-datepicker/dist/react-datepicker.css';

import React from 'react';
import { useController, UseControllerProps } from 'react-hook-form';
import DatePicker, { ReactDatePickerProps } from 'react-datepicker';

type Props = {
  label: string;
} & UseControllerProps & Partial<ReactDatePickerProps>;

export default function DateInput(props: Props) {
  const { field, fieldState } = useController({...props, defaultValue: ""});

  return (
    <div className='block'>
      <DatePicker 
        {...props}
        {...field} 
        selected={field.value} 
        onChange={(date) => field.onChange(date)}
        placeholderText={props.label}
        className={`
          rounded-lg w-[100%] flex flex-col 
          ${fieldState.error 
            ? 'bg-red-50 border-red-500 text-red-900' 
            : (!fieldState.invalid && fieldState.isDirty) ? 'bg-green-50 border-green-500 text-green-900': ''}
        `}
      />
      {fieldState.error && (
        <div className='text-red-500 text-sm'>{fieldState.error?.message}</div>
      )}
    </div>
  );  
}

"use client";

import React, { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useForm, SubmitHandler, FieldValues } from 'react-hook-form';
import { Button } from 'flowbite-react';
import Input from '../components/Input';
import DateInput from '../components/DateInput';
import { createAuction, updateAuction } from '../actions/auctionActions';
import toast from 'react-hot-toast';
import { Auction } from '@/types';

type Props = {
  auction?: Auction;
}

export default function AuctionForm({auction}: Props) {
  const router = useRouter();
  const {
    handleSubmit,
    setFocus,
    control,
    reset,
    formState: { isSubmitting, isValid },
  } = useForm({
    mode: "onTouched"
  })

  const isCreate = !auction;

  useEffect(() => {
    //console.log("Focus")
    setFocus("make");
    if(auction) {
      const {make, model, year, mileage, color} = auction;
      reset({make, model, year, mileage, color});
    }
  },[setFocus, auction, reset]);

  const onSubmit = async (data: FieldValues) => { 
    try {
      let id = auction?.id ?? "";
      let res: any;
      if(isCreate) {
        res = await createAuction(data);
        id = res.id; 
      } else {
        res = await updateAuction(id, data);
      }
      if(Object.hasOwn(res, "error")) throw res.error;
      
      //redirect to newly created auction
      router.push(`/auctions/details/${id}`);
    } catch(e: any) {
      toast.error(`${e.status} ${e.message}`);      
    }
  }  

  return (
    <form className='flex flex-col mt-3' onSubmit={handleSubmit(onSubmit)}>
      <Input name='make' label='Make' control={control} rules={{ required:"Model is required" }}/>
      <Input name='model' label='Model' control={control} rules={{ required: "Model is required" }}/> 
      <Input name='color' label='Color' control={control} rules={{ required: "Color is required" }}/> 
      <div className='grid grid-cols-2 gap-3'>
        <Input name='year' label='Year' control={control} type="number" rules={{ required: "Year is required" }}/> 
        <Input name='mileage' label='Mileage' control={control} type="number" rules={{ required: "Mileage is required" }}/> 
      </div>

      { isCreate && (
        <>
          <Input name='imageUrl' label='ImageUrl' control={control} 
            rules={{ required: "Image URL is required" }}/> 
          
          <div className='grid grid-cols-2 gap-3'>
            <Input name='reservePrice' label='Reserve Price (enter 0 if no reserve)' control={control} type="number" rules={{ required: "Reserve Price is required" }}/> 
            <DateInput 
              name='auctionEnd' 
              label='Auction end date/time' 
              dateFormat='dd MMMM yyyy h:mm a'
              showTimeSelect
              control={control} 
              rules={{ required: "Auction end date/time is required" }}
            /> 
          </div>

        </>
      )}

      <div className='flex justify-between'>
        <Button type="reset" outline className='gray'>Cancel</Button>
        <Button 
          type="submit" 
          outline 
          className='success' 
          isProcessing={isSubmitting}
          disabled={!isValid}
        >
          Submit
        </Button>
      </div>
    </form>
  )
}

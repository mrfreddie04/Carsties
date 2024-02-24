"use client";

import React from 'react';
import { useForm, FieldValues } from 'react-hook-form';
import { useBidStore } from '@/hooks/useBidStore';
import { placeBidForAuction } from '@/app/actions/auctionActions';
import toast from 'react-hot-toast';
import { numberWithCommas } from '@/app/utils/utils';
import { Bid, FetchError } from '@/types';

type Props = {
  auctionId: string;
  highBid: number;
}

export default function BidForm({auctionId, highBid}: Props) {
  const {
    handleSubmit,
    register,
    reset,
    formState: { errors },
  } = useForm();

  const addBid = useBidStore(state => state.addBid);  

  const onSubmit = (data: FieldValues) => {
    const amount = +data["amount"];

    if(amount <= highBid) {
      reset();  
      return toast.error(`Bid must be at least $${numberWithCommas(highBid+1)}` );
    }

    placeBidForAuction(auctionId, amount)
      .then( (res: Bid | FetchError) => {
        if("error" in res) throw res.error;
        //console.log("ADDBID",res);
        addBid(res);  //update state
        reset();      //reset form
      })
      .catch( err => {
        toast.error(err.message);
      })
  } 

  return (
    <form className='flex items-center border-2 rounded-lg py-2' onSubmit={handleSubmit(onSubmit)}>
      <input
        type="number"
        className='input-custom text-sm text-gray-600'
        {...register("amount", { required: "Bid amount is required"})}
        placeholder={`Enter you bid (minimum bid is $${numberWithCommas(highBid+1)})`}
      />     
    </form>  
  )
}


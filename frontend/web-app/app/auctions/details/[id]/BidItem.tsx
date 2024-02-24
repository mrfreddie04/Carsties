import React from 'react';
import { format } from 'date-fns';
import { numberWithCommas } from '@/app/utils/utils';
import { Bid } from '@/types';

type Props = {
  bid: Bid;
};

const getBidInfo = (status: string) => {
  let bgColor = "";
  let text = "";
  switch(status) {
    case "Accepted":
      bgColor = "bg-green-200";
      text = "Bid accepted";
      break;
    case "AcceptedBelowReserve":
      bgColor = "bg-amber-500";
      text = "Reserver not met";
      break;        
    case "TooLow":
      bgColor = "bg-red-200";
      text = "Bid was too low";
      break;   
    default:      
      bgColor = "bg-red-200";
      text = "Bid placed after auction finished";          
  }
  return {bgColor, text}
};

export default function BidItem({bid}: Props) {

  const {text, bgColor} = getBidInfo(bid.bidStatus);

  return (
    <div className={`
      border-gray-300 border-2 px-3 py-2 rounded-lg 
      flex justify-between items-center mb-2
      ${bgColor}
    `}>
      <div className='flex flex-col'>
        <span>Bidder: {bid.bidder}</span>
        <span className='text-gray-700 text-sm'>Time: {format(bid.bidTime, "dd MMM yyyy h:mm a")}</span>
      </div>
      <div className='flex flex-col text-right'>
        <div className='text-xl font-semibold'>${numberWithCommas(bid.amount)}</div>
        <div className='flex flex-row items-center'>
          <span>{text}</span>          
        </div>
      </div>
    </div>
  )
}

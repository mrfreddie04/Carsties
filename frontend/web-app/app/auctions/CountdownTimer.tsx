"use client";

import React from 'react';
import Countdown, { zeroPad } from 'react-countdown'; 
import { useBidStore } from '@/hooks/useBidStore';
import { useShallow } from 'zustand/react/shallow';
import { usePathname } from 'next/navigation';

type Props = {
  auctionEnd: string;
}

type CountdownStatus = {
  days: number,
  hours: number, 
  minutes: number, 
  seconds: number, 
  completed: boolean
}

const renderer = ({ days, hours, minutes, seconds, completed }: CountdownStatus) => {
  return (
    <div className={`
      border-2 border-white text-white py-1 px-2 rounded-lg flex justify-center
      ${completed ? "bg-red-600": (days === 0 && hours < 10 ? "bg-amber-600" : "bg-green-600")}
    `}>
      { completed ? 
        (<span>Auction finished</span>) : 
        (
          <span suppressHydrationWarning={true}>
            {zeroPad(days)}:{zeroPad(hours,2)}:{zeroPad(minutes,2)}:{zeroPad(seconds,2)}
          </span>
        ) 
      }
    </div>
  );
};

export default function CountdownTimer({auctionEnd}: Props) {
  const open = useBidStore(useShallow(state => state.open));
  const setOpen = useBidStore(state => state.setOpen);
  const pathname = usePathname();

  const auctionFinished = () => {
    if(pathname.startsWith("/auctions/details") && open) setOpen(false);
  }

  return (
    <div>
      <Countdown 
        date={new Date(auctionEnd)}
        renderer={renderer}
        onComplete={auctionFinished} 
      />    
    </div>
  )
}

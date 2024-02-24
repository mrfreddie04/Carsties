import React from 'react'
import Link from 'next/link';
import CountdownTimer from "./CountdownTimer";
import CarImage from './CarImage';
import { Auction } from '@/types';
import CurrentBid from './CurrentBid';

type Props = {
  auction: Auction;
}

export default function AuctionCard({auction}: Props) {
  return (
    <Link href={`/auctions/details/${auction.id}`} className='group'>
      <div className='w-full bg-gray-200 aspect-w-16 aspect-h-10 rounded-lg overflow-hidden shadow-lg'>
        <div >
          <CarImage imageUrl={auction.imageUrl}/>
          <div className='absolute left-2 bottom-2'>
            <CountdownTimer auctionEnd={auction.auctionEnd}/>
          </div>
          <div className='absolute right-2 top-2'>
            <CurrentBid reservePrice={auction.reservePrice} amount={auction.currentHighBid}/>
          </div>          
        </div>       
      </div>
      <div className='flex justify-between items-center mt-4'>
        <h3 className='text-gray-700'>{auction.make} {auction.model}</h3>
        <p className='font-semibold text-sm'>{auction.year}</p>
      </div>
    </Link>
  )
}

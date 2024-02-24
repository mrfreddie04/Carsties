'use client';

import React, { useEffect, useState } from 'react';
import { Session } from 'next-auth';
import { useShallow } from 'zustand/react/shallow';
import { useBidStore } from '@/hooks/useBidStore';
import { getBidsForAuction } from '@/app/actions/auctionActions';
import { Auction } from '@/types';
import BidItem from './BidItem';
import toast from 'react-hot-toast';
import Heading from '@/app/components/Heading';
import { numberWithCommas } from '@/app/utils/utils';
import EmptyFilter from '@/app/components/EmptyFilter';
import BidForm from './BidForm';

type Props = {
  auction: Auction;
  user: Session['user'] | null;
}

export default function BidList({auction, user}: Props) {
  const [loading, setLoading] = useState(false);
  
  const {bids,open} = useBidStore(useShallow(state => ({bids:state.bids,open:state.open})));
  const setBids = useBidStore(state => state.setBids);  
  const setOpen = useBidStore(state => state.setOpen);

  const openForBids = new Date(auction.auctionEnd) > new Date();

  const highBid = bids.filter(bid => bid.bidStatus.includes("Accepted"))
    .reduce((prev,bid) => Math.max(bid.amount,prev), 0);

  useEffect(() => {
    setLoading(true);
    getBidsForAuction(auction.id)
      .then( res => {
        if("error" in res) throw res.error;
        setBids(res);
      })
      .catch( err => {
        toast.error(err.message);
      })
      .finally(() => setLoading(false));
  }, [auction.id]);

  useEffect(() => {
    setOpen(openForBids);
  },[openForBids]);

  if(loading) return (<span>Loading bids...</span>);


  return (
    <div className='rounded-lg shadow-md'>
      <div className='py-2 px-4 bg-white'>
        <div className='sticky top-0 bg-white p-2'>
          <Heading title={`Current high bid is $${numberWithCommas(highBid)}`}/>
        </div>
      </div>

      <div className='overflow-auto h-[400px] flex flex-col-reverse px-2'>
        {bids.length === 0 ? (
          <EmptyFilter title="No bids for this item" subtitle="Please feel free to make a bid"/>
        ) : (
          <>
            {bids.map(bid => (
              <BidItem key={bid.id} bid={bid}/>
            ))}
          </>
        )}
      </div>

      <div className='px-2 pb-2 text-gray-500'>
        { !open && (
            <div className='flex items-center justify-center p-2 text-lg font-semibold'>
              This auction has finished
            </div>
          ) ||
          !user && (
            <div className='flex items-center justify-center p-2 text-lg font-semibold'>
              Please log in to place a bid
            </div>
          ) ||
          !!user && auction.seller === user.username && (
            <div className='flex items-center justify-center p-2 text-lg font-semibold'>
              You cannot bid on your own auction
            </div>
          ) ||
          // auction.status !== "Live" && (
          //   <div className='flex items-center justify-center p-2 text-lg font-semibold'>
          //     The auction has finished
          //   </div>
          // ) || 
          (
            <BidForm auctionId={auction.id} highBid={highBid}/>
          )
        }        
      </div>
    </div>
  );
}

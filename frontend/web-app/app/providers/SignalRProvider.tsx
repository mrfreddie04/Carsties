"use client";

import React, { ReactNode, useEffect, useState } from 'react'
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { usePathname } from 'next/navigation'
import { useAuctionStore } from '@/hooks/useAuctionStore';
import { useBidStore } from '@/hooks/useBidStore';
import { Auction, AuctionFinished, Bid } from '@/types';
import { Session } from 'next-auth';
import toast from 'react-hot-toast';
import AuctionCreatedToast from '../components/AuctionCreatedToast';
import AuctionFinishedToast from '../components/AuctionFinishedToast';
import { getDetailedViewData } from '../actions/auctionActions';

type Props = {
  children: ReactNode;
  user: Session['user'] | null;
}

let pathname = "";

export default function SignalRProvider({children,user}: Props) {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
  const addBid = useBidStore(state => state.addBid);  

  const apiUrl = process.env.NODE_ENV === "production" 
    ? "https://api.carsties.com/notifications" 
    : process.env.NEXT_PUBLIC_NOTIFY_URL;

  pathname = usePathname();

  // console.log("NEXT_PUBLIC_NOTIFY_URL", process.env.NEXT_PUBLIC_NOTIFY_URL);
  // console.log("NODE_ENV", process.env.NODE_ENV);
  // console.log("apiUrl", apiUrl);

  useEffect(() => {
    //create connection to SignalR Hub
    const newConnection = new HubConnectionBuilder()
      .withUrl(apiUrl!)
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  },[apiUrl]);

  useEffect(() => {
    //connect & subscribe to methods  
    if(connection) {
      connection.start()
        .then( () => {
          console.log("Connected to notification hub");

          connection.on("BidPlaced", (bid: Bid) => {
            if(bid.bidStatus.includes("Accepted")) {
              setCurrentPrice(bid.auctionId, bid.amount);
            }
            if(pathname.includes(bid.auctionId)) {
              //console.log("addBid",pathname);
              addBid(bid);
            }
          });

          connection.on("AuctionCreated", (auction: Auction) => {
            //console.log("AuctionCreated", auction.seller, user?.username);
            if(auction.seller !== user?.username) {
              return toast(<AuctionCreatedToast auction={auction}/>, {duration: 5000});
            }
          });
          
          connection.on("AuctionFinished", (auctionFinished: AuctionFinished) => {
            const auctionPromise = getDetailedViewData(auctionFinished.auctionId);
            return toast.promise(auctionPromise,{
              loading: 'Loading',
              error: (err) => 'Auction finished!',
              success: (auction) =>
                <AuctionFinishedToast auction={auction} auctionFinished={auctionFinished} />
            }, {success: {duration: 5000, icon: null}});
          });                

        }).catch(err => {
          console.log(err);
        });
    }

    return () => { 
      if(connection) connection.stop();
    }
  },[connection, addBid, setCurrentPrice, user?.username])  
  
  return (
    children
  )
}

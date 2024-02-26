"use server";

import { PagedResult, Auction, AuctionUpdate, Bid, FetchError } from '@/types';
import { fetchWrapper } from '@/app/lib/fetchWrapper';
import { FieldValues } from 'react-hook-form';
import { revalidatePath } from 'next/cache';

export async function getData(query: string): Promise<PagedResult<Auction>> {
  return await fetchWrapper.get(`search${query}`);
}

export async function getDetailedViewData(id: string): Promise<Auction> {
  return await fetchWrapper.get(`auctions/${id}`);
}

export async function getBidsForAuction(id: string): Promise<Bid[] | FetchError> {
  return await fetchWrapper.get(`bids/${id}`);
}

export async function placeBidForAuction(auctionID: string, amount: number): Promise<Bid | FetchError>  {
  return await fetchWrapper.post(`bids?auctionID=${auctionID}&amount=${amount}`,{});
}

export async function createAuction(data: FieldValues) {
  return await fetchWrapper.post("auctions", data);
}

export async function updateAuction(id: string, data: FieldValues) {
  const res = await fetchWrapper.put("auctions", id, data);
  revalidatePath(`/auctions/${id}`);
  return res;
}

export async function deleteAuction(id: string) {
  return await fetchWrapper.del("auctions", id);
}

export async function updateAuctionTest() {
  const id: string = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
  const data: Partial<AuctionUpdate> = {
    mileage: Math.floor(Math.random() * 100000) + 1
  };

  return await fetchWrapper.put("auctions", id, data);
}


// export async function updateAuctionTest() {
//   const id: string = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
//   const data: Partial<AuctionUpdate> = {
//     mileage: Math.floor(Math.random() * 100000) + 1
//   };

//   const token = await getTokenWorkaround();

//   const res = await fetch(`http://localhost:6001/auctions/${id}`, {
//     method: "PUT",
//     headers: {
//       "Content-Type": "application/json",
//       "Authorization": `Bearer ${token?.access_token}`
//     },
//     body: JSON.stringify(data),
//   });
  
//   if(!res.ok) return {status: res.status, message: res.statusText};

//   return res.statusText;
// }

// export async function getData(query: string): Promise<PagedResult<Auction>> {
//   const res = await fetch(`http://localhost:6001/search${query}`);
  
//   if(!res.ok) throw new Error("Failed to fetch data");

//   return await res.json(); //returns the body of the response
// }
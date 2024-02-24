'use client';

import React, { useEffect, useState } from 'react';
import { useShallow } from 'zustand/react/shallow'
import qs from 'query-string';
import AuctionCard from './AuctionCard';
import { Auction, FetchError, PagedResult, isFetchError } from '@/types';
import { useParamsStore } from '@/hooks/useParamsStore';
import { useAuctionStore } from '@/hooks/useAuctionStore';
import AppPagination from '../components/AppPagination';
import { getData } from '../actions/auctionActions';
import Filters from './Filters';
import EmptyFilter from '../components/EmptyFilter';

export default function Listings() {
  //const [data, setData] = useState<PagedResult<Auction> | FetchError>();
  const [loading, setLoading] = useState(false);
  const data = useAuctionStore(useShallow(state => ({
    auctions: state.auctions, 
    totalCount: state.totalCount, 
    pageCount: state.pageCount
  })));
  const setData = useAuctionStore(state => state.setData);

  const params = useParamsStore(useShallow(state => ({
    pageNumber: state.pageNumber, 
    pageSize: state.pageSize, 
    orderBy: state.orderBy,
    filterBy: state.filterBy,
    searchTerm: state.searchTerm,
    seller: state.seller,
    winner: state.winner
  })));
  const setParams = useParamsStore(state => state.setParams);
  const query = qs.stringifyUrl({
    url: "", query: params
  });

  //Object.values(params).map( ([k,v]) => `${k}=${v}`).join('&');

  const setPageNumber = (pageNumber: number) => setParams({pageNumber});

  useEffect(() => {
    setLoading(true);
    getData(query)
      .then( data => setData(data))
      .finally(() => setLoading(false));
  }, [query]);

  if(loading) return (<h3>Loading...</h3>)

  if(isFetchError(data)) return (<h3>Error...</h3>)
  
  return (
    <>
      <Filters />
      { data.totalCount === 0 ? (
        <EmptyFilter showReset={true}/>
        ) : (
        <>
          <div className='grid grid-cols-4 gap-6'>
            { data.auctions.map( (auction) => (
              <AuctionCard key={auction.id} auction={auction}/>
            ))}
          </div>
          <div className="flex justify-center mt-4">
            <AppPagination pageCount={data.pageCount} currentPage={params.pageNumber} 
              onPageChage={setPageNumber}
            />    
          </div>
        </>  
      )}
    </>
  )
}

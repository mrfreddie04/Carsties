'use client';

import React, { useEffect, useState } from 'react';
import { useShallow } from 'zustand/react/shallow'
import qs from 'query-string';
import AuctionCard from './AuctionCard';
import { Auction, PagedResult } from '@/types';
import { useParamsStore } from '@/hooks/useParamsStore';
import AppPagination from '../components/AppPagination';
import { getData } from '../actions/auctionActions';
import Filters from './Filters';
import EmptyFilter from '../components/EmptyFilter';

export default function Listings() {
  const [data, setData] = useState<PagedResult<Auction>>();
  const params = useParamsStore(useShallow(state => ({
    pageNumber: state.pageNumber, 
    pageSize: state.pageSize, 
    orderBy: state.orderBy,
    filterBy: state.filterBy,
    searchTerm: state.searchTerm
  })));
  const setParams = useParamsStore(state => state.setParams);
  const query = qs.stringifyUrl({
    url: "", query: params
  });

  const setPageNumber = (pageNumber: number) => setParams({pageNumber});

  useEffect(() => {
    const fetchData = async () => {
      const result = await getData(query);
      setData(result);
    }
    fetchData();
  }, [query]);

  if(!data) return (<h3>Loading...</h3>)
  
  return (
    <>
      <Filters />
      { data.totalCount === 0 ? (
        <EmptyFilter showReset={true}/>
        ) : (
        <>
          <div className='grid grid-cols-4 gap-6'>
            { data.results.map( (auction) => (
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

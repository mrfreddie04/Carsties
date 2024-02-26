import React from 'react'
import { getDetailedViewData } from '@/app/actions/auctionActions';
import Heading from '@/app/components/Heading';
import AuctionForm from '../../AuctionForm';

type Props = {
  params: {id:string};
};

export default async function Update({params}: Props) {
  const data = await getDetailedViewData(params.id);
  return (
    <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
      <Heading title="Update your auction!" subtitle="Please update the details of your car"/>
      <AuctionForm auction={data}/>
    </div>
  )
}


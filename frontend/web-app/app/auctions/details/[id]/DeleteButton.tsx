"use client";

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { Button } from 'flowbite-react';
import { deleteAuction } from '@/app/actions/auctionActions';

type Props = {
  id: string
}

export default function DeleteButton({id}: Props) {
  const [isLoading, setIsLoading] = useState(false);
  const router = useRouter();

  const handleDelete = async () => {
    try {
      setIsLoading(true);
      const res = await deleteAuction(id);
      if(res.error) throw res.error;
      router.push("/");
    } catch(e: any) {
      toast.error(`${e.status} ${e.message}`);      
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <Button color='failure' onClick={handleDelete} isProcessing={isLoading}>
      Delete Auction
    </Button>
  );
}

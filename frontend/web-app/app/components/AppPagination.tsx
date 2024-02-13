'use client';

import React from 'react';
import { Pagination } from 'flowbite-react';

type Props = {
  pageCount: number,
  currentPage: number,
  onPageChage: (page: number) => void
}

export default function AppPagination({pageCount, onPageChage, currentPage}: Props) {
  return (
    <Pagination 
      currentPage={currentPage} 
      totalPages={pageCount} 
      onPageChange={(page: number) => onPageChage(page)} 
      layout='pagination'
      showIcons={true}
      className='text-blue-500 mb-5'
    />
  )
}

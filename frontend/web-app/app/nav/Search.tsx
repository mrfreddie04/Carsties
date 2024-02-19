'use client';

import React from 'react'
import { FaSearch } from "react-icons/fa";
import { useParamsStore } from '@/hooks/useParamsStore';
import { useShallow } from 'zustand/react/shallow';
import { usePathname, useRouter } from 'next/navigation';

export default function Search() {  
  const router = useRouter();
  const pathName = usePathname();
  const {
    searchValue,
    setSearchValue,
    setParams
  } = useParamsStore(useShallow(state => ({
    searchValue: state.searchValue,
    setSearchValue: state.setSearchValue,
    setParams: state.setParams
  })));

  const search = () => {
    setParams({searchTerm:searchValue});
    if(pathName !== "/") router.push("/");
  } 
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => setSearchValue(e.target.value);
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if(e.key === "Enter") search();
  }
    
  return (
    <div className='flex w-[50%] items-center border-2 rounded-full py-2 shadow-sm'>
      <input 
        type="text" 
        placeholder="Search for cars by make, model, or color"
        value={searchValue} 
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        className='
        flex-grow pl-5 bg-transparent focus:outline-none 
        border-transparent focus:border-transparent
        focus:ring-0
        text-sm
        text-gray-600
        '
      />
      <button onClick={search}>
        <FaSearch size={35} className='bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2'/>
      </button>
    </div>
  )
}

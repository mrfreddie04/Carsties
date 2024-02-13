import React from 'react';
import { Button } from 'flowbite-react';
import { useShallow } from 'zustand/react/shallow'
import { AiOutlineClockCircle, AiOutlineSortAscending } from 'react-icons/ai';
import { BsFillStopCircleFill, BsStopwatch } from 'react-icons/bs';
import { GiFinishLine, GiFlame } from 'react-icons/gi';
import { useParamsStore } from '@/hooks/useParamsStore';

// type Props = {
//   pageSize: number;
//   setPageSize: (pageSize: number) => void;
// };

const pageSizeButtons = [4, 8, 12];
const orderButtons = [
  {label: "Alphabetical", icon: AiOutlineSortAscending, value: "make"}, 
  {label: "End date", icon: AiOutlineClockCircle, value: "endingSoon"},
  {label: "Recently Added", icon: BsFillStopCircleFill, value: "new"},
];
const filterButtons = [
  {label: "Live Auctions", icon: GiFlame, value: "live"},
  {label: "Ending < 6 hours", icon: GiFinishLine, value: "endingSoon"},
  {label: "Completed", icon: BsStopwatch, value: "finished"}, 
];

export default function Filters() {
  const { pageSize, orderBy, filterBy } = useParamsStore(useShallow(state => ({
    pageSize: state.pageSize,
    orderBy: state.orderBy,
    filterBy: state.filterBy
  })));
  const setParams = useParamsStore(state => state.setParams);
  const setPageSize = (pageSize: number) => setParams({pageSize});
  const setOrderBy = (orderBy: string) => setParams({orderBy});
  const setFilterBy = (filterBy: string) => setParams({filterBy});  

  return (
    <div className='flex justify-between items-center mb-4'>
      <div>
        <span className='uppercase text-sm text-gray-500 mr-2'>Filter by</span>
        <Button.Group>
          {filterButtons.map( ({label, icon: Icon, value}) => (
            <Button 
              key={value} 
              onClick={() => setFilterBy(value)}
              color={value === filterBy ? "red" : "gray"}
              className='focus:ring-0'
            >
              <Icon className='mr-3 h-4 w-4'/>
              {label}
            </Button>
          ))}
        </Button.Group>        
      </div>      
      <div>
        <span className='uppercase text-sm text-gray-500 mr-2'>Order by</span>
        <Button.Group>
          {orderButtons.map( ({label, icon: Icon, value}) => (
            <Button 
              key={value} 
              onClick={() => setOrderBy(value)}
              color={value === orderBy ? "red" : "gray"}
              className='focus:ring-0'
            >
              <Icon className='mr-3 h-4 w-4'/>
              {label}
            </Button>
          ))}
        </Button.Group>        
      </div>      
      <div>
        <span className='uppercase text-sm text-gray-500 mr-2'>Page size</span>
        <Button.Group>
          {pageSizeButtons.map( (size,index) => (
            <Button 
              key={index} 
              onClick={() => setPageSize(size)}
              color={size === pageSize ? "red" : "gray"}
              className='focus:ring-0'
            >
              {size}
            </Button>
          ))}
        </Button.Group>
      </div>
    </div>
  )
}

"use client";

import React from 'react';
import Link from 'next/link';
import { Session } from 'next-auth';
import { signOut } from 'next-auth/react';
import { usePathname, useRouter } from 'next/navigation';
import { Dropdown } from 'flowbite-react';
import { HiCog, HiUser } from 'react-icons/hi2';
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from 'react-icons/ai';
import { useParamsStore } from '@/hooks/useParamsStore';

type Props = {
  user: Session['user'];
}

export default function UserActions({user}: Props) {
  const setParams = useParamsStore(state => state.setParams);
  const router = useRouter();
  const pathName = usePathname();  

  const setSeller = () => {
    if(pathName !== "/") router.push("/");    
    setParams({seller:user.username, winner: undefined})
  }

  const setWinner = () => {
    if(pathName !== "/") router.push("/");
    setParams({winner:user.username, seller: undefined})
  }

  return (
    <Dropdown label={`Welcome ${user.name}`} inline>
      <Dropdown.Item icon={HiUser} onClick={setSeller}>
        My Auctions
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillTrophy} onClick={setWinner}>
        Auctions won
      </Dropdown.Item>
      <Dropdown.Item icon={AiFillCar}>
        <Link href="/auctions/create">Sell my car</Link>
      </Dropdown.Item>
      <Dropdown.Item icon={HiCog}>
        <Link href="/session">Session (dev only)</Link>
      </Dropdown.Item>
      <Dropdown.Divider/>
      <Dropdown.Item icon={AiOutlineLogout} onClick={() => signOut({callbackUrl: "/"})}>
        Sign out
      </Dropdown.Item>      
    </Dropdown>
  )
}



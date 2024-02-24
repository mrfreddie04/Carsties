//"use server";

import React from "react";
import { getCurrentUser } from "../actions/authActions";
import Search from "./Search";
import Logo from "./Logo";
import LoginButton from "./LoginButton";
import UserActions from "./UserActions";
import { Session } from "next-auth";

type Props = {
  user: Session['user'] | null;
}

export default function Navbar({user}: Props) {
  //console.log("Server component");
  //const user = await getCurrentUser();

  return (
    <header className='sticky top-0 z-50 flex justify-between bg-white p-5 items-center text-gray-800 shadow-md'>
      <Logo />
      <Search />
      {user ? (<UserActions user={user}/>) : (<LoginButton />)}      
    </header>
  );
}

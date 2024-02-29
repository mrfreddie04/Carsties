import { getServerSession } from "next-auth";
import { authOptions } from "../api/auth/[...nextauth]/authOptions";
import { getToken } from "next-auth/jwt";
import { cookies, headers } from "next/headers";
import { NextApiRequest } from "next";
import { DynamicServerError } from "next/dist/client/components/hooks-server-context";

//export type AppUser = User & { username: string};

//extract user info from session cookie
export async function getSession() {
  return await getServerSession(authOptions);
}

export async function getCurrentUser() {
  try {
    const session = await getSession();
    //console.log({session});
    if(!session) return null;
    return session.user;
  } catch(e) {
    if(e instanceof DynamicServerError) throw e;
    console.log(e);
    return null;
  }
}

export async function getTokenWorkaround() {
  const req = {
    headers: Object.fromEntries(headers() as Headers),
    cookies: Object.fromEntries(cookies().getAll().map( cookie => [cookie.name, cookie.value])),
  } as NextApiRequest;
  const token = await getToken({req});
  return token;
}